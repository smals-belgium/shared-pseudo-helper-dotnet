// <copyright file="JweDecryptedPayload.cs" company="Riziv-Inami">
// Copyright (c) Riziv-Inami. All rights reserved.
// </copyright>

namespace Nihdi.Common.Pseudonymisation.Jose;

/// <summary>
/// Represents the decrypted payload of a JWE (JSON Web Encryption) object, including the protected
/// header and the plaintext payload.
/// </summary>
public class JweDecryptedPayload
{
    /// <summary>
    /// Gets or sets the protected header as a JSON string.
    /// </summary>
    /// <value>
    /// The protected header as a JSON string.
    /// </value>
    public string ProtectedHeader { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the decrypted payload as a string.
    /// </summary>
    /// <value>The decrypted payload as a string.</value>
    public string Payload { get; set; } = string.Empty;
}