using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using Microsoft.IdentityModel.Tokens;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nihdi.Common.Pseudonymisation.Jose;

namespace Nihdi.Common.Pseudonymisation.Tests;

[TestClass]
public class CustomJweHandlerTests
{
    private CustomJweHandler _handler = null!;
    private SymmetricSecurityKey _validKey = null!;
    private EncryptingCredentials _validCredentials = null!;

    [TestInitialize]
    public void Setup()
    {
        _handler = new CustomJweHandler();

        // Create a valid 256-bit key
        byte[] keyBytes = new byte[32];
        for (int i = 0; i < keyBytes.Length; i++)
        {
            keyBytes[i] = (byte)i;
        }

        _validKey = new SymmetricSecurityKey(keyBytes);
        _validCredentials = new EncryptingCredentials(_validKey, "dir", "A256GCM");
    }

    [TestMethod]
    public void CreateToken_WithValidInput_ShouldCreateJweToken()
    {
        // Arrange
        string payload = "Test payload data";

        // Act
        string jweToken = _handler.CreateToken(payload, _validCredentials);

        // Assert
        Assert.IsNotNull(jweToken);
        Assert.IsTrue(jweToken.Contains(".."), "JWE token should have empty encrypted key section for 'dir' algorithm");

        string[] parts = jweToken.Split('.');
        Assert.AreEqual(5, parts.Length, "JWE Compact Serialization should have 5 parts");
        Assert.IsTrue(parts[1].Length == 0, "Encrypted key should be empty for 'dir' algorithm");
    }

    [TestMethod]
    public void CreateToken_WithCustomHeaders_ShouldIncludeCustomParams()
    {
        // Arrange
        string payload = "Test payload";
        var customParams = new Dictionary<string, object>
        {
            { "kid", "test-key-id" },
            { "custom", "value" }
        };

        // Act
        string jweToken = _handler.CreateToken(payload, _validCredentials, customParams);

        // Assert
        string[] parts = jweToken.Split('.');
        string headerJson = Encoding.UTF8.GetString(Base64UrlEncoder.DecodeBytes(parts[0]));
        var header = JsonSerializer.Deserialize<Dictionary<string, object>>(headerJson);

        Assert.IsNotNull(header, "Header deserialization resulted in null.");
        Assert.IsTrue(header.ContainsKey("kid"));
        Assert.AreEqual("test-key-id", header["kid"].ToString());
        Assert.IsTrue(header.ContainsKey("custom"));
        Assert.AreEqual("value", header["custom"].ToString());
    }

    [TestMethod]
    public void CreateToken_WithNullCredentials_ShouldThrowArgumentNullException()
    {
        // Arrange
        string payload = "Test payload";

        // Act & Assert
        Assert.ThrowsException<ArgumentNullException>(() =>
            _handler.CreateToken(payload, null!));
    }

    [TestMethod]
    public void CreateToken_WithNonDirAlgorithm_ShouldThrowNotSupportedException()
    {
        // Arrange
        string payload = "Test payload";
        var credentials = new EncryptingCredentials(_validKey, "RSA-OAEP", "A256GCM");

        // Act & Assert
        var ex = Assert.ThrowsException<NotSupportedException>(() =>
            _handler.CreateToken(payload, credentials));

        Assert.IsTrue(ex.Message.Contains("RSA-OAEP"));
    }

    [TestMethod]
    public void CreateToken_WithNonSymmetricKey_ShouldThrowArgumentException()
    {
        // Arrange
        string payload = "Test payload";
        var rsaKey = new RsaSecurityKey(System.Security.Cryptography.RSA.Create());
        var credentials = new EncryptingCredentials(rsaKey, "dir", "A256GCM");

        // Act & Assert
        var ex = Assert.ThrowsException<ArgumentException>(() =>
            _handler.CreateToken(payload, credentials));

        Assert.IsTrue(ex.Message.Contains("SymmetricSecurityKey"));
    }

    [TestMethod]
    public void CreateToken_WithInvalidKeySize_ShouldThrowArgumentException()
    {
        // Arrange
        string payload = "Test payload";
        var invalidKey = new SymmetricSecurityKey(new byte[16]); // 128-bit key instead of 256-bit
        var credentials = new EncryptingCredentials(invalidKey, "dir", "A256GCM");

        // Act & Assert
        var ex = Assert.ThrowsException<ArgumentException>(() =>
            _handler.CreateToken(payload, credentials));

        Assert.IsTrue(ex.Message.Contains("256 bits"), "Exception message should indicate the key size requirement.");
    }

    [TestMethod]
    public void DecryptJwe_WithValidToken_ShouldReturnOriginalPayload()
    {
        // Arrange
        string originalPayload = "This is a test payload for encryption";
        string jweToken = _handler.CreateToken(originalPayload, _validCredentials);

        // Act
        string decryptedPayload = _handler.DecryptJwe(jweToken, _validCredentials);

        // Assert
        Assert.AreEqual(originalPayload, decryptedPayload);
    }

