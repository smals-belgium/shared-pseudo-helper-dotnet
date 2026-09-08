// <copyright file="JweRecipientHeaderTests.cs" company="Riziv-Inami">
// Copyright (c) Riziv-Inami. All rights reserved.
// </copyright>

namespace Nihdi.Common.Pseudonymisation.Tests.Jose;

using System.Collections.Generic;
using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nihdi.Common.Pseudonymisation.Jose;

[TestClass]
public class JweRecipientHeaderTests
{
    [TestMethod]
    public void Constructor_InitializesPropertiesWithEmptyStrings()
    {
        // Act
        var header = new JweRecipientHeader();

        // Assert
        Assert.AreEqual(string.Empty, header.Jku);
        Assert.AreEqual(string.Empty, header.Alg);
        Assert.AreEqual(string.Empty, header.Kid);
    }

    [TestMethod]
    public void ToDictionary_WithAllPropertiesSet_ReturnsAllProperties()
    {
        // Arrange
        var header = new JweRecipientHeader
        {
            Alg = "RSA-OAEP",
            Jku = "https://example.com/keys.json",
            Kid = "key-123"
        };

        // Act
        var dict = header.ToDictionary();

        // Assert
        Assert.AreEqual(3, dict.Count);
        Assert.AreEqual("RSA-OAEP", dict["alg"]);
        Assert.AreEqual("https://example.com/keys.json", dict["jku"]);
        Assert.AreEqual("key-123", dict["kid"]);
    }

    [TestMethod]
    public void ToDictionary_WithEmptyProperties_ExcludesEmptyProperties()
    {
        // Arrange
        var header = new JweRecipientHeader
        {
            Alg = string.Empty,
            Jku = string.Empty,
            Kid = string.Empty
        };

        // Act
        var dict = header.ToDictionary();

        // Assert
        Assert.AreEqual(0, dict.Count);
    }

    [TestMethod]
    public void ToDictionary_WithNullProperties_ExcludesNullProperties()
    {
        // Arrange
        var header = new JweRecipientHeader
        {
            Alg = null!,
            Jku = null!,
            Kid = null!
        };

        // Act
        var dict = header.ToDictionary();

        // Assert
        Assert.AreEqual(0, dict.Count);
    }

    [TestMethod]
    public void ToDictionary_WithOnlyAlgSet_ReturnsOnlyAlg()
    {
        // Arrange
        var header = new JweRecipientHeader
        {
            Alg = "dir",
            Jku = string.Empty,
            Kid = string.Empty
        };

        // Act
        var dict = header.ToDictionary();

        // Assert
        Assert.AreEqual(1, dict.Count);
        Assert.IsTrue(dict.ContainsKey("alg"));
        Assert.AreEqual("dir", dict["alg"]);
        Assert.IsFalse(dict.ContainsKey("jku"));
        Assert.IsFalse(dict.ContainsKey("kid"));
    }

    [TestMethod]
    public void ToDictionary_WithOnlyJkuSet_ReturnsOnlyJku()
    {
        // Arrange
        var header = new JweRecipientHeader
        {
            Alg = string.Empty,
            Jku = "https://example.com/jwks",
            Kid = string.Empty
        };

        // Act
        var dict = header.ToDictionary();

        // Assert
        Assert.AreEqual(1, dict.Count);
        Assert.IsTrue(dict.ContainsKey("jku"));
        Assert.AreEqual("https://example.com/jwks", dict["jku"]);
        Assert.IsFalse(dict.ContainsKey("alg"));
        Assert.IsFalse(dict.ContainsKey("kid"));
    }

    [TestMethod]
    public void ToDictionary_WithOnlyKidSet_ReturnsOnlyKid()
    {
        // Arrange
        var header = new JweRecipientHeader
        {
            Alg = string.Empty,
            Jku = string.Empty,
            Kid = "my-key-id"
        };

        // Act
        var dict = header.ToDictionary();

        // Assert
        Assert.AreEqual(1, dict.Count);
        Assert.IsTrue(dict.ContainsKey("kid"));
        Assert.AreEqual("my-key-id", dict["kid"]);
        Assert.IsFalse(dict.ContainsKey("alg"));
        Assert.IsFalse(dict.ContainsKey("jku"));
    }

    [TestMethod]
    public void ToDictionary_WithWhitespaceProperties_IncludesWhitespaceProperties()
    {
        // Arrange
        var header = new JweRecipientHeader
        {
            Alg = "  ",
            Jku = "\t",
            Kid = "\n"
        };

        // Act
        var dict = header.ToDictionary();

        // Assert
        Assert.AreEqual(3, dict.Count);
        Assert.AreEqual("  ", dict["alg"]);
        Assert.AreEqual("\t", dict["jku"]);
        Assert.AreEqual("\n", dict["kid"]);
    }

