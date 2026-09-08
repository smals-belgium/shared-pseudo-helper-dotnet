// <copyright file="JweDecryptionTests.cs" company="Riziv-Inami">
// Copyright (c) Riziv-Inami. All rights reserved.
// </copyright>

namespace Nihdi.Common.Pseudonymisation.Tests.Jose;

using System;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nihdi.Common.Pseudonymisation.Jose;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Parameters;

[TestClass]
public class JweDecryptionTests
{
    private RSA _rsa = null!;
    private RsaSecurityKey _rsaKey = null!;
    private byte[] _testCek = null!;
    private byte[] _encryptedCek = null!;

    [TestInitialize]
    public void Setup()
    {
        // Generate RSA key pair using standard .NET API
        // This ensures we test against the actual platform behavior, not our abstraction
#if NETFRAMEWORK
        _rsa = new RSACng(2048);  // Explicitly use RSACng on .NET Framework
#else
        _rsa = RSA.Create(2048);   // Use default on .NET 8+
#endif

        // Export parameters to create RsaSecurityKey
        var rsaParameters = _rsa.ExportParameters(true);
        _rsaKey = new RsaSecurityKey(rsaParameters);

        // Generate test CEK (256-bit for AES-256)
        _testCek = new byte[32];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(_testCek);
        }

