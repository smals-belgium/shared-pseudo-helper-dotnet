// <copyright file="TransitInfo.cs" company="Riziv-Inami">
// Copyright (c) Riziv-Inami. All rights reserved.
// </copyright>

namespace Nihdi.Common.Pseudonymisation.Internal;

using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.IdentityModel.Tokens;
using Nihdi.Common.Pseudonymisation.Exceptions;
using Nihdi.Common.Pseudonymisation.Extensions;
using Nihdi.Common.Pseudonymisation.Jose;
using Nihdi.Common.Pseudonymisation.Utils;
using Org.BouncyCastle.Math;

/// <inheritdoc/>
public class TransitInfo : ITransitInfo
{
    private readonly TimeSpan _clockSkew = TimeSpan.FromMinutes(1); // as per ehealth spec
    private readonly IDateTimeService _dateTimeService;
    private readonly Domain _domain;
    private string? _raw;
    private JsonElement _parsed;

    /// <summary>
    /// Initializes a new instance of the <see cref="TransitInfo"/> class.
    /// </summary>
    /// <param name="domain">A <see cref="Domain"/> instance.</param>
    /// <param name="raw">A raw JSON string representing the transit information.</param>
    /// <param name="dateTimeService">A <see cref="IDateTimeService"/> instance.</param>
    public TransitInfo(Domain domain, string? raw, IDateTimeService? dateTimeService = null)
    {
        this._domain = domain;
        this._raw = raw;
        this._dateTimeService = dateTimeService ?? new DateTimeService();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TransitInfo"/> class.
    /// </summary>
    /// <param name="domain">A <see cref="Domain"/> instance.</param>
    /// <param name="scalar">A <see cref="BigInteger"/> instance representing the scalar value.</param>
    /// <param name="customizer">A <see cref="ITransitInfoCustomizer"/> instance.</param>
    /// <param name="dateTimeService">A <see cref="IDateTimeService"/> instance.</param>
    public TransitInfo(
        Domain domain,
        BigInteger scalar,
        ITransitInfoCustomizer customizer,
        IDateTimeService? dateTimeService = null)
    {
        if (domain.ActiveKeyEncryptionMethod == null)
        {
            throw new ArgumentException($"The domain {nameof(domain.ActiveKeyEncryptionMethod)} property cannot be null.");
        }

        this._dateTimeService = dateTimeService ?? new DateTimeService();

        if (domain == null)
        {
            throw new InvalidOperationException($"`{nameof(domain)}` is requred.");
        }

        var activeKid = domain.ActiveKid;

        if (activeKid == null)
        {
            throw new InvalidOperationException($"Not able to decrypt the active kid of the domain `{domain}`."
               + "The domain probably needs to be refreshed.");
        }

        var secretKey = domain.SecretKeys?[activeKid];
        if (secretKey == null)
        {
            throw new InvalidOperationException(
                $"SecretKey with kid '{activeKid}' not found: is your user allowed "
                + $"to get secret keys for the domain `{domain.Key}`?");
        }

        TransitPayload transitPayload = CreatePayload(domain, scalar, customizer.Payload);
        Dictionary<string, object> customParams =
        CreateHeaderParams(domain, activeKid, transitPayload, customizer.Header);

        var enc = domain.ActiveKeyEncryptionMethod ?? throw new InvalidOperationException("ActiveKey EncryptionMethod cannot be null when createing JweHeader.");

        var payload = JsonSerializer.Serialize(transitPayload);
        var encryptingCredentials = new EncryptingCredentials(secretKey, "dir", SecurityAlgorithms.Aes256Gcm);

        var handler = new CustomJweHandler();
        var jweCompact = handler.CreateToken(payload, encryptingCredentials, customParams);

        this._domain = domain;
        _parsed = JweUtils.ToJsonFlattened(jweCompact);
        Debug.WriteLine(_parsed.GetRawText());
    }

    /// <inheritdoc/>
    public string AsString()
    {
        if (_raw == null)
        {
            _raw = JweUtils.ToJweCompact(_parsed)
                ?? throw new InvalidOperationException("JWE compact serialization failed.");
        }

        return _raw;
    }

    /// <inheritdoc/>
    public string Audience()
    {
        var parsedJson = Parse();
        if (!parsedJson.TryGetProperty("aud", out JsonElement value) || value.ValueKind != JsonValueKind.Null)
        {
            throw new InvalidTransitInfoException("Missing `aud` in header.");
        }

        return value.ToString();
    }

    /// <inheritdoc/>
    public void ValidateHeader()
    {
        var parsedJson = Parse();
        if (!parsedJson.TryGetProperty("header", out JsonElement headerElement) || headerElement.ValueKind == JsonValueKind.Null)
        {
            throw new InvalidTransitInfoException("Missing 'Header' in transit info.");
        }

        ValidateTransitInfoHeader(headerElement);
    }

    /// <summary>
    /// Returns a dictionary containing the parameteres of the header of this <see cref="ITransitInfo"/>.
    /// Changes done on the returned dictionary are not reflected on this <see cref="ITransitInfo"/>.
    /// </summary>
    /// <returns>A dictionary containing the parameters of the header.</returns>
    public Dictionary<string, object> Header()
    {
        var parsedJson = Parse();

        if (!parsedJson.TryGetProperty("header", out JsonElement headerElement)
            || headerElement.ValueKind == JsonValueKind.Null)
        {
            throw new InvalidTransitInfoException("`header` property is missing.");
        }

        var dictionary = JsonSerializer.Deserialize<Dictionary<string, object>>(headerElement.GetRawText());

        if (dictionary == null)
        {
            throw new InvalidTransitInfoException("`header` property could not be converted to dictionary.");
        }

        return dictionary;
    }

    /// <summary>
    /// Returns a dictionary containing the payload of this <see cref="ITransitInfo"/>.
    /// Changes done on the returned dictionary are not reflected on this <see cref="ITransitInfo"/>.
    /// </summary>
    /// <returns>A <see cref="System.Collections.Generic.Dictionary{TKey, TValue}"/> containing the payload.</returns>
    /// <exception cref="InvalidTransitInfoException">Thrown when the transit info string cannot be parsed or is invalid.</exception>
    /// <exception cref="UnknownKidException">Thrown when the `kid` used to encrypt this transit info is unknown.</exception>
    public Dictionary<string, object>? Payload()
    {
        var parsedTransitInfo = Parse();

        // Convert JsonElement to JsonObject for mutability
        var parsedTransitInfoObj = JsonNode.Parse(parsedTransitInfo.GetRawText())?.AsObject()
            ?? throw new InvalidTransitInfoException("Failed to parse transit info.");

        // Check if "payload" already exists
        if (!parsedTransitInfoObj.TryGetPropertyValue("payload", out var payloadNode) || payloadNode is null)
        {
            ValidateHeader();

            // Retrieve 'header' from the parsed transit info.
            if (!parsedTransitInfoObj.TryGetPropertyValue("header", out var transitInfoHeaderNode) || transitInfoHeaderNode is null)
            {
                throw new InvalidTransitInfoException("`header` property is missing.");
            }

            // Get 'kid' from the header.
            var kid = transitInfoHeaderNode["kid"]?.ToString();
            if (kid == null || kid.Length == 0)
            {
                throw new InvalidOperationException("Could not find `kid` field in transit info header.");
            }

            // Retrieve the secret key based on kid
            var secretKey = _domain.SecretKeys?[kid];
            if (secretKey == null)
            {
                throw new UnknownKidException(kid!);
            }

            try
            {
                // Decrypt the token using the secret key
                if (!parsedTransitInfoObj.TryGetPropertyValue("token", out var tokenNode) || tokenNode == null)
                {
                    throw new InvalidTransitInfoException("'token' property is missing in TransitInfo header.");
                }

                var encryptingCredentials = new EncryptingCredentials(secretKey, "dir", SecurityAlgorithms.Aes256Gcm);
                var handler = new CustomJweHandler();
                var token = tokenNode.ToString();
                var decryptedJwe = handler.DecryptJwe(token, encryptingCredentials);

                var decryptedPayloadNode = JsonNode.Parse(decryptedJwe);

                parsedTransitInfoObj["payload"] = decryptedPayloadNode;
            }
            catch (Exception ex)
            {
                throw new InvalidTransitInfoException("Error when decrypting transitInfo", ex);
            }
        }

        var payloadJson = parsedTransitInfoObj["payload"]?.ToJsonString();
        if (payloadJson != null)
        {
            var transitPayload = JsonSerializer.Deserialize<TransitPayload>(payloadJson);
            return transitPayload?.ToDictionary();
        }

        return null;
    }

    /// <summary>
    /// Validates the payload of the transit info.
    /// </summary>
    internal void ValidatePayload()
    {
        var payload = Payload() ?? throw new InvalidOperationException("The Payload cannot be null or empty.");

        if (!payload.TryGetValue("iat", out var iatValue))
        {
            throw new InvalidOperationException("The payload does not contain a `iat` field.");
        }

        long iat = Convert.ToInt64(iatValue);

        var exp = (long)(payload?["exp"] ?? throw new InvalidOperationException());
        var currentTime = _dateTimeService.UtcNow;

        if (currentTime.Add(_clockSkew).IsBefore(DateTimeHelper.FromEpochSecondsToDateTime(iat)))
        {
            throw new InvalidTransitInfoException("transifInfo not yet ready for use (iat > now)");
        }

        if (currentTime.Add(-_clockSkew).IsAfter(DateTimeHelper.FromEpochSecondsToDateTime(exp)))
        {
            throw new InvalidTransitInfoException("expired transifInfo (now > exp)");
        }
    }

    private JsonElement Parse()
    {
        if (_parsed.ValueKind != JsonValueKind.Undefined)
        {
            return _parsed;
        }

        JsonElement parsedTemp;
        try
        {
            parsedTemp = JweUtils.ToJsonFlattened(_raw);
        }
        catch (Exception ex)
        {
            throw new InvalidTransitInfoException("Error when parsing transitInfo.", ex);
        }

        if (parsedTemp.ValueKind == JsonValueKind.Null)
        {
            throw new InvalidTransitInfoException("Could not parse jwe compact.");
        }

        if (!parsedTemp.TryGetProperty("header", out var header) || header.ValueKind == JsonValueKind.Null)
        {
            throw new InvalidTransitInfoException("`header` property is missing.");
        }

        var alg = header.GetProperty("alg").GetString();
        if (string.IsNullOrEmpty(alg) || alg != JweAlgorithm.DIR.ToString().ToLower())
        {
            throw new InvalidTransitInfoException("`alg` with value `dir` expected in header.");
        }

        if (!header.TryGetProperty("enc", out _))
        {
            throw new InvalidTransitInfoException("Missing `enc` in header.");
        }

        var aud = header.GetProperty("aud").GetString();
        if (string.IsNullOrEmpty(aud))
        {
            throw new InvalidTransitInfoException("Missing `aud` in header.");
        }

        _parsed = parsedTemp;
        return parsedTemp;
    }

    private Dictionary<string, object> CreateHeaderParams(
            Domain domain,
            string kid,
            TransitPayload transitPayload,
            Dictionary<string, object> customHeaderParams)
    {
        if (domain.Audience == null)
        {
            throw new ArgumentException($"The domain {nameof(domain.Audience)} property cannot be null.");
        }

        var computedCustomHeaderParams = new Dictionary<string, object>(customHeaderParams)!;
        computedCustomHeaderParams.Add("kid", kid);
        computedCustomHeaderParams.Add("aud", domain.Audience);
        computedCustomHeaderParams.Add("iat", transitPayload.Iat);
        computedCustomHeaderParams.Add("exp", transitPayload.Exp);
        return computedCustomHeaderParams;
    }

    private void ValidateTransitInfoHeader(JsonElement? transitInfoHeader)
    {
        if (transitInfoHeader is null
            || !(transitInfoHeader.Value.TryGetProperty("aud", out JsonElement audElement)
            || audElement.GetString() != _domain.Audience))
        {
            throw new InvalidTransitInfoException("Invalid `aud`");
        }
    }

    private TransitPayload CreatePayload(Domain domain, BigInteger scalar, Dictionary<string, object> customPayload)
    {
        if (domain.InTransitTtl == null)
        {
            throw new ArgumentException($"The domain {nameof(domain.InTransitTtl)} property cannot be null.");
        }

        var inTransitTtl = domain.InTransitTtl;
        var currentTime = _dateTimeService.UtcNow;

        var payload = new TransitPayload
        {
            Iat = currentTime.ToUnixTimeSeconds(),
            Exp = currentTime.Add(inTransitTtl.Value).ToUnixTimeSeconds(),
            Scalar = Convert.ToBase64String(scalar.ToByteArray()),
        };

        // Add custom payload properties if needed
        foreach (var kvp in customPayload)
        {
            var propertyInfo = typeof(TransitPayload).GetProperty(kvp.Key);
            if (propertyInfo != null && propertyInfo.CanWrite)
            {
                throw new InvalidOperationException($"The property '{kvp.Key}' already exists in the TransitPayload class.");
            }
            else
            {
                if (payload.AdditionalProperties.ContainsKey(kvp.Key))
                {
                    throw new InvalidOperationException($"The property '{kvp.Key}' already exists in the AdditionalProperties dictionary.");
                }

                payload.AdditionalProperties[kvp.Key] = kvp.Value;
            }
        }

        return payload;
    }
}