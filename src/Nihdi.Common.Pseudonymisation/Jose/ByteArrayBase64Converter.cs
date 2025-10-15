// <copyright file="ByteArrayBase64Converter.cs" company="Riziv-Inami">
// Copyright (c) Riziv-Inami. All rights reserved.
// </copyright>

namespace Nihdi.Common.Pseudonymisation.Jose;

using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.IdentityModel.Tokens;

/// <summary>
/// A JSON converter for byte arrays that encodes/decodes them using Base64Url.
/// </summary>
public class ByteArrayBase64Converter : JsonConverter<byte[]>
{
    /// <inheritdoc/>
    public override byte[]? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            string? base64UrlString = reader.GetString();

            if (string.IsNullOrEmpty(base64UrlString))
            {
                return null;
            }

            try
            {
                return Base64UrlEncoder.DecodeBytes(base64UrlString);
            }
            catch (FormatException)
            {
                throw new JsonException($"Invalid Base64Url string: {base64UrlString}");
            }
        }

        return null;
    }

    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, byte[] value, JsonSerializerOptions options)
    {
        if (value == null)
        {
            writer.WriteNullValue();
            return;
        }

        writer.WriteStringValue(Base64UrlEncoder.Encode(value));
    }
}