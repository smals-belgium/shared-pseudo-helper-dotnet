// <copyright file="JweUtils.cs" company="Riziv-Inami">
// Copyright (c) Riziv-Inami. All rights reserved.
// </copyright>

namespace Nihdi.Common.Pseudonymisation.Jose;

using System.Text;
using System.Text.Json;
using Microsoft.IdentityModel.Tokens;
using Nihdi.Common.Pseudonymisation.Utils;

/// <summary>
/// Utility class for converting between JWE compact and flattened JSON formats.
/// </summary>
public static class JweUtils
{
    /// <summary>
    /// Converts a JWE compact string to its flattened JSON representation.
    /// </summary>
    /// <param name="jweCompact">The JWE compact string.</param>
    /// <returns>The flattened JSON representation of the JWE.</returns>
    public static JsonElement ToJsonFlattened(string? jweCompact)
    {
        if (jweCompact == null)
        {
            throw new ArgumentNullException(nameof(jweCompact), "JWE compact string cannot be null.");
        }

        var parts = jweCompact.Split('.');

        if (parts.Length != 5)
        {
            throw new ArgumentException("Invalid JWE compact format. Expected 5 parts separated by '.'");
        }

        // The parts are as follows:
        string protectedHeaderBase64 = parts[0]; // Base64URL-encoded protected header
        string encryptedKey = parts[1];    // Base64URL-encoded encrypted key (empty for "dir" algorithm)
        string iv = parts[2];              // Base64URL-encoded initialization vector
        string ciphertext = parts[3];      // Base64URL-encoded ciphertext
        string tag = parts[4];             // Base64URL-encoded authentication tag

        JsonElement protectedHeader;

        try
        {
            byte[] bytes = Base64UrlEncoder.DecodeBytes(protectedHeaderBase64);
            string protectedHeaderJson = Encoding.UTF8.GetString(bytes);

            using JsonDocument headerDoc = JsonDocument.Parse(protectedHeaderJson);
            protectedHeader = headerDoc.RootElement.Clone();
        }
        catch (Exception)
        {
            throw new ArgumentException("Invalid Base64 URL encoding in protected header.");
        }

        var jweFlattenedJson = new Dictionary<string, object>
        {
        { "protected", protectedHeaderBase64 },      // Base64URL-encoded protected header
        { "header", protectedHeader },              // Add the decoded protected header in JSON format
        { "encrypted_key", encryptedKey },     // Base64URL-encoded encrypted key (can be empty for dir)
        { "iv", iv },                         // Base64URL-encoded IV
        { "ciphertext", ciphertext },          // Base64URL-encoded ciphertext
        { "tag", tag },                  // Base64URL-encoded tag (authentication tag)
        { "token", jweCompact },
        };

        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
        };

        string jsonString = JsonSerializer.Serialize(jweFlattenedJson, jsonOptions);
        using JsonDocument doc = JsonDocument.Parse(jsonString);

        return doc.RootElement.Clone();
    }

    /// <summary>
    /// Converts a JWE flattened JSON representation to its compact string format.
    /// </summary>
    /// <param name="jweFlattenedJson">The JWE flattened JSON representation.</param>
    /// <returns>The compact string format of the JWE.</returns>
    public static string? ToJweCompact(JsonElement jweFlattenedJson)
    {
        if (jweFlattenedJson.ValueKind != JsonValueKind.Object)
        {
            throw new ArgumentException("Expected a JSON object for JWE flattened JSON.");
        }

        // Extract required parts from JsonElement
        string? protectedHeaderBase64 = jweFlattenedJson.GetProperty("protected").GetString();
        string? encryptedKey = jweFlattenedJson.TryGetProperty("encrypted_key", out var ekProp) ? ekProp.GetString() : string.Empty;
        string? iv = jweFlattenedJson.GetProperty("iv").GetString();
        string? ciphertext = jweFlattenedJson.GetProperty("ciphertext").GetString();
        string? tag = jweFlattenedJson.GetProperty("tag").GetString();

        if (string.IsNullOrEmpty(protectedHeaderBase64))
        {
            throw new ArgumentException("Missing 'protected' header.");
        }

        if (string.IsNullOrEmpty(iv))
        {
            throw new ArgumentException("Missing 'iv' (initialization vector).");
        }

        if (string.IsNullOrEmpty(ciphertext))
        {
            throw new ArgumentException("Missing 'ciphertext'.");
        }

        if (string.IsNullOrEmpty(tag))
        {
            throw new ArgumentException("Missing 'tag' (authentication tag).");
        }

        // Check if all required elements are present
        // For algorithms like 'dir', the 'encrypted_key' field may be empty
        encryptedKey = encryptedKey ?? string.Empty;

        // Construct the JWE compact format by concatenating the parts with dots
        var jweCompact = $"{protectedHeaderBase64}.{encryptedKey}.{iv}.{ciphertext}.{tag}";

        return jweCompact;
    }
}
