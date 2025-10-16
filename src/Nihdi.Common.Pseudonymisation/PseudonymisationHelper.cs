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

        if (jwksUrl == null)
        {
            _logger.LogInformation($"{nameof(jwksUrl)} is null: this {nameof(PseudonymisationHelper)} will not be able to encrypt or decrypt any transit info");
        }

        if (jwksSupplier == null)
        {
            _logger.LogInformation($"{nameof(jwksSupplier)} is null: this {nameof(PseudonymisationHelper)} will not be able to encrypt or decrypt any transit info");

            _jwkSet = (Task<JsonWebKeySet>)Task.FromException(new ArgumentNullException($"{nameof(jwksSupplier)} cannot be null if you need to encrypt/decrypt transit info"));
        }
        else
        {
            RefreshJwks();
        }

        var properties = new Dictionary<string, object?>
        {
            { "jwksSupplier", jwksSupplier },
        };

        properties
        .Where(property => property.Value == null)
        .ToList()
        .ForEach(property => _logger.LogInformation(
            "{0} is null: this PseudonymisationHelper will not be able to encrypt or decrypt any transit inform",
            property.Key));

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
    public static PseudonymisationHelperBuilder Builder()
    {
        return new PseudonymisationHelperBuilder();
    }

    /// <summary>
    /// Refresh the JWK set.
    /// This method will retrieve the JWK set string by calling `jwksSupplier`
    /// given in the constructor, every time there is an unknown key ID in your
    /// recipient of one of the JWE defined in the domain.
    /// Despite this, it is recommended to call this method as soon as you know
    /// that an update has been made to the JWKS.
    /// It is up to you to cache or not the string returned by `jwksSupplier`.
    /// </summary>
    public void RefreshJwks()
    {
        var json = _jwksSupplier?.Invoke().Result;
        _jwkSet = Task.FromResult(JsonWebKeySet.Create(json));
    }

    /// <summary>
    /// Refresh the domain for the given domain key.
    /// If the domain is not present, it will be retrieved from the pseudonymisation service
    /// and added to the list of known domains.
    /// If the domain is already present, it will be updated with the latest information.
    /// </summary>
    /// <param name="domainKey">The domain key.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task RefreshDomain(string domainKey)
    {
        await _semaphore.WaitAsync();

        try
        {
            if (_pseudonymisationClient == null)
            {
                throw new ArgumentNullException(nameof(_pseudonymisationClient));
            }

            var result = await _pseudonymisationClient
                .GetDomain(domainKey)
                .ContinueWith(task => CreateDomain(task.Result))
                .ContinueWith(task => _domains[task.Result.Key ?? throw new InvalidOperationException()] = task.Result);
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

        if (jku != null && _jwksSupplier != null && jkuList?.Contains(jku) == true && secretKeysFromEHealth != null)
        {
            isKnownJku = true;

            foreach (var secretKey in secretKeysFromEHealth)
            {
                try
                {
                    var kid = secretKey.Kid ?? throw new InvalidOperationException("Could not find `kid` field.");
                    var jwe = secretKey.Jwe;
                    jwe.Recipients.RemoveAll(r => r.Header.Jku != jku);
                    var isActiveKid = secretKey.Active;

                    var myKid = jwe.Recipients.FirstOrDefault(r => r.Header.Jku == jku)?.Header.Kid;

                    if (myKid != null)
                    {
                        var privateKid = myKid;
                        var jsonWebKey = GetJsonWebKey(myKid, domainKey);

                        if (jsonWebKey != null)
                        {
                            // Get the RSA private key using the key supplier
                            var hash = jsonWebKey.X5tS256;

                            if (_privateKeySupplier == null)
                            {
                                throw new ArgumentNullException($"{nameof(_privateKeySupplier)}");
                            }

                            string privateKeyJwkJson = _privateKeySupplier(hash);
                            JsonWebKey privateKeyJwk = JsonWebKeyFactory.Create(privateKeyJwkJson);

                            var decryptedJwe = jwe.Decrypt(privateKeyJwk, myKid);

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

                            if (isActiveKid)
                            {
                                activeKid = kid;
                                activeKeyAlgorithm = alg;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error while decoding JWT token.");
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

        if (activeKid == null)
        {
            throw new InvalidOperationException("No secretKey contains an activeKid claim.");
        }

        if (activeKeyAlgorithm == null)
        {
            throw new InvalidOperationException("Impossible to determine the activeKey algorithm.");
        }

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

    private JsonWebKey? GetJsonWebKey(string privateKid, string? domainKey)
    {
        var jweKey = GetJwks().Keys.FirstOrDefault(k => k.KeyId == privateKid);
        // If the private key is unknown, we will refresh the JWKS
        if (jweKey == null)
        {
            RefreshJwks();
            jweKey = GetJwks().Keys.FirstOrDefault(k => k.KeyId == privateKid);
        }

        if (jweKey == null)
        {
            // If the private key is still unknown, we log the error
            var msg = $"The kid `{privateKid}` is not present in the"
                  + $"JWKS `{_jwksUrl}`: impossible to encrypt/decrypt any "
                  + $"transit info of domain `{domainKey}`";
            _logger.LogError(msg);
        }

        return jweKey;
    }

    private JsonWebKeySet GetJwks()
    {
        try
        {
            return _jwkSet?.Result ?? throw new InvalidOperationException("JwkSet was not initialized.");
        }
        catch (ThreadInterruptedException ex)
        {
            _logger.LogError(ex, "The current thread was interrupted.");
            Thread.CurrentThread.Interrupt();
            throw;
        }
        catch (AggregateException ex)
        {
            foreach (var exception in ex.InnerExceptions)
            {
                _logger.LogError(exception, exception.Message);
            }

            throw;
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