        // Encrypt CEK with RSA public key
        _encryptedCek = _rsa.Encrypt(_testCek, RSAEncryptionPadding.OaepSHA256);
    }

    [TestCleanup]
    public void Cleanup()
    {
        _rsa?.Dispose();
    }

    [TestMethod]
    public void UnwrapKey_WithValidRsaKey_ReturnsOriginalCek()
    {
        // Act
        byte[] unwrappedCek = JweDecryption.UnwrapKey(_encryptedCek, _rsaKey);

        // Assert
        CollectionAssert.AreEqual(_testCek, unwrappedCek);
    }

    [TestMethod]
    public void UnwrapKey_WithNonRsaKey_ThrowsNotSupportedException()
    {
        // Arrange
        var symmetricKey = new SymmetricSecurityKey(new byte[32]);

        // Act & Assert
        var exception = Assert.ThrowsException<NotSupportedException>(() =>
            JweDecryption.UnwrapKey(_encryptedCek, symmetricKey));

        Assert.AreEqual("Only RSA key unwrapping is supported.", exception.Message);
    }

    [TestMethod]
    public void UnwrapKey_WithDifferentRsaKey_ThrowsCryptographicException()
    {
        // Arrange - Use platform-specific RSA creation directly
#if NETFRAMEWORK
        using var differentRsa = new RSACng(2048);
#else
        using var differentRsa = RSA.Create(2048);
#endif
        var differentRsaParameters = differentRsa.ExportParameters(true);
        var differentKey = new RsaSecurityKey(differentRsaParameters);

        // Act & Assert
        Assert.ThrowsException<CryptographicException>(() =>
            JweDecryption.UnwrapKey(_encryptedCek, differentKey));
    }

    [TestMethod]
    public void UnwrapKey_WithPublicKeyOnly_ThrowsCryptographicException()
    {
        // Arrange - Create RSA key with public parameters only
        var publicParameters = _rsa.ExportParameters(false);
        var publicOnlyKey = new RsaSecurityKey(publicParameters);

        // Act & Assert
        Assert.ThrowsException<CryptographicException>(() =>
            JweDecryption.UnwrapKey(_encryptedCek, publicOnlyKey));
    }

    [TestMethod]
    public void DecryptAesGcm_WithValidInputs_DecryptsSuccessfully()
    {
        // Arrange
        string plaintext = "Hello, World! This is a test message.";
        byte[] plaintextBytes = Encoding.UTF8.GetBytes(plaintext);
        byte[] aad = Encoding.UTF8.GetBytes("additional authenticated data");
        byte[] cek = new byte[32]; // 256-bit key
        byte[] iv = new byte[12];  // 96-bit IV for GCM

        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(cek);
            rng.GetBytes(iv);
        }

        // Encrypt using BouncyCastle (to have known good ciphertext)
        byte[] ciphertext;
        byte[] authTag;
        var gcm = new GcmBlockCipher(new AesEngine());
        var parameters = new AeadParameters(new KeyParameter(cek), 128, iv, aad);
        gcm.Init(true, parameters);

        byte[] cipherTextWithTag = new byte[gcm.GetOutputSize(plaintextBytes.Length)];
        int len = gcm.ProcessBytes(plaintextBytes, 0, plaintextBytes.Length, cipherTextWithTag, 0);
        gcm.DoFinal(cipherTextWithTag, len);

        // Split ciphertext and tag
        ciphertext = new byte[plaintextBytes.Length];
        authTag = new byte[16]; // 128-bit tag
        Array.Copy(cipherTextWithTag, 0, ciphertext, 0, ciphertext.Length);
        Array.Copy(cipherTextWithTag, ciphertext.Length, authTag, 0, authTag.Length);

        // Act
        byte[] decrypted = JweDecryption.DecryptAesGcm(aad, cek, iv, ciphertext, authTag);

        // Assert
        string decryptedText = Encoding.UTF8.GetString(decrypted);
        Assert.AreEqual(plaintext, decryptedText);
    }

    [TestMethod]
    public void DecryptAesGcm_WithNullAad_ThrowsArgumentNullException()
    {
        // Arrange
        byte[] cek = new byte[32];
        byte[] iv = new byte[12];
        byte[] ciphertext = new byte[10];
        byte[] authTag = new byte[16];

        // Act & Assert
        var exception = Assert.ThrowsException<ArgumentNullException>(() =>
            JweDecryption.DecryptAesGcm(null!, cek, iv, ciphertext, authTag));

        Assert.AreEqual("aad", exception.ParamName);
    }

    [TestMethod]
    public void DecryptAesGcm_WithNullCek_ThrowsArgumentNullException()
    {
        // Arrange
        byte[] aad = new byte[10];
        byte[] iv = new byte[12];
        byte[] ciphertext = new byte[10];
        byte[] authTag = new byte[16];

        // Act & Assert
        var exception = Assert.ThrowsException<ArgumentNullException>(() =>
            JweDecryption.DecryptAesGcm(aad, null!, iv, ciphertext, authTag));

        Assert.AreEqual("cek", exception.ParamName);
    }

    [TestMethod]
    public void DecryptAesGcm_WithNullIv_ThrowsArgumentNullException()
    {
        // Arrange
        byte[] aad = new byte[10];
        byte[] cek = new byte[32];
        byte[] ciphertext = new byte[10];
        byte[] authTag = new byte[16];

        // Act & Assert
        var exception = Assert.ThrowsException<ArgumentNullException>(() =>
            JweDecryption.DecryptAesGcm(aad, cek, null!, ciphertext, authTag));

        Assert.AreEqual("iv", exception.ParamName);
    }

    [TestMethod]
    public void DecryptAesGcm_WithNullCiphertext_ThrowsArgumentNullException()
    {
        // Arrange
        byte[] aad = new byte[10];
        byte[] cek = new byte[32];
        byte[] iv = new byte[12];
        byte[] authTag = new byte[16];

        // Act & Assert
        var exception = Assert.ThrowsException<ArgumentNullException>(() =>
            JweDecryption.DecryptAesGcm(aad, cek, iv, null!, authTag));

        Assert.AreEqual("ciphertext", exception.ParamName);
    }

    [TestMethod]
    public void DecryptAesGcm_WithNullAuthTag_ThrowsArgumentNullException()
    {
        // Arrange
        byte[] aad = new byte[10];
        byte[] cek = new byte[32];
        byte[] iv = new byte[12];
        byte[] ciphertext = new byte[10];

        // Act & Assert
        var exception = Assert.ThrowsException<ArgumentNullException>(() =>
            JweDecryption.DecryptAesGcm(aad, cek, iv, ciphertext, null!));

        Assert.AreEqual("authTag", exception.ParamName);
    }

    [TestMethod]
    public void DecryptAesGcm_WithInvalidAuthTag_ThrowsException()
    {
        // Arrange
        byte[] aad = Encoding.UTF8.GetBytes("aad");
        byte[] cek = new byte[32];
        byte[] iv = new byte[12];
        byte[] ciphertext = new byte[10];
        byte[] authTag = new byte[16];

        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(cek);
            rng.GetBytes(iv);
            rng.GetBytes(ciphertext);
            rng.GetBytes(authTag); // Random tag won't match
        }

        // Act & Assert
        var exception = Assert.ThrowsException<Exception>(() =>
            JweDecryption.DecryptAesGcm(aad, cek, iv, ciphertext, authTag));

        Assert.AreEqual("AuthenticationException failed: Invalid ciphertext or tag.", exception.Message);
    }

    [TestMethod]
    public void DecryptAesGcm_WithWrongKey_ThrowsException()
    {
        // Arrange - First encrypt with one key
        string plaintext = "Test message";
        byte[] plaintextBytes = Encoding.UTF8.GetBytes(plaintext);
        byte[] aad = Encoding.UTF8.GetBytes("aad");
        byte[] correctCek = new byte[32];
        byte[] wrongCek = new byte[32];
        byte[] iv = new byte[12];

        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(correctCek);
            rng.GetBytes(wrongCek);
            rng.GetBytes(iv);
        }

        // Encrypt with correct key
        var gcm = new GcmBlockCipher(new AesEngine());
        var parameters = new AeadParameters(new KeyParameter(correctCek), 128, iv, aad);
        gcm.Init(true, parameters);

        byte[] cipherTextWithTag = new byte[gcm.GetOutputSize(plaintextBytes.Length)];
        int len = gcm.ProcessBytes(plaintextBytes, 0, plaintextBytes.Length, cipherTextWithTag, 0);
        gcm.DoFinal(cipherTextWithTag, len);

        byte[] ciphertext = new byte[plaintextBytes.Length];
        byte[] authTag = new byte[16];
        Array.Copy(cipherTextWithTag, 0, ciphertext, 0, ciphertext.Length);
        Array.Copy(cipherTextWithTag, ciphertext.Length, authTag, 0, authTag.Length);

        // Act & Assert - Decrypt with wrong key
        var exception = Assert.ThrowsException<Exception>(() =>
            JweDecryption.DecryptAesGcm(aad, wrongCek, iv, ciphertext, authTag));

        Assert.AreEqual("AuthenticationException failed: Invalid ciphertext or tag.", exception.Message);
    }

    [TestMethod]
    public void DecryptAesGcm_WithEmptyPlaintext_DecryptsSuccessfully()
    {
        // Arrange
        byte[] plaintextBytes = new byte[0];
        byte[] aad = Encoding.UTF8.GetBytes("aad");
        byte[] cek = new byte[32];
        byte[] iv = new byte[12];

        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(cek);
            rng.GetBytes(iv);
        }

        // Encrypt empty plaintext
        var gcm = new GcmBlockCipher(new AesEngine());
        var parameters = new AeadParameters(new KeyParameter(cek), 128, iv, aad);
        gcm.Init(true, parameters);

        byte[] cipherTextWithTag = new byte[gcm.GetOutputSize(0)];
        int len = gcm.ProcessBytes(plaintextBytes, 0, 0, cipherTextWithTag, 0);
        gcm.DoFinal(cipherTextWithTag, len);

        byte[] ciphertext = new byte[0];
        byte[] authTag = new byte[16];
        Array.Copy(cipherTextWithTag, 0, authTag, 0, authTag.Length);

        // Act
        byte[] decrypted = JweDecryption.DecryptAesGcm(aad, cek, iv, ciphertext, authTag);

        // Assert
        Assert.AreEqual(0, decrypted.Length);
    }

    [TestMethod]
    public void DecryptAesGcm_WithDifferentTagSizes_WorksCorrectly()
    {
        // Test with different authentication tag sizes (96, 104, 112, 120, 128 bits)
        int[] tagSizes = { 96, 104, 112, 120, 128 };

        foreach (int tagSizeBits in tagSizes)
        {
            // Arrange
            int tagSizeBytes = tagSizeBits / 8;
            string plaintext = $"Test with {tagSizeBits}-bit tag";
            byte[] plaintextBytes = Encoding.UTF8.GetBytes(plaintext);
            byte[] aad = Encoding.UTF8.GetBytes("aad");
            byte[] cek = new byte[32];
            byte[] iv = new byte[12];

            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(cek);
                rng.GetBytes(iv);
            }

            // Encrypt with specific tag size
            var gcm = new GcmBlockCipher(new AesEngine());
            var parameters = new AeadParameters(new KeyParameter(cek), tagSizeBits, iv, aad);
            gcm.Init(true, parameters);

            byte[] cipherTextWithTag = new byte[gcm.GetOutputSize(plaintextBytes.Length)];
            int len = gcm.ProcessBytes(plaintextBytes, 0, plaintextBytes.Length, cipherTextWithTag, 0);
            gcm.DoFinal(cipherTextWithTag, len);

            byte[] ciphertext = new byte[plaintextBytes.Length];
            byte[] authTag = new byte[tagSizeBytes];
            Array.Copy(cipherTextWithTag, 0, ciphertext, 0, ciphertext.Length);
            Array.Copy(cipherTextWithTag, ciphertext.Length, authTag, 0, authTag.Length);

            // Act
            byte[] decrypted = JweDecryption.DecryptAesGcm(aad, cek, iv, ciphertext, authTag);

            // Assert
            string decryptedText = Encoding.UTF8.GetString(decrypted);
            Assert.AreEqual(plaintext, decryptedText, $"Failed for tag size {tagSizeBits} bits");
        }
    }

    [TestMethod]
    public void DecryptAesGcm_WithEmptyAad_DecryptsSuccessfully()
    {
        // Arrange
        string plaintext = "Test with empty AAD";
        byte[] plaintextBytes = Encoding.UTF8.GetBytes(plaintext);
        byte[] aad = new byte[0]; // Empty AAD
        byte[] cek = new byte[32];
        byte[] iv = new byte[12];

        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(cek);
            rng.GetBytes(iv);
        }

        // Encrypt
        var gcm = new GcmBlockCipher(new AesEngine());
        var parameters = new AeadParameters(new KeyParameter(cek), 128, iv, aad);
        gcm.Init(true, parameters);

        byte[] cipherTextWithTag = new byte[gcm.GetOutputSize(plaintextBytes.Length)];
        int len = gcm.ProcessBytes(plaintextBytes, 0, plaintextBytes.Length, cipherTextWithTag, 0);
        gcm.DoFinal(cipherTextWithTag, len);

        byte[] ciphertext = new byte[plaintextBytes.Length];
        byte[] authTag = new byte[16];
        Array.Copy(cipherTextWithTag, 0, ciphertext, 0, ciphertext.Length);
        Array.Copy(cipherTextWithTag, ciphertext.Length, authTag, 0, authTag.Length);

        // Act
        byte[] decrypted = JweDecryption.DecryptAesGcm(aad, cek, iv, ciphertext, authTag);

        // Assert
        string decryptedText = Encoding.UTF8.GetString(decrypted);
        Assert.AreEqual(plaintext, decryptedText);
    }

    [TestMethod]
    public void DecryptAesGcm_WithLargeData_DecryptsSuccessfully()
    {
        // Arrange - Test with 10KB of data
        byte[] plaintextBytes = new byte[10 * 1024];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(plaintextBytes);
        }

        byte[] aad = Encoding.UTF8.GetBytes("Large data test AAD");
        byte[] cek = new byte[32];
        byte[] iv = new byte[12];

        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(cek);
            rng.GetBytes(iv);
        }

        // Encrypt
        var gcm = new GcmBlockCipher(new AesEngine());
        var parameters = new AeadParameters(new KeyParameter(cek), 128, iv, aad);
        gcm.Init(true, parameters);

        byte[] cipherTextWithTag = new byte[gcm.GetOutputSize(plaintextBytes.Length)];
        int len = gcm.ProcessBytes(plaintextBytes, 0, plaintextBytes.Length, cipherTextWithTag, 0);
        gcm.DoFinal(cipherTextWithTag, len);

        byte[] ciphertext = new byte[plaintextBytes.Length];
        byte[] authTag = new byte[16];
        Array.Copy(cipherTextWithTag, 0, ciphertext, 0, ciphertext.Length);
        Array.Copy(cipherTextWithTag, ciphertext.Length, authTag, 0, authTag.Length);

        // Act
        byte[] decrypted = JweDecryption.DecryptAesGcm(aad, cek, iv, ciphertext, authTag);

        // Assert
        CollectionAssert.AreEqual(plaintextBytes, decrypted);
    }

    [TestMethod]
    public void DecryptAesGcm_WithModifiedAad_ThrowsException()
    {
        // Arrange
        string plaintext = "Test message";
        byte[] plaintextBytes = Encoding.UTF8.GetBytes(plaintext);
        byte[] originalAad = Encoding.UTF8.GetBytes("original aad");
        byte[] modifiedAad = Encoding.UTF8.GetBytes("modified aad");
        byte[] cek = new byte[32];
        byte[] iv = new byte[12];

        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(cek);
            rng.GetBytes(iv);
        }

        // Encrypt with original AAD
        var gcm = new GcmBlockCipher(new AesEngine());
        var parameters = new AeadParameters(new KeyParameter(cek), 128, iv, originalAad);
        gcm.Init(true, parameters);

        byte[] cipherTextWithTag = new byte[gcm.GetOutputSize(plaintextBytes.Length)];
        int len = gcm.ProcessBytes(plaintextBytes, 0, plaintextBytes.Length, cipherTextWithTag, 0);
        gcm.DoFinal(cipherTextWithTag, len);

        byte[] ciphertext = new byte[plaintextBytes.Length];
        byte[] authTag = new byte[16];
        Array.Copy(cipherTextWithTag, 0, ciphertext, 0, ciphertext.Length);
        Array.Copy(cipherTextWithTag, ciphertext.Length, authTag, 0, authTag.Length);

        // Act & Assert - Decrypt with modified AAD (should fail)
        var exception = Assert.ThrowsException<Exception>(() =>
            JweDecryption.DecryptAesGcm(modifiedAad, cek, iv, ciphertext, authTag));

        Assert.AreEqual("AuthenticationException failed: Invalid ciphertext or tag.", exception.Message);
    }

    [TestMethod]
    public void UnwrapKey_RoundTrip_WithDifferentKeySizes_WorksCorrectly()
    {
        // Test with different RSA key sizes
        int[] keySizes = { 2048, 3072 };

        foreach (int keySize in keySizes)
        {
            // Arrange - Use platform-specific RSA creation directly
#if NETFRAMEWORK
            using var rsa = new RSACng(keySize);
#else
            using var rsa = RSA.Create(keySize);
#endif
            var rsaParameters = rsa.ExportParameters(true);
            var rsaKey = new RsaSecurityKey(rsaParameters);

            byte[] originalCek = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(originalCek);
            }

            // Encrypt the CEK
            byte[] encryptedCek = rsa.Encrypt(originalCek, RSAEncryptionPadding.OaepSHA256);

            // Act - Unwrap the CEK
            byte[] unwrappedCek = JweDecryption.UnwrapKey(encryptedCek, rsaKey);

            // Assert
            CollectionAssert.AreEqual(originalCek, unwrappedCek, $"Round trip failed for RSA key size {keySize}");
        }
    }
}