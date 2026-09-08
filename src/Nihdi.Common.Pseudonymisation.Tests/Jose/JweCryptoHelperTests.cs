// <copyright file="JweCryptoHelperTests.cs" company="Riziv-Inami">
// Copyright (c) Riziv-Inami. All rights reserved.
// </copyright>

namespace Nihdi.Common.Pseudonymisation.Tests.Jose;

using System;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.IdentityModel.Tokens;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nihdi.Common.Pseudonymisation.Jose;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;

[TestClass]
public class JweCryptoHelperTests
{
    private readonly JsonWebKey _jwk = new JsonWebKey();

    [TestInitialize]
    public void Initialize()
    {
        using (ECDsa ecdsa = ECDsa.Create(ECCurve.NamedCurves.nistP521))
        {
            ECParameters parameters = ecdsa.ExportParameters(true);
            _jwk.Kty = "EC";
            _jwk.Crv = "P-521";
            _jwk.D = Convert.ToBase64String(parameters.D!);
            _jwk.X = Convert.ToBase64String(parameters.Q.X!);
            _jwk.Y = Convert.ToBase64String(parameters.Q.Y!);

            Debug.WriteLine($"Private Key: {_jwk.D}");
            Debug.WriteLine($"Public Key X: {_jwk.X}");
            Debug.WriteLine($"Public Key Y: {_jwk.Y}");
        }
    }

    // 🔹 Test DeriveAesKeyFromEcdh
    [TestMethod]
    public void DeriveAesKeyFromEcdh_ShouldGenerateValidAesKey()
    {
        // Arrange: Simulated ECDH shared secret
        byte[] sharedSecretBytes = Enumerable.Repeat((byte)0xAB, 66).ToArray(); // 66 bytes for P-521
        BigInteger sharedSecret = new BigInteger(1, sharedSecretBytes); // Ensures positive BigInteger

        int expectedKeySizeBits = 256; // AES-256

        // Act: Derive AES key from ECDH shared secret
        byte[] derivedKey = JweCryptoHelper.DeriveAesKeyFromEcdh(sharedSecret, expectedKeySizeBits);

        // Assert
        Assert.IsNotNull(derivedKey, "Derived key should not be null.");
        Assert.AreEqual(expectedKeySizeBits / 8, derivedKey.Length, "Derived key length should be 32 bytes (256-bit).");

        // Ensure different secrets produce different keys
        byte[] differentSecretBytes = Enumerable.Repeat((byte)0xCD, 66).ToArray();
        BigInteger differentSharedSecret = new BigInteger(1, differentSecretBytes);
        byte[] differentDerivedKey = JweCryptoHelper.DeriveAesKeyFromEcdh(differentSharedSecret, expectedKeySizeBits);

        Assert.IsFalse(derivedKey.SequenceEqual(differentDerivedKey), "Derived keys should be different for different shared secrets.");
    }

    // 🔹 Test GetCurveParameters
    [TestMethod]
    public void GetCurveParameters_ShouldReturnValidParameters()
    {
        // Act & Assert for supported curves
        Assert.IsNotNull(JweCryptoHelper.GetCurveParameters("P-521"), "P-521 should return valid curve parameters.");

        // Ensure unsupported curve throws exception
        Assert.ThrowsException<CryptographicException>(() => JweCryptoHelper.GetCurveParameters("P-999"), "Unknown curve should throw an exception.");
    }

    // 🔹 Test ConvertToPrivateKey with P-521
    [TestMethod]
    public void ConvertToPrivateKey_ShouldReturnValidECPrivateKeyParameters()
    {
        // Arrange: Sample EC Private Key in JWK format (P-521)
        string jwkJson = JsonSerializer.Serialize(_jwk, new JsonSerializerOptions { WriteIndented = true });
        Assert.IsNotNull(jwkJson, "JWK should not be null after deserialization.");

        // Act
        ECPrivateKeyParameters privateKey = JweCryptoHelper.ConvertToPrivateKey(_jwk);

        // Assert
        Assert.IsNotNull(privateKey, "Private key should not be null.");
        Assert.IsTrue(privateKey.D.BitLength > 0, "Private key must have a valid bit length.");
    }

    // 🔹 Test ConvertToPublicKey with P-521
    [TestMethod]
    public void ConvertToPublicKey_ShouldReturnValidECPublicKeyParameters()
    {
        Assert.IsNotNull(_jwk, "JWK should not be null after deserialization.");

        // Act: Convert to BouncyCastle public key
        ECPublicKeyParameters publicKey = JweCryptoHelper.ConvertToPublicKey(_jwk);

        // Assert
        Assert.IsNotNull(publicKey, "Public key should not be null.");
        Assert.IsNotNull(publicKey.Q, "Public key must have a valid ECPoint.");
    }
}