    [TestMethod]
    public void DecryptJwe_WithNullCredentials_ShouldThrowArgumentNullException()
    {
        // Arrange
        string jweToken = "dummy.token.parts.for.test";

        // Act & Assert
        Assert.ThrowsException<ArgumentNullException>(() =>
            _handler.DecryptJwe(jweToken, null!));
    }

    [TestMethod]
    public void DecryptJwe_WithNonDirAlgorithm_ShouldThrowNotSupportedException()
    {
        // Arrange
        string jweToken = "dummy.token.parts.for.test";
        var credentials = new EncryptingCredentials(_validKey, "RSA-OAEP", "A256GCM");

        // Act & Assert
        var ex = Assert.ThrowsException<NotSupportedException>(() =>
            _handler.DecryptJwe(jweToken, credentials));

        Assert.IsTrue(ex.Message.Contains("RSA-OAEP"));
    }

    [TestMethod]
    public void DecryptJwe_WithNonSymmetricKey_ShouldThrowArgumentException()
    {
        // Arrange
        string jweToken = "dummy.token.parts.for.test";
        var rsaKey = new RsaSecurityKey(System.Security.Cryptography.RSA.Create());
        var credentials = new EncryptingCredentials(rsaKey, "dir", "A256GCM");

        // Act & Assert
        var ex = Assert.ThrowsException<ArgumentException>(() =>
            _handler.DecryptJwe(jweToken, credentials));

        Assert.IsTrue(ex.Message.Contains("SymmetricSecurityKey"));
    }

    [TestMethod]
    public void DecryptJwe_WithInvalidTokenFormat_ShouldThrowArgumentException()
    {
        // Arrange
        string invalidToken = "not.enough.parts";

        // Act & Assert
        var ex = Assert.ThrowsException<ArgumentException>(() =>
            _handler.DecryptJwe(invalidToken, _validCredentials));

        Assert.IsTrue(ex.Message.Contains("Invalid JWE token format"));
    }

    [TestMethod]
    public void DecryptJwe_WithCorruptedToken_ShouldThrowException()
    {
        // Arrange
        string originalPayload = "Test payload";
        string jweToken = _handler.CreateToken(originalPayload, _validCredentials);

        // Corrupt the ciphertext part
        string[] parts = jweToken.Split('.');
        parts[3] = Base64UrlEncoder.Encode(new byte[] { 1, 2, 3, 4 }); // Invalid ciphertext
        string corruptedToken = string.Join(".", parts);

        // Act & Assert
        Assert.ThrowsException<JweDecryptionException>(() =>
            _handler.DecryptJwe(corruptedToken, _validCredentials));
    }

    [TestMethod]
    public void RoundTrip_WithVariousPayloads_ShouldWorkCorrectly()
    {
        // Arrange
        string[] testPayloads =
        {
            "Simple text",
            "Text with special characters: !@#$%^&*()",
            "Unicode text: 你好世界 🌍",
            "{\"json\":\"payload\",\"with\":\"nested\",\"data\":[1,2,3]}",
            new string('A', 1000), // Large payload
            string.Empty // Empty payload
        };

        foreach (var payload in testPayloads)
        {
            // Act
            string jweToken = _handler.CreateToken(payload, _validCredentials);
            string decrypted = _handler.DecryptJwe(jweToken, _validCredentials);

            // Assert
            Assert.AreEqual(payload, decrypted, $"Failed for payload: {payload}");
        }
    }

    [TestMethod]
    public void CreateToken_HeaderStructure_ShouldBeCorrect()
    {
        // Arrange
        string payload = "Test";

        // Act
        string jweToken = _handler.CreateToken(payload, _validCredentials);
        string[] parts = jweToken.Split('.');
        string headerJson = Encoding.UTF8.GetString(Base64UrlEncoder.DecodeBytes(parts[0]));
        var header = JsonSerializer.Deserialize<Dictionary<string, object>>(headerJson);

        // Assert
        Assert.IsNotNull(header, "Header deserialization resulted in null."); // Ensure header is not null
        Assert.IsTrue(header.ContainsKey("alg"));
        Assert.AreEqual("dir", header["alg"].ToString());
        Assert.IsTrue(header.ContainsKey("enc"));
        Assert.AreEqual("A256GCM", header["enc"].ToString());
        Assert.IsTrue(header.ContainsKey("typ"));
        Assert.AreEqual("JWT", header["typ"].ToString());
    }

    [TestMethod]
    public void DecryptJwe_WithDifferentKey_ShouldFail()
    {
        // Arrange
        string payload = "Secret data";
        string jweToken = _handler.CreateToken(payload, _validCredentials);

        // Create different key
        byte[] differentKeyBytes = new byte[32];
        for (int i = 0; i < differentKeyBytes.Length; i++)
        {
            differentKeyBytes[i] = (byte)(i + 100);
        }

        var differentKey = new SymmetricSecurityKey(differentKeyBytes);
        var differentCredentials = new EncryptingCredentials(differentKey, "dir", "A256GCM");

        // Act & Assert
        Assert.ThrowsException<JweDecryptionException>(() =>
            _handler.DecryptJwe(jweToken, differentCredentials));
    }
}