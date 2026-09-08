// <copyright file="JsonWebKeyExtensions.cs" company="Riziv-Inami">
// Copyright (c) Riziv-Inami. All rights reserved.
// </copyright>

namespace Nihdi.Common.Pseudonymisation.Extensions;

using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;

/// <summary>
/// Extension methods for working with <see cref="JsonWebKey"/> objects.
/// </summary>
public static class JsonWebKeyExtensions
{
    /// <summary>
    /// Extracts RSA parameters from a JsonWebKey.
    /// </summary>
    /// <param name="jwk">The JsonWebKey to extract RSA parameters from.</param>
    /// <returns>An RSAParameters object containing the extracted RSA key information.</returns>
    public static RSAParameters ExtractRsaParameters(this JsonWebKey jwk)
    {
        if (jwk.Kty != "RSA")
        {
            throw new InvalidOperationException("The provided key is not an RSA key.");
        }

        var rsaParameters = new RSAParameters
        {
            Modulus = Base64UrlEncoder.DecodeBytes(jwk.N),
            Exponent = Base64UrlEncoder.DecodeBytes(jwk.E),
        };

        // If the JWK contains private key parts, add them
        if (!string.IsNullOrEmpty(jwk.D))
        {
            rsaParameters.D = Base64UrlEncoder.DecodeBytes(jwk.D);
            rsaParameters.P = Base64UrlEncoder.DecodeBytes(jwk.P);
            rsaParameters.Q = Base64UrlEncoder.DecodeBytes(jwk.Q);
        }

        if (!string.IsNullOrEmpty(jwk.DP)
            && !string.IsNullOrEmpty(jwk.DQ)
            && !string.IsNullOrEmpty(jwk.QI))
        {
            rsaParameters.DP = Base64UrlEncoder.DecodeBytes(jwk.DP);
            rsaParameters.DQ = Base64UrlEncoder.DecodeBytes(jwk.DQ);
            rsaParameters.InverseQ = Base64UrlEncoder.DecodeBytes(jwk.QI);
        }

        return rsaParameters;
    }
}