    [TestMethod]
    public void ToDictionary_WithMixedProperties_ReturnsOnlyNonEmptyProperties()
    {
        // Arrange
        var header = new JweRecipientHeader
        {
            Alg = "A256GCM",
            Jku = string.Empty,
            Kid = "test-key"
        };

        // Act
        var dict = header.ToDictionary();

        // Assert
        Assert.AreEqual(2, dict.Count);
        Assert.AreEqual("A256GCM", dict["alg"]);
        Assert.AreEqual("test-key", dict["kid"]);
        Assert.IsFalse(dict.ContainsKey("jku"));
    }

    [TestMethod]
    public void ToDictionary_CalledMultipleTimes_ReturnsNewInstanceEachTime()
    {
        // Arrange
        var header = new JweRecipientHeader
        {
            Alg = "RSA-OAEP",
            Kid = "key-1"
        };

        // Act
        var dict1 = header.ToDictionary();
        var dict2 = header.ToDictionary();

        // Assert
        Assert.AreNotSame(dict1, dict2, "Should return new dictionary instances");
        CollectionAssert.AreEqual(dict1, dict2, "Contents should be equal");

        // Modify one dictionary
        dict1["alg"] = "modified";

        // Verify other dictionary is not affected
        Assert.AreEqual("RSA-OAEP", dict2["alg"]);
    }

    [TestMethod]
    public void JsonSerialization_WithAllProperties_SerializesCorrectly()
    {
        // Arrange
        var header = new JweRecipientHeader
        {
            Alg = "RSA-OAEP-256",
            Jku = "https://server.example.com/keys.jwks",
            Kid = "2011-04-29"
        };

        // Act
        var json = JsonSerializer.Serialize(header);
        var deserialized = JsonSerializer.Deserialize<JweRecipientHeader>(json);

        // Assert
        Assert.IsNotNull(deserialized);
        Assert.AreEqual(header.Alg, deserialized.Alg);
        Assert.AreEqual(header.Jku, deserialized.Jku);
        Assert.AreEqual(header.Kid, deserialized.Kid);
    }

    [TestMethod]
    public void JsonSerialization_UsesCorrectPropertyNames()
    {
        // Arrange
        var header = new JweRecipientHeader
        {
            Alg = "dir",
            Jku = "https://example.com/keys",
            Kid = "key123"
        };

        // Act
        var json = JsonSerializer.Serialize(header);

        // Assert
        Assert.IsTrue(json.Contains("\"alg\":\"dir\""));
        Assert.IsTrue(json.Contains("\"jku\":\"https://example.com/keys\""));
        Assert.IsTrue(json.Contains("\"kid\":\"key123\""));
    }

    [TestMethod]
    public void JsonDeserialization_FromValidJson_DeserializesCorrectly()
    {
        // Arrange
        var json = "{\"alg\":\"A128KW\",\"jku\":\"https://example.org/jwks.json\",\"kid\":\"e9bc097a-ce51-4036-9562-d2ade882db0d\"}";

        // Act
        var header = JsonSerializer.Deserialize<JweRecipientHeader>(json);

        // Assert
        Assert.IsNotNull(header);
        Assert.AreEqual("A128KW", header.Alg);
        Assert.AreEqual("https://example.org/jwks.json", header.Jku);
        Assert.AreEqual("e9bc097a-ce51-4036-9562-d2ade882db0d", header.Kid);
    }

    [TestMethod]
    public void JsonDeserialization_WithMissingProperties_SetsToEmptyStrings()
    {
        // Arrange
        var json = "{\"alg\":\"RSA1_5\"}";

        // Act
        var header = JsonSerializer.Deserialize<JweRecipientHeader>(json);

        // Assert
        Assert.IsNotNull(header);
        Assert.AreEqual("RSA1_5", header.Alg);
        Assert.AreEqual(string.Empty, header.Jku);
        Assert.AreEqual(string.Empty, header.Kid);
    }

    [TestMethod]
    public void JsonDeserialization_EmptyJson_CreatesObjectWithEmptyProperties()
    {
        // Arrange
        var json = "{}";

        // Act
        var header = JsonSerializer.Deserialize<JweRecipientHeader>(json);

        // Assert
        Assert.IsNotNull(header);
        Assert.AreEqual(string.Empty, header.Alg);
        Assert.AreEqual(string.Empty, header.Jku);
        Assert.AreEqual(string.Empty, header.Kid);
    }
}