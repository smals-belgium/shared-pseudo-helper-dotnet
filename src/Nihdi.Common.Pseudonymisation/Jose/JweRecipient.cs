// <copyright file="JweRecipient.cs" company="Riziv-Inami">
// Copyright (c) Riziv-Inami. All rights reserved.
// </copyright>

namespace Nihdi.Common.Pseudonymisation.Jose;

using System.Text.Json.Serialization;

/// <summary>
/// Represents a recipient in a JWE (JSON Web Encryption) object.
/// </summary>
public class JweRecipient
{
    /// <summary>
    /// Gets or sets the encrypted key for the recipient, encoded in Base64Url.
    /// </summary>
    /// <value>The encrypted key as a byte array.</value>
    [JsonPropertyName("encrypted_key")]
    [JsonConverter(typeof(ByteArrayBase64Converter))]
    public byte[]? EncryptedKey
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the recipient-specific header parameters.
    /// </summary>
    /// <value>The recipient-specific header parameters.</value>
    [JsonPropertyName("header")]
    public JweRecipientHeader Header { get; set; } = new JweRecipientHeader();
}