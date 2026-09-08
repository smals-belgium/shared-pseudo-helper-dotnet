// <copyright file="JsonWebKeyFactory.cs" company="Riziv-Inami">
// Copyright (c) Riziv-Inami. All rights reserved.
// </copyright>

namespace Nihdi.Common.Pseudonymisation.Jose;

using Microsoft.IdentityModel.Tokens;

/// <summary>
/// Factory class for creating JsonWebKey instances with inferred algorithms.
/// </summary>
public static class JsonWebKeyFactory
{
    /// <summary>
    /// Creates a JsonWebKey from a JSON string, inferring the algorithm if not specified
    /// based on the key type and usage.
    /// </summary>
    /// <param name="jwkJson">The JSON representation of the JWK.</param>
    /// <returns>The created JsonWebKey with inferred algorithm if necessary.</returns>
    public static JsonWebKey Create(string jwkJson)
    {
        if (string.IsNullOrEmpty(jwkJson))
        {
            throw new ArgumentNullException(nameof(jwkJson));
        }

        var jsonWebKey = JsonWebKey.Create(jwkJson);

        if (string.IsNullOrEmpty(jsonWebKey.Alg))
        {
            jsonWebKey.Alg = DetermineAlgorithm(jsonWebKey);
        }

        return jsonWebKey;
    }

    /// <summary>
    /// Determines the appropriate algorithm based on the key type and usage.
    /// </summary>
    /// <param name="jwk">The JsonWebKey instance.</param>
    /// <returns>The inferred algorithm string.</returns>
    private static string DetermineAlgorithm(JsonWebKey jwk)
    {
        return jwk.Kty switch
        {
            "RSA" => GetRsaAlgorithm(jwk.Use),
            "EC" => GetEcAlgorithm(jwk.Alg),
            "oct" => GetOctAlgorithm(jwk.Use),
            "OKP" => GetOkpAlgorithm(jwk.Alg),
            _ => throw new NotSupportedException($"Unsupported key type: {jwk.Kty}."),
        };
    }

    private static string GetRsaAlgorithm(string use)
    {
        return use switch
        {
            "enc" => "RSA-OAEP-256",
            "sig" => "RS256",
            _ => "RSA-OAEP-256",
        };
    }

    private static string GetEcAlgorithm(string alg)
    {
        return !string.IsNullOrEmpty(alg) ? alg : "ECDH-ES";
    }

    private static string GetOctAlgorithm(string use)
    {
        return use switch
        {
            "enc" => "A256GCM",
            "sig" => "HS256",
            _ => "A256GCM",
        };
    }

    private static string GetOkpAlgorithm(string alg)
    {
        return !string.IsNullOrEmpty(alg) ? alg : "X25519";
    }
}
