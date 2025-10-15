// <copyright file="JweContentEncryptionAlgorithm.cs" company="Riziv-Inami">
// Copyright (c) Riziv-Inami. All rights reserved.
// </copyright>

namespace Nihdi.Common.Pseudonymisation.Jose;

/// <summary>
/// Specifies the algorithms available for content encryption in JSON Web Encryption (JWE).
/// </summary>
public enum JweContentEncryptionAlgorithm
{
    /// <summary>
    /// Authenticated Encryption using AES in GCM mode with a 128-bit key.
    /// </summary>
    A128GCM,

    /// <summary>
    /// Authenticated Encryption using AES in GCM mode with a 192-bit key.
    /// </summary>
    A192GCM,

    /// <summary>
    /// Authenticated Encryption using AES in GCM mode with a 256-bit key.
    /// </summary>
    A256GCM,

    /// <summary>
    /// Authenticated Encryption using AES in CBC mode combined with HMAC-SHA256.
    /// </summary>
    A128CBC_HS256,

    /// <summary>
    /// Authenticated Encryption using AES in CBC mode combined with HMAC-SHA384.
    /// </summary>
    A192CBC_HS384,

    /// <summary>
    /// Authenticated Encryption using AES in CBC mode combined with HMAC-SHA512.
    /// </summary>
    A256CBC_HS512,
}