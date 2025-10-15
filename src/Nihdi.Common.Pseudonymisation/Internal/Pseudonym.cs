// <copyright file="Pseudonym.cs" company="Riziv-Inami">
// Copyright (c) Riziv-Inami. All rights reserved.
// </copyright>

namespace Nihdi.Common.Pseudonymisation.Internal;

using Microsoft.IdentityModel.Tokens;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Math.EC;

/// <inheritdoc/>
public class Pseudonym : Point, IPseudonym
{
    private readonly ITransitInfoCustomizer _noOpTransitInfoCustomizer = new DefaultTransitInfoCustomizer();

    /// <summary>
    /// Initializes a new instance of the <see cref="Pseudonym"/> class.
    /// </summary>
    /// <param name="ecPoint">The elliptic curve point.</param>
    /// <param name="domain">The domain.</param>
    public Pseudonym(ECPoint ecPoint, IDomain domain)
        : base(ecPoint, domain)
    {
    }

    /// <inheritdoc/>
    public string AsString()
    {
        byte[] bytes = EcPoint.GetEncoded(false);
        return Base64UrlEncoder.Encode(EcPoint.GetEncoded(false));
    }

    /// <inheritdoc/>
    public string AsShortString()
    {
        return Base64UrlEncoder.Encode(EcPoint.GetEncoded(true));
    }

    /// <inheritdoc/>
    public Task<IPseudonymInTransit> ConvertTo(IDomain toDomain)
    {
        var domain = Domain;
        var random = domain.CreateRandom();
        var payload = domain.CreatePayloadString(Multiply(random));

        if (domain.Key == null)
        {
            throw new ArgumentNullException(nameof(domain.Key));
        }

        if (toDomain.Key == null)
        {
            throw new ArgumentException(nameof(toDomain.Key));
        }

        if (payload == null)
        {
            throw new ArgumentNullException(nameof(payload));
        }

        if (domain.PseudonymisationClient == null)
        {
            throw new ArgumentNullException(nameof(domain.PseudonymisationClient));
        }

        return domain.PseudonymisationClient.ConvertTo(domain.Key, toDomain.Key, payload)
            .ContinueWith(t =>
                ((PseudonymInTransitFactory)toDomain
                    .PseudonymInTransitFactory)
                    .FromRawResponse(t.Result, random));
    }

    /// <inheritdoc/>
    public virtual IPseudonymInTransit InTransit()
    {
        return InTransit(_noOpTransitInfoCustomizer);
    }

    /// <inheritdoc/>
    public virtual IPseudonymInTransit InTransit(ITransitInfoCustomizer transitInfoCustomizer)
    {
        var domain = Domain;
        var random = domain.CreateRandom();
        var randomModInverse = random.ModInverse(EcPoint.Curve.Order);
        var blinded = new Pseudonym(EcPoint.Multiply(randomModInverse).Normalize(), domain);
        var transitInfo = new TransitInfo(domain, random, transitInfoCustomizer);
        return new PseudonymInTransit(blinded, transitInfo, this);
    }

    /// <summary>
    /// Multiplies the current point by a scalar.
    /// </summary>
    /// <param name="scalar">The scalar to multiply by.</param>
    /// <returns>The resulting point.</returns>
    public Pseudonym Multiply(BigInteger scalar)
    {
        return new Pseudonym(EcPoint.Multiply(scalar).Normalize(), Domain);
    }

    /// <summary>
    /// Multiplies the current point by the modular inverse of a scalar.
    /// </summary>
    /// <param name="scalar">The scalar to compute the modular inverse of and multiply by.</param>
    /// <returns>The resulting point.</returns>
    public Pseudonym MultiplyByModInverse(BigInteger scalar)
    {
        return Multiply(scalar.ModInverse(EcPoint.Curve.Order));
    }

    /// <summary>
    /// Converts this pseudonym to a value.
    /// </summary>
    /// <returns>The value representation of this pseudonym.</returns>
    public Value AsValue()
    {
        return new Value(EcPoint, Domain);
    }
}