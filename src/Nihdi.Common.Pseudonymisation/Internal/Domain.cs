// <copyright file="Domain.cs" company="Riziv-Inami">
// Copyright (c) Riziv-Inami. All rights reserved.
// </copyright>

namespace Nihdi.Common.Pseudonymisation.Internal;

using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using Nihdi.Common.Pseudonymisation.Jose;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Math.EC;
using Org.BouncyCastle.Security;

/// <inheritdoc/>
public class Domain : IDomain
{
    private readonly ValueFactory _valueFactory;
    private readonly PseudonymFactory _pseudonymFactory;
    private readonly PseudonymInTransitFactory _pseudonymInTransitFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="Domain"/> class with specified parameters.
    /// </summary>
    /// <param name="key">The cryptographic key used for pseudonymization.</param>
    /// <param name="crv">The name of the elliptic curve to be used.</param>
    /// <param name="curve">The elliptic curve implementation.</param>
    /// <param name="audience">The intended audience for generated tokens.</param>
    /// <param name="bufferSize">The buffer size for operations.</param>
    /// <param name="secretKeys">A dictionary of symmetric keys identified by their key IDs.</param>
    /// <param name="activeKid">The key ID of the currently active key.</param>
    /// <param name="activeKeyEncryptionMethod">The encryption algorithm used for the active key.</param>
    /// <param name="inTransitTtl">The time-to-live duration for in-transit pseudonyms.</param>
    /// <param name="pseudonymisationClient">The client used for pseudonymization operations.</param>
    /// <param name="secureRandom">The secure random number generator to use, or null to create a new one.</param>
    public Domain(
        string? key,
        string? crv,
        ECCurve curve,
        string? audience,
        int bufferSize,
        IDictionary<string, SymmetricSecurityKey>? secretKeys,
        string? activeKid,
        JweContentEncryptionAlgorithm? activeKeyEncryptionMethod,
        TimeSpan? inTransitTtl,
        IPseudonymisationClient? pseudonymisationClient,
        SecureRandom? secureRandom)
    {
        Key = key;
        Crv = crv;
        Curve = curve;
        Audience = audience;
        BufferSize = bufferSize;
        SecretKeys = secretKeys;
        ActiveKid = activeKid;
        ActiveKeyEncryptionMethod = activeKeyEncryptionMethod;
        InTransitTtl = inTransitTtl;
        PseudonymisationClient = pseudonymisationClient;
        _valueFactory = new ValueFactory(this);
        _pseudonymFactory = new PseudonymFactory(this);
        _pseudonymInTransitFactory = new PseudonymInTransitFactory(this);
        SecureRandom = secureRandom ?? new SecureRandom();
    }

    /// <inheritdoc/>
    public string? Key
    {
        get; private set;
    }

    /// <summary>
    /// Gets the curve name.
    /// </summary>
    /// <value>The curve name.</value>
    public string? Crv
    {
        get; private set;
    }

    /// <summary>
    /// Gets the ECCurve instance.
    /// </summary>
    /// <value>The ECCurve instance.</value>
    public ECCurve Curve
    {
        get; private set;
    }

    /// <summary>
    /// Gets the intended audience for tokens.
    /// </summary>
    /// <value>The intended audience for tokens.</value>
    public string? Audience
    {
        get; private set;
    }

    /// <summary>
    /// Gets the buffer size for domain operations.
    /// </summary>
    /// <value>The buffer size for domain operations.</value>
    public int BufferSize
    {
        get; private set;
    }

    /// <summary>
    /// Gets the dictionary of symmetric secret keys.
    /// </summary>
    /// <value>The dictionary of symmetric secret keys.</value>
    public IDictionary<string, SymmetricSecurityKey>? SecretKeys
    {
        get; private set;
    }

    /// <summary>
    /// Gets the active key identifier.
    /// </summary>
    /// <value>The active key identifier.</value>
    public string? ActiveKid
    {
        get; private set;
    }

    /// <summary>
    /// Gets the active key encryption algorithm.
    /// </summary>
    /// <value>The active key encryption algorithm.</value>
    public JweContentEncryptionAlgorithm? ActiveKeyEncryptionMethod
    {
        get; private set;
    }

    /// <summary>
    /// Gets the time-to-live for in-transit pseudonyms.
    /// </summary>
    /// <value>The time-to-live for in-transit pseudonyms.</value>
    public TimeSpan? InTransitTtl
    {
        get; private set;
    }

    /// <summary>
    /// Gets the pseudonymisation client.
    /// </summary>
    /// <value>The pseudonymisation client.</value>
    public IPseudonymisationClient? PseudonymisationClient
    {
        get; private set;
    }

