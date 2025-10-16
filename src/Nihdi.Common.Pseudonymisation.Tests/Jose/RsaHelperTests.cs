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
    public void Create_OnNetFramework_ReturnsRsaCng()
    {
#if NETFRAMEWORK
        // Act
        using var rsa = RsaHelper.Create();

        // Assert
        Assert.IsInstanceOfType(rsa, typeof(RSACng));
#else
        // This test only runs on .NET Framework
        Assert.Inconclusive("This test is only for .NET Framework");
#endif
    }

    [TestMethod]
    public void Create_OnNetCore_ReturnsValidRsa()
    {
#if !NETFRAMEWORK
        // Act
        using var rsa = RsaHelper.Create();

        // Assert
        Assert.IsNotNull(rsa);
        // On .NET Core/5+, RSA.Create() returns an internal type
        // We can't check the exact type, but we can verify it works
        Assert.IsTrue(rsa.KeySize > 0);
#else
        // This test only runs on .NET Core/5+
        Assert.Inconclusive("This test is only for .NET Core/5+");
#endif
    }
}