// <copyright file="PseudonymisationHelper.cs" company="Riziv-Inami">
// Copyright (c) Riziv-Inami. All rights reserved.
// </copyright>

namespace Nihdi.Common.Pseudonymisation;

using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Xml;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using Nihdi.Common.Pseudonymisation.Internal;
using Nihdi.Common.Pseudonymisation.Jose;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;

/// <summary>
/// Helper class for managing pseudonymisation domains and related operations.
/// </summary>
public sealed class PseudonymisationHelper
{
    private const string _kidProblemWithDomain =
        "Failed to decrypt the secret key with kid `{Kid}` of the domain `{DomainKey}`. " +
        "The response from eHealth was\n{RawDomain}";

    /// <summary>
    /// How long the helper waits for eHealth, or for the JWKS supplier, before giving up.
    /// </summary>
    private static readonly TimeSpan _eHealthTimeout = TimeSpan.FromSeconds(5);

    private readonly ILogger<PseudonymisationHelper> _logger;
    private readonly Uri? _jwksUrl;
    private readonly Func<Task<string>>? _jwksSupplier;
    private readonly IPseudonymisationClient _pseudonymisationClient;
    private readonly Func<string, string>? _privateKeySupplier;
    private readonly SecureRandom _secureRandom;
    private readonly SemaphoreSlim _semaphore;
    private readonly ConcurrentDictionary<string, IDomain?> _domains;
    private readonly ConcurrentDictionary<string, bool> _refreshableDomains;

    /// <summary>
    /// Unmodifiable copy of <see cref="_refreshableDomains"/>.
    /// It is create to prevent any change causing damage on the navigation
    /// in the returned list by the caller.
    /// </summary>
    private volatile IImmutableSet<string> _unmodifiableCopyOfRefreshDomains;
    private volatile Task<JsonWebKeySet>? _jwkSet;

