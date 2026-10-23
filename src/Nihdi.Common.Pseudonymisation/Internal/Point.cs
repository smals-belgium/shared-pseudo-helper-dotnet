// <copyright file="Point.cs" company="Riziv-Inami">
// Copyright (c) Riziv-Inami. All rights reserved.
// </copyright>

namespace Nihdi.Common.Pseudonymisation.Internal;

using Nihdi.Common.Pseudonymisation;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Math.EC;

/// <inheritdoc/>
internal abstract class Point : IPoint
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Point"/> class.
    /// </summary>
    /// <param name="ecPoint">The elliptic curve point.</param>
    /// <param name="domainImpl">The domain implementation.</param>
    protected Point(ECPoint ecPoint, IDomain domainImpl)
    {
        EcPoint = ecPoint;
        Domain = (Domain)domainImpl;
    }

    /// <inheritdoc/>
    public Domain Domain
    {
        get; private set;
    }

    /// <inheritdoc/>
    IDomain IPoint.Domain => Domain;

    /// <summary>
    /// Gets the elliptic curve point.
    /// </summary>
    /// <value>The elliptic curve point.</value>
    internal ECPoint EcPoint
    {
        get; private set;
    }

    /// <summary>
    /// Compute the Y coordinate on the basis of X coordinate.
    /// One of the 2 possible Y will be returned: you have no control on which
    /// one will be chosen.
    /// The code has been copied because the Bouncy Castle method is not public.
    /// </summary>
    /// <param name="curve">The elliptic curve.</param>
    /// <param name="x">X coordinate as BigInteger.</param>
    /// <returns>The Y coordinate as BigInteger, or null if it cannot be computed.</returns>
    public static BigInteger? ComputeY(ECCurve curve, BigInteger x)
    {
        var xFieldElement = curve.FromBigInteger(x);
        var rhs = xFieldElement.Square()
                               .Add(curve.A)
                               .Multiply(xFieldElement)
                               .Add(curve.B);
        var y = rhs.Sqrt();
        return y == null ? null : y.ToBigInteger();
    }

    /// <inheritdoc/>
    public string X()
    {
        return Convert.ToBase64String(EcPoint.XCoord.GetEncoded());
    }

    /// <inheritdoc/>
    public string Y()
    {
        return Convert.ToBase64String(EcPoint.YCoord.GetEncoded());
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj)
    {
        if (obj is null)
        {
            return false;
        }

        if (this == obj)
        {
            return true;
        }

        if (!(obj is Point point))
        {
            return false;
        }

        if (!Equals(Domain.Key, point.Domain.Key))
        {
            return false;
        }

        // Compare with the X of the other point. This differs from the Java version on purpose:
        // Java compares this point with itself, so any two points of a domain are equal there.
        // Java also handles a pseudonym in transit here. C# does not need to: PseudonymInTransit
        // is not a Point, so it is already rejected by the check above.
        return Equals(EcPoint.XCoord, point.EcPoint.XCoord);
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Uses only the X coordinate, like Equals. Otherwise two equal points (same X, opposite Y)
    /// would get different hash codes.
    /// </remarks>
    public override int GetHashCode()
    {
        return HashCode.Combine(EcPoint.XCoord, Domain.Key);
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return string.Format(
            "{{\"x\": \"{0}\", \"y\": \"{1}\", \"domain\": \"{2}\"}}",
            Convert.ToBase64String(EcPoint.XCoord.GetEncoded()),
            Convert.ToBase64String(EcPoint.YCoord.GetEncoded()),
            Domain.Key);
    }
}