    /// <inheritdoc/>
    IValueFactory IDomain.ValueFactory => _valueFactory;

    /// <inheritdoc/>
    IPseudonymFactory IDomain.PseudonymFactory => _pseudonymFactory;

    /// <inheritdoc/>
    IPseudonymInTransitFactory IDomain.PseudonymInTransitFactory => _pseudonymInTransitFactory;

    /// <summary>
    /// Gets the secure random generator.
    /// </summary>
    /// <value>The secure random generator.</value>
    public SecureRandom? SecureRandom
    {
        get; private set;
    }

    /// <summary>
    /// Gets the value factory for this domain.
    /// </summary>
    /// <value>
    /// The value factory instance.
    /// </value>
    public PseudonymFactory PseudonymFactory => _pseudonymFactory;

    /// <summary>
    /// Gets the value factory for this domain.
    /// </summary>
    /// <value>
    /// The value factory instance.
    /// </value>
    public ValueFactory ValueFactory => _valueFactory;

    /// <summary>
    /// Gets the pseudonym in transit factory for this domain.
    /// </summary>
    /// <value>
    /// The pseudonym in transit factory instance.
    /// </value>
    public PseudonymInTransitFactory PseudonymInTransitFactory => _pseudonymInTransitFactory;

    /// <summary>
    /// Creates a payload string from a pseudonym, automatically extracting transit information if available.
    /// </summary>
    /// <param name="pseudonym">The pseudonym to create a payload from.</param>
    /// <returns>A JSON string representation of the pseudonym payload.</returns>
    internal string CreatePayloadString(IPseudonym pseudonym)
    {
        return CreatePayloadString(pseudonym, (pseudonym as IPseudonymInTransit)?.GetTransitInfo().ToString() ?? null);
    }

    /// <summary>
    /// Creates a payload string from a pseudonym with the specified transit information.
    /// </summary>
    /// <param name="pseudonym">The pseudonym to create a payload from.</param>
    /// <param name="transitInfo">The transit information to include in the payload, or null if none.</param>
    /// <returns>A JSON string representation of the pseudonym payload.</returns>
    internal string CreatePayloadString(IPseudonym pseudonym, string? transitInfo)
    {
        return CreatePayload(pseudonym, transitInfo);
    }

    /// <summary>
    /// Creates a payload string from a pseudonym, automatically extracting transit information if it's a pseudonym in transit.
    /// </summary>
    /// <param name="pseudonym">The pseudonym to create a payload from.</param>
    /// <returns>A JSON string representation of the pseudonym payload.</returns>
    internal string CreatePayload(IPseudonym pseudonym)
    {
        if (pseudonym is IPseudonymInTransit pseudonymInTransit)
        {
            return CreatePayload(pseudonym, pseudonymInTransit.GetTransitInfo().AsString());
        }

        return CreatePayload(pseudonym, null);
    }

    /// <summary>
    /// Creates a JSON payload object containing pseudonym information and optional transit data.
    /// </summary>
    /// <param name="pseudonym">The pseudonym to create a payload from.</param>
    /// <param name="transitInfo">The transit information to include in the payload, or null if none.</param>
    /// <returns>A JSON string representation of the pseudonym payload.</returns>
    internal string CreatePayload(IPseudonym pseudonym, string? transitInfo)
    {
        var payload = new JObject
        {
            { "id", JToken.FromObject(Guid.NewGuid()) },
            { "crv", JToken.FromObject(Crv ?? throw new InvalidOperationException("`crv` field cannot be null.")) },
            { "x", JToken.FromObject(pseudonym.X()) },
            { "y", JToken.FromObject(pseudonym.Y()) },
        };

        if (transitInfo != null)
        {
            payload.Add("transitInfo", JToken.FromObject(transitInfo));
        }

        return payload.ToString();
    }

    /// <summary>
    /// Creates a cryptographically secure random big integer suitable for elliptic curve operations.
    /// </summary>
    /// <remarks>
    /// This method ensures the generated random value is neither zero nor equal to the curve order,
    /// which would result in invalid calculations in elliptic curve cryptography.
    /// </remarks>
    /// <returns>A random big integer within the valid range for the domain's elliptic curve.</returns>
    internal BigInteger CreateRandom()
    {
        BigInteger random;

        var curveOrder = Curve.Order;
        do
        {
            // 1 is excluded to prevent no-op blinding
            // P521.getOrder() is excluded to prevent `INF` (infinite) result
            // Not sure those checks are necessary because I guess BouncyCastle already does it
            random = Curve.RandomFieldElementMult(SecureRandom).ToBigInteger();
        }
        while (random.Equals(BigInteger.Zero) || random.Equals(curveOrder));

        return random;
    }
}