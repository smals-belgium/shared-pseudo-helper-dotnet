// <copyright file="JweCryptoHelper.cs" company="Riziv-Inami">
// Copyright (c) Riziv-Inami. All rights reserved.
// </copyright>

namespace Nihdi.Common.Pseudonymisation.Jose;

using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Org.BouncyCastle.Asn1.Sec;
using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;

/// <summary>
/// Helper class for JWE cryptographic operations.
/// </summary>
public static class JweCryptoHelper
{
    /// <summary>
    /// Converts a JsonWebKey representing an EC private key to BouncyCastle ECPrivateKeyParameters.
    /// </summary>
    /// <param name="jwk">The JSON Web Key representing the EC private key.</param>
    /// <returns>A BouncyCastle ECPrivateKeyParameters instance.</returns>
    public static ECPrivateKeyParameters ConvertToPrivateKey(JsonWebKey jwk)
    {
        if (string.IsNullOrEmpty(jwk.D))
        {
            throw new CryptographicException("JWK private key (D) is missing.");
        }

        // Convert Base64Url-encoded private key to BigInteger
        BigInteger d = new BigInteger(1, Base64UrlEncoder.DecodeBytes(jwk.D));

        // Get curve parameters
        var curve = ECNamedCurveTable.GetByName(jwk.Crv); // "P-256", "P-384", "P-521", etc.
        if (curve == null)
        {
            throw new CryptographicException($"Unsupported curve: {jwk.Crv}");
        }

        // Create ECPrivateKeyParameters for BouncyCastle
        return new ECPrivateKeyParameters(d, new ECDomainParameters(curve));
    }

    /// <summary>
    /// Converts a JsonWebKey representing an EC public key to BouncyCastle ECPublicKeyParameters.
    /// </summary>
    /// <param name="jwk">The JSON Web Key representing the EC public key.</param>
    /// <returns>A BouncyCastle ECPublicKeyParameters instance.</returns>
    public static ECPublicKeyParameters ConvertToPublicKey(JsonWebKey jwk)
    {
        if (jwk == null)
        {
            throw new ArgumentNullException(nameof(jwk));
        }

        if (string.IsNullOrEmpty(jwk.Crv) || string.IsNullOrEmpty(jwk.X) || string.IsNullOrEmpty(jwk.Y))
        {
            throw new ArgumentException("Invalid JWK: Missing required parameters.");
        }

        // Decode the base64url-encoded coordinates
        byte[] xBytes = Base64UrlEncoder.DecodeBytes(jwk.X);
        byte[] yBytes = Base64UrlEncoder.DecodeBytes(jwk.Y);

        Debug.WriteLine($"X (hex): {BitConverter.ToString(xBytes).Replace("-", string.Empty)}");
        Debug.WriteLine($"Y (hex): {BitConverter.ToString(yBytes).Replace("-", string.Empty)}");

        Debug.WriteLine($"Raw X Base64: {jwk.X}");
        Debug.WriteLine($"Raw Y Base64: {jwk.Y}");

        // Decode Base64Url-encoded X and Y coordinates
        BigInteger x = new BigInteger(1, xBytes);
        BigInteger y = new BigInteger(1, yBytes);

        // Get curve parameters based on the JWK 'crv' field
        X9ECParameters curveParams = GetCurveParameters(jwk.Crv);

        // Create ECPoint from X and Y coordinates
        Org.BouncyCastle.Math.EC.ECPoint q =
            curveParams.Curve.CreatePoint(x, y);

        // Validate that the point is on the curve
        if (!q.IsValid())
        {
            throw new CryptographicException("Invalid EC Point: The given X, Y coordinates do not form a valid point on the curve.");
        }

        return new ECPublicKeyParameters(q, new ECDomainParameters(curveParams));
    }

    /// <summary>
    /// Gets the BouncyCastle curve parameters for a given curve name.
    /// </summary>
    /// <param name="crv">The curve name (e.g., "P-256", "P-384", "P-521").</param>
    /// <returns>The corresponding X9ECParameters.</returns>
    public static X9ECParameters GetCurveParameters(string crv)
    {
        return crv switch
        {
            "P-256" => SecNamedCurves.GetByName("secp256r1"),
            "P-384" => SecNamedCurves.GetByName("secp384r1"),
            "P-521" => SecNamedCurves.GetByName("secp521r1"),
            _ => throw new CryptographicException($"Unsupported curve: {crv}"),
        };
    }

    /// <summary>
    /// Derives an AES key from the shared secret obtained via ECDH key agreement.
    /// </summary>
    /// <param name="sharedSecret">The shared secret (ECDH output).</param>
    /// <param name="keySizeBits">The desired key size in bits (e.g., 256, 384, 512).</param>
    /// <returns>The derived AES key as a byte array.</returns>
    public static byte[] DeriveAesKeyFromEcdh(BigInteger sharedSecret, int keySizeBits)
    {
        // Convert BouncyCastle BigInteger to byte array (unsigned)
        byte[] sharedSecretBytes = sharedSecret.ToByteArrayUnsigned();

        // Create context-specific information ("otherInfo") for key derivation
        byte[] otherInfo = Encoding.UTF8.GetBytes("ECDH-ES Key Agreement");

        // Use HKDF (HMAC-based Key Derivation Function) with SHA-256
        using (var hkdf = new HMACSHA256(sharedSecretBytes))
        {
            byte[] keyLengthBytes = BitConverter.GetBytes(keySizeBits);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(keyLengthBytes);
            }

            // Derive the AES key (truncated to the required Length)
            return hkdf.ComputeHash(otherInfo.Concat(keyLengthBytes).ToArray()).Take(keySizeBits / 8).ToArray();
        }
    }
}
