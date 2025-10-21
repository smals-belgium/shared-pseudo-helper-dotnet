using System.Text;
using System.Text.Json;
using Microsoft.IdentityModel.Tokens;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nihdi.Common.Pseudonymisation.Jose;

namespace Nihdi.Common.Pseudonymisation.Tests.Jose;

[TestClass]
public class ByteArrayBase64ConverterTests
{
    private JsonSerializerOptions _options = null!;

    [TestInitialize]
    public void Setup()
    {
        _options = new JsonSerializerOptions();
        _options.Converters.Add(new ByteArrayBase64Converter());
    }

    [TestMethod]
    public void Write_ValidByteArray_EncodesToBase64Url()
    {
        byte[] input = Encoding.UTF8.GetBytes("Hello World!");
        string expected = Base64UrlEncoder.Encode(input);

        string json = JsonSerializer.Serialize(input, _options);
        Assert.AreEqual($"\"{expected}\"", json);
    }

    [TestMethod]
    public void Write_EmptyByteArray_EncodesToEmptyString()
    {
        byte[] input = [];
        string json = JsonSerializer.Serialize(input, _options);
        Assert.AreEqual("\"\"", json);
    }

    [TestMethod]
    public void Write_NullByteArray_WritesNull()
    {
        byte[]? input = null;
        string json = JsonSerializer.Serialize(input, _options);
        Assert.AreEqual("null", json);
    }

    [TestMethod]
    public void Read_ValidBase64Url_DecodesToByteArray()
    {
        byte[] expected = Encoding.UTF8.GetBytes("Hello World!");
        string base64Url = Base64UrlEncoder.Encode(expected);
        string json = $"\"{base64Url}\"";

        byte[]? actual = JsonSerializer.Deserialize<byte[]>(json, _options);
        CollectionAssert.AreEqual(expected, actual);
    }

    [TestMethod]
    public void Read_EmptyString_ReturnsNull()
    {
        string json = "\"\"";
        byte[]? actual = JsonSerializer.Deserialize<byte[]>(json, _options);
        Assert.IsNull(actual);
    }

    [TestMethod]
    public void Read_NullValue_ReturnsNull()
    {
        string json = "null";
        byte[]? actual = JsonSerializer.Deserialize<byte[]>(json, _options);
        Assert.IsNull(actual);
    }

    [TestMethod]
    [ExpectedException(typeof(JsonException))]
    public void Read_InvalidBase64Url_ThrowsJsonException()
    {
        string json = "\"Not@Base64!\"";
        JsonSerializer.Deserialize<byte[]>(json, _options);
    }

    [TestMethod]
    public void Read_NonStringToken_ReturnsNull()
    {
        string json = "123";
        byte[]? actual = JsonSerializer.Deserialize<byte[]>(json, _options);
        Assert.IsNull(actual);
    }

    [TestMethod]
    public void RoundTrip_ByteArray_PreservesData()
    {
        var testCases = new[]
        {
            new byte[] { 0, 1, 2, 3, 4, 5 },
            new byte[] { 255, 254, 253 },
            Encoding.UTF8.GetBytes("Special chars: éàù€"),
            new byte[] { 0x00, 0xFF, 0x7F, 0x80 }
        };

        foreach (var original in testCases)
        {
            string json = JsonSerializer.Serialize(original, _options);
            byte[]? decoded = JsonSerializer.Deserialize<byte[]>(json, _options);
            CollectionAssert.AreEqual(original, decoded);
        }
    }

    [TestMethod]
    public void Serialize_ByteArrayProperty_InObject_WorksCorrectly()
    {
        var testObj = new TestClass
        {
            Id = 42,
            Data = Encoding.UTF8.GetBytes("Test Data")
        };

        string json = JsonSerializer.Serialize(testObj, _options);
        var deserialized = JsonSerializer.Deserialize<TestClass>(json, _options);

        Assert.IsNotNull(deserialized);
        Assert.AreEqual(testObj.Id, deserialized.Id);
        CollectionAssert.AreEqual(testObj.Data, deserialized.Data);
    }

    private class TestClass
    {
        public int Id
        {
            get; set;
        }

        public byte[]? Data
        {
            get; set;
        }
    }
}