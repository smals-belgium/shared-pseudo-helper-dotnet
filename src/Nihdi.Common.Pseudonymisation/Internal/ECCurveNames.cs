// <copyright file="ECCurveNames.cs" company="Riziv-Inami">
// Copyright (c) Riziv-Inami. All rights reserved.
// </copyright>

namespace Nihdi.Common.Pseudonymisation.Internal;

using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Math.EC;

/// <summary>
/// Helper class for handling ECCurve names and retrieval.
/// </summary>
public static class EcCurveNames
{
    /// <summary>
    /// Gets the ECCurve instance corresponding to the given curve name.
    /// </summary>
    /// <param name="curveName">The name of the curve (e.g., "P-521").</param>
    /// <returns>The corresponding <see cref="ECCurve"/> instance.</returns>
    public static ECCurve GetCurveFromString(string curveName)
    {
        if (curveName == null)
        {
            throw new ArgumentNullException(nameof(curveName));
        }

        try
        {
            switch (curveName)
            {
                case "P-521":

                    var ecParams = ECNamedCurveTable.GetByNameLazy(curveName);
                    return ecParams.Curve;
                default:
                    throw new ArgumentException($"Unsupported curve: {curveName}");
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Could not get ECCurve by name `{curveName}`", ex);
        }
    }
}