    /// <summary>
    /// Initializes a new instance of the <see cref="PseudonymisationHelper"/> class.
    /// </summary>
    /// <param name="jwksUrl">The URL to fetch the JSON Web Key Set (JWKS) from.</param>
    /// <param name="jwksSupplier">A function that supplies the JWKS as a string.</param>
    /// <param name="pseudonymisationClient">The pseudonymisation client to use for domain operations.</param>
    /// <param name="privateKeySupplier">A function that supplies the private key in JWK format based on the key ID.</param>
    /// <remarks>
    /// Calls `jwksSupplier` and waits up to 5 seconds for the answer. If that fails, the constructor
    /// does not throw: the failure is logged and a new fetch starts at once. The next domain loaded
    /// with <see cref="GetDomain"/> waits for that fetch, and fails if the key server is still down.
    /// </remarks>
    public PseudonymisationHelper(
        Uri? jwksUrl,
        Func<Task<string>>? jwksSupplier,
        IPseudonymisationClient pseudonymisationClient,
        Func<string, string>? privateKeySupplier)
    {
        var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddFilter("Microsoft", LogLevel.Warning)
                .AddFilter("System", LogLevel.Warning)
                .AddFilter("Pseudonymisation.Helper", LogLevel.Debug)
                .AddConsole();
        });

        _logger = loggerFactory.CreateLogger<PseudonymisationHelper>();

        this._jwksUrl = jwksUrl;
        this._jwksSupplier = jwksSupplier;
        this._pseudonymisationClient = pseudonymisationClient;
        this._privateKeySupplier = privateKeySupplier;
        _refreshableDomains = new();
        _unmodifiableCopyOfRefreshDomains = ImmutableHashSet<string>.Empty;

        _secureRandom = CryptoServicesRegistrar.GetSecureRandom();

        _semaphore = new SemaphoreSlim(1, 1);

        InitJwksSilently();

        var properties = new Dictionary<string, object?>
        {
            { nameof(jwksSupplier), jwksSupplier },
            { nameof(jwksUrl), jwksUrl },
        };

        foreach (var property in properties.Where(property => property.Value == null))
        {
            _logger.LogWarning(
                "`{Property}` is null: this PseudonymisationHelper will not be able to encrypt or decrypt any transit info",
                property.Key);
        }

        _domains = new ConcurrentDictionary<string, IDomain?>();
    }

    /// <summary>
    /// Gets return the {@link Set} of domains that must be refreshed.
    /// This list will be automatically populated by this <see cref="PseudonymisationHelper"/>
    /// each time the domain is created, if the JKU is defined as a reciptient
    /// of any key of the domain, the domain will be added to this set.
    /// </summary>
    /// <value>
    /// An unmodifiable <see cref="IImmutableSet{T}"/> containing the list of
    /// the domains that must be refreshed.
    /// </value>
    /// <remarks>
    /// Never calls eHealth: the set is held in memory. It lists the domains you own that have
    /// already been loaded, not every domain you own: one never asked for is not in it.
    /// </remarks>
    public IImmutableSet<string> RefreshableDomains
    {
        get
        {
            return _unmodifiableCopyOfRefreshDomains;
        }
    }

    /// <summary>
    /// Creates a new instance of the <see cref="PseudonymisationHelperBuilder"/> class.
    /// </summary>
    /// <returns>A new instance of the <see cref="PseudonymisationHelperBuilder"/> class.</returns>
    /// <remarks>Never calls eHealth: the builder only collects values, `Build()` is where work starts.</remarks>
    public static PseudonymisationHelperBuilder Builder()
    {
        return new PseudonymisationHelperBuilder();
    }

    /// <summary>
    /// Ask for the JWK set to be fetched again.
    /// This method does not wait for the answer: it only replaces the pending fetch by a new
    /// one, which the next reader of the JWK set will wait for.
    /// It is called automatically every time there is an unknown key ID in your recipient of
    /// one of the JWE defined in the domain. Despite this, it is recommended to call it as
    /// soon as you know that an update has been made to the JWKS.
    /// It is up to you to cache or not the string returned by `jwksSupplier`.
    /// If you did not provide any `jwksSupplier`, this method has no effect.
    /// </summary>
    /// <remarks>
    /// Calls `jwksSupplier` on the spot, despite the name and the `void` return. What it does not
    /// do is wait for the answer: the next reader of the JWK set pays that wait.
    /// </remarks>
    public void FlagJwksForRefresh()
    {
        if (_jwksSupplier != null)
        {
            // The supplier is called here, not inside ParseJwks, so that a supplier which
            // throws on the spot fails the caller instead of the task nobody is watching yet.
            var pending = ParseJwks(WithTimeout(_jwksSupplier(), _eHealthTimeout));

            // Nobody is obliged to read this fetch: another call may replace it first, and the
            // caller of this method never waits for it. A failure nobody reads is dropped on purpose,
            // since a newer fetch supersedes it; reading it here stops .NET from reporting it later
            // as an unobserved task exception, far from its cause.
            _ = pending.ContinueWith(
                task => _ = task.Exception,
                CancellationToken.None,
                TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously,
                TaskScheduler.Default);

            _jwkSet = pending;
        }
    }

    /// <summary>
    /// Refresh the domain for the given domain key.
    /// If the domain is not present, it will be retrieved from the pseudonymisation service
    /// and added to the list of known domains.
    /// If the domain is already present, it will be updated with the latest information.
    /// </summary>
    /// <param name="domainKey">The domain key.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    /// <remarks>
    /// Always calls eHealth, through <see cref="IPseudonymisationClient.GetDomain(string)"/>.
    /// One refresh runs at a time for all domains together, so a slow answer delays the others.
    /// </remarks>
    public async Task RefreshDomain(string domainKey)
    {
        await _semaphore.WaitAsync();

        try
        {
            if (_pseudonymisationClient == null)
            {
                throw new ArgumentNullException(nameof(_pseudonymisationClient));
            }

            // Store the domain under the requested key, not under the key returned by eHealth,
            // which can be different.
            await _pseudonymisationClient
                .GetDomain(domainKey)
                .ContinueWith(task => CreateDomain(task.Result))
                .ContinueWith(task => _domains[domainKey] = task.Result);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    /// <summary>
    /// Get the domain for the given domain key.
    /// If the domain is not present, it will be retrieved from the pseudonymisation service.
    /// </summary>
    /// <param name="domainKey">The key of the domain to retrieve.</param>
    /// <returns>A <see cref="IDomain"/> representing the domain, or null if not found.</returns>
    /// <remarks>
    /// Calls eHealth only when the domain is not held yet; a domain already loaded is returned
    /// from memory. The same call is therefore instant or a network round trip, depending on what
    /// the rest of the application did before.
    /// </remarks>
    public Task<IDomain?> GetDomain(string domainKey)
    {
        IDomain? domain;

        if (_domains.TryGetValue(domainKey, out domain))
        {
            return Task.FromResult(domain);
        }

        return RefreshDomain(domainKey).ContinueWith(unused => _domains[domainKey]);
    }

    /// <summary>
    /// Decodes a Base64Url-encoded string to a byte array.
    /// </summary>
    /// <param name="input">The Base64Url-encoded string.</param>
    /// <returns>The decoded byte array.</returns>
    private static byte[] Base64UrlDecode(string? input)
    {
        if (string.IsNullOrEmpty(input))
        {
            throw new ArgumentException($"Field `{input}` cannot be null or empty.", nameof(input));
        }

        return Base64UrlEncoder.DecodeBytes(input);
    }

    /// <summary>
    /// Returns a task that gives up after the given delay.
    /// The work already started is not cancelled, only abandoned: whoever started it cannot be
    /// told to stop, so the call may well still be running when this task has already failed.
    /// </summary>
    /// <typeparam name="T">The type of the awaited result.</typeparam>
    /// <param name="task">The task to put a time limit on.</param>
    /// <param name="timeout">How long to wait before giving up.</param>
    /// <returns>A task that fails with a <see cref="TimeoutException"/> once the delay is over.</returns>
    private static async Task<T> WithTimeout<T>(Task<T> task, TimeSpan timeout)
    {
        using (var giveUp = new CancellationTokenSource())
        {
            var finished = await Task.WhenAny(task, Task.Delay(timeout, giveUp.Token)).ConfigureAwait(false);

            if (finished != task)
            {
                throw new TimeoutException($"No answer after {timeout.TotalSeconds:0.#} seconds.");
            }

            // Stop the timer, otherwise it holds a pending task until the delay is over.
            giveUp.Cancel();

            return await task.ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Returns the first of the given key IDs that our JWK set knows about.
    /// </summary>
    /// <param name="jwks">Our own JWK set.</param>
    /// <param name="kids">The key IDs the recipients were encrypted for.</param>
    /// <returns>The matching key, or null when the JWK set knows none of them.</returns>
    private static JsonWebKey? FindMatchingJwk(JsonWebKeySet jwks, List<string> kids)
    {
        return kids
            .Select(kid => jwks.Keys.FirstOrDefault(key => key.KeyId == kid))
            .FirstOrDefault(key => key != null);
    }

    /// <summary>
    /// Parses the JWK set once the supplier has answered.
    /// </summary>
    /// <param name="fetch">The pending call to the JWKS supplier.</param>
    /// <returns>The parsed JWK set.</returns>
    private static async Task<JsonWebKeySet> ParseJwks(Task<string> fetch)
    {
        return JsonWebKeySet.Create(await fetch.ConfigureAwait(false));
    }

    private Domain CreateDomain(string rawDomain)
    {
        string? activeKid = null;
        JweContentEncryptionAlgorithm? activeKeyAlgorithm = null;
        bool isKnownJku = false;
        var jku = _jwksUrl == null ? null : _jwksUrl.ToString();

        var parsedEHealthDomain = JsonNode.Parse(rawDomain) ?? throw new InvalidOperationException("Could not parse domain data to JSON.");
        var domainKey = parsedEHealthDomain["domain"]?.GetValue<string>() ?? throw new InvalidOperationException($"Could not find an eHealth domain with domain field equals to domain'");
        var secretKeysFromEHealth = (parsedEHealthDomain["secretKeys"]?.Parent).Deserialize<SecretKeysFromEHealth>()?.SecretKeys;
        var secretKeys = new ConcurrentDictionary<string, SymmetricSecurityKey>(1, secretKeysFromEHealth?.Count ?? 0);

        var jkuList = parsedEHealthDomain["jku"]?.Deserialize<List<string>>();

        // If we are an owner of the domain, we decrypt all the secret keys
        if (jku != null && _jwksSupplier != null && jkuList?.Contains(jku) == true && secretKeysFromEHealth != null)
        {
            isKnownJku = true;

            foreach (var secretKey in secretKeysFromEHealth)
            {
                try
                {
                    var kid = secretKey.Kid ?? throw new InvalidOperationException("Could not find `kid` field.");
                    var jwe = secretKey.Jwe;
                    var jweKey = GetJweKey(jwe, jku);

                    if (jweKey != null)
                    {
                        // Get the RSA private key using the key supplier
                        var hash = jweKey.X5tS256;

                        if (_privateKeySupplier == null)
                        {
                            throw new ArgumentNullException($"{nameof(_privateKeySupplier)}");
                        }

                        string privateKeyJwkJson = _privateKeySupplier(hash);
                        JsonWebKey privateKeyJwk = JsonWebKeyFactory.Create(privateKeyJwkJson);

                        var decryptedJwe = jwe.Decrypt(privateKeyJwk, jweKey.KeyId);

                        // Parse the decrypted payload to extract the secret key
                        var jwkJson = JsonNode.Parse(decryptedJwe.Payload);
                        var jwk = JsonWebKey.Create(decryptedJwe.Payload);

                        var algName = jwkJson?["alg"]?.ToString() ?? throw new InvalidOperationException();

                        var alg = ConvertStringToJweAlgorithm(algName);

                        var k = jwkJson["k"]?.ToString();
                        if (k == null)
                        {
                            throw new InvalidOperationException("Jwk does not contain a `k` property.");
                        }

                        var secretKeyBytes = Base64UrlDecode(k);
                        // Utils.Base64Decoder.Decode(k)
                        secretKeys[kid] = new SymmetricSecurityKey(secretKeyBytes);

                        if (secretKey.Active)
                        {
                            activeKid = kid;
                            activeKeyAlgorithm = alg;
                        }
                    }
                    else
                    {
                        _logger.LogError(_kidProblemWithDomain, kid, domainKey, rawDomain);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "An error occurred when processing the domain `{DomainKey}`. Response from eHealth was\n{RawDomain}",
                        domainKey,
                        rawDomain);
                    throw new InvalidOperationException("Failed to process the JWE object.", ex);
                }
            }
        }

        var curveName = parsedEHealthDomain["crv"]?.ToString()
            ?? throw new InvalidDataException("`crv` can not be null.");

        var curve = EcCurveNames.GetCurveFromString(curveName);

        var bufferSize = parsedEHealthDomain?["bufferSize"]?.GetValue<int>()
            ?? throw new InvalidDataException("The buffersize property is invalid.");

        var audience = parsedEHealthDomain["audience"]?.ToString();

        var ttl = parsedEHealthDomain["timeToLiveInTransit"]?.GetValue<string>() ?? string.Empty;

        var timeToLiveInTransit = XmlConvert.ToTimeSpan(ttl);

        var domain = new Domain(
            domainKey,
            curveName,
            curve,
            audience,
            bufferSize,
            secretKeys,
            activeKid,
            activeKeyAlgorithm,
            timeToLiveInTransit,
            _pseudonymisationClient,
            _secureRandom);

        // Try to add a new key. (The value is unused but TryAdd
        // expects a value parameter, so we give it a bool.)
        if (isKnownJku && _refreshableDomains.TryAdd(domainKey, false))
        {
            _unmodifiableCopyOfRefreshDomains = _refreshableDomains.Keys.ToImmutableHashSet();
        }

        return domain;
    }

    private JweContentEncryptionAlgorithm ConvertStringToJweAlgorithm(string algName)
    {
        try
        {
            JweContentEncryptionAlgorithm algorithm = (JweContentEncryptionAlgorithm)Enum.Parse(typeof(JweContentEncryptionAlgorithm), algName, false);

            return algorithm;
        }
        catch (ArgumentException ex)
        {
            throw new ArgumentException($"Invalid algorithm: {algName}", ex);
        }
    }

    /// <summary>
    /// Finds, in our own JWK set, the key that one of the recipients of this JWE was encrypted
    /// for. A secret key can be addressed to several recipients, and more than one of them can
    /// carry our JKU, so every matching key ID is tried, not just the first.
    /// </summary>
    /// <param name="parsedJwe">The encrypted secret key, with its list of recipients.</param>
    /// <param name="jku">The address of our own JWK set, as eHealth knows it.</param>
    /// <returns>The key to decrypt with, or null when none of the recipients is ours.</returns>
    private JsonWebKey? GetJweKey(JweToken parsedJwe, string jku)
    {
        var kids = parsedJwe.Recipients
            .Select(recipient => recipient.Header)
            .Where(header => jku == header.Jku)
            .Select(header => header.Kid)
            .ToList();

        if (kids.Count == 0)
        {
            _logger.LogError("No valid recipient found in the domain's secret key for the JKU: {Jku}", jku);
            return null;
        }

        var jweKey = FindMatchingJwk(GetJwks(), kids);

        // The key IDs may be newer than the JWK set we hold: fetch it again and look once more.
        if (jweKey == null)
        {
            FlagJwksForRefresh();
            jweKey = FindMatchingJwk(GetJwks(), kids);
        }

        if (jweKey == null)
        {
            _logger.LogError("No JWK with kids in {Kids} found in the JKU `{Jku}`", string.Join(", ", kids), jku);
        }

        return jweKey;
    }

    /// <summary>
    /// Waits for the JWK set. When the fetch failed, the JWK set is flagged for refresh so that
    /// the next reader starts a new fetch instead of failing on the same stale error.
    /// </summary>
    /// <returns>The current JWK set.</returns>
    private JsonWebKeySet GetJwks()
    {
        try
        {
            return _jwkSet?.Result ?? throw new InvalidOperationException("JwkSet was not initialized.");
        }
        catch (ThreadInterruptedException ex)
        {
            _logger.LogError(ex, "The current thread was interrupted.");
            FlagJwksForRefresh();
            Thread.CurrentThread.Interrupt();
            throw;
        }
        catch (AggregateException ex)
        {
            foreach (var exception in ex.InnerExceptions)
            {
                _logger.LogError(exception, exception.Message);
            }

            FlagJwksForRefresh();
            throw;
        }
    }

    /// <summary>
    /// Starts the first fetch of the JWK set and waits for it, without ever failing the caller.
    /// A key server that is down must not stop an application from starting: the failure is
    /// logged, and the next reader of the JWK set will try again.
    /// </summary>
    private void InitJwksSilently()
    {
        if (_jwksSupplier == null)
        {
            _jwkSet = Task.FromException<JsonWebKeySet>(
                new NullReferenceException("`jwksSupplier` cannot be null if you need to encrypt/decrypt transit info"));
            return;
        }

        FlagJwksForRefresh();

        try
        {
            // Wait for that first fetch here: the key set is then ready before this constructor
            // returns, so the first transit info does not pay for it, and a key server that is
            // down is reported now rather than hours later on the first real call. The returned
            // set is not needed — it is already held in _jwkSet.
            _ = GetJwks();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve JWKS");
        }
    }

    /// <summary>
    /// Builder class for constructing instances of <see cref="PseudonymisationHelper"/>.
    /// </summary>
    public class PseudonymisationHelperBuilder
    {
        private Uri? _jwksUrl;
        private Func<Task<string>>? _jwksSupplier;
        private Func<string, string>? _privateKeySupplier;
        private IPseudonymisationClient? _pseudonymisationClient;

        /// <summary>
        /// Sets the JWKS URL.
        /// If this URL is set, the JKU of the recipients of the JWE
        /// will be populated with this URL.
        /// </summary>
        /// <param name="jwksUrl">The JWKS URL.</param>
        /// <returns>A builder for constructing instances of <see cref="PseudonymisationHelper"/>.</returns>
        public PseudonymisationHelperBuilder JwksUrl(Uri? jwksUrl)
        {
            this._jwksUrl = jwksUrl;
            return this;
        }

        /// <summary>
        /// Sets the JWKS supplier function.
        /// This function will be called each time there is an unknown key ID.
        /// </summary>
        /// <param name="jwksSupplier">The JWKS supplier function.</param>
        /// <returns>A builder for constructing instances of <see cref="PseudonymisationHelper"/>.</returns>
        public PseudonymisationHelperBuilder JwkSupplier(Func<Task<string>>? jwksSupplier)
        {
            this._jwksSupplier = jwksSupplier;
            return this;
        }

        /// <summary>
        /// Sets the private key supplier function.
        /// This function will be called each time there is an unknown key ID.
        /// </summary>
        /// <param name="privateKeySupplier">The private key supplier function.</param>
        /// <returns>A builder for constructing instances of <see cref="PseudonymisationHelper"/>.</returns>
        public PseudonymisationHelperBuilder PrivateKeySupplier(Func<string, string>? privateKeySupplier)
        {
            this._privateKeySupplier = privateKeySupplier;
            return this;
        }

        /// <summary>
        /// Sets the pseudonymisation client.
        /// </summary>
        /// <param name="pseudonymisationClient">The pseudonymisation client.</param>
        /// <returns>A builder for constructing instances of <see cref="PseudonymisationHelper"/>.</returns>
        public PseudonymisationHelperBuilder PseudonymisationClient(IPseudonymisationClient? pseudonymisationClient)
        {
            this._pseudonymisationClient = pseudonymisationClient;
            return this;
        }

        /// <summary>
        /// Builds the <see cref="PseudonymisationHelper"/> instance.
        /// </summary>
        /// <returns>A new instance of <see cref="PseudonymisationHelper"/>.</returns>
        /// <remarks>
        /// Calls the JWKS supplier and waits up to 5 seconds for the answer before returning.
        /// A failure is logged, not thrown: a key server that is down does not stop the build.
        /// </remarks>
        public PseudonymisationHelper? Build()
        {
            if (_pseudonymisationClient == null)
            {
                throw new InvalidOperationException($"{nameof(_pseudonymisationClient)} cannot be null.");
            }

            return new PseudonymisationHelper(_jwksUrl, _jwksSupplier, _pseudonymisationClient, _privateKeySupplier);
        }
    }
}
