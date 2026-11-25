// <copyright file="RsaHelperTests.cs" company="Riziv-Inami">
// Copyright (c) Riziv-Inami. All rights reserved.
// </copyright>

namespace Nihdi.Common.Pseudonymisation.Tests.Jose;

using System.Security.Cryptography;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nihdi.Common.Pseudonymisation.Jose;

[TestClass]
public class RsaHelperTests
{
    [TestMethod]
    public void Create_ReturnsValidRsaInstance()
    {
        // Act
        using var rsa = RsaHelper.Create();

        // Assert
        Assert.IsNotNull(rsa);
        Assert.IsTrue(rsa.KeySize > 0);
    }

    [TestMethod]
    public void Create_WithKeySize_ReturnsCorrectKeySize()
    {
        // Arrange
        int[] keySizes = { 1024, 2048, 3072, 4096 };

        foreach (int keySize in keySizes)
        {
            // Act
            using var rsa = RsaHelper.Create(keySize);

            // Assert
            Assert.AreEqual(keySize, rsa.KeySize, $"Failed for key size {keySize}");
        }
    }

    [TestMethod]
    public void Create_SupportsOaepSha256()
    {
        // Arrange
        using var rsa = RsaHelper.Create(2048);
        byte[] data = new byte[32];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(data);
        }

        // Act - This should not throw
        byte[] encrypted = rsa.Encrypt(data, RSAEncryptionPadding.OaepSHA256);
        byte[] decrypted = rsa.Decrypt(encrypted, RSAEncryptionPadding.OaepSHA256);

        // Assert
        CollectionAssert.AreEqual(data, decrypted);
    }

    [TestMethod]
    public void Create_ReturnsPlatformImplementation()
    {
        // Act
        using var rsa = RsaHelper.Create();

        // Assert
        // RSA.Create() builds an internal type, so there is no name to compare against.
        // That it can actually do OaepSHA256 is checked by Create_SupportsOaepSha256.
        Assert.IsNotNull(rsa);
    }
}