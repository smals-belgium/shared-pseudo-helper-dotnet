// <copyright file="JweRecipientHeader.cs" company="Riziv-Inami">
// Copyright (c) Riziv-Inami. All rights reserved.
// </copyright>

namespace Nihdi.Common.Pseudonymisation.Jose;

using System.Text.Json.Serialization;

/// <summary>
/// Represents a JSON Web Encryption (JWE) recipient header that contains parameters
/// specific to a recipient of the JWE encrypted content.
/// </summary>
/// <remarks>
/// JWE allows encrypted content to be prepared for multiple recipients, each using their own
/// key management algorithms and encryption keys.
/// </remarks>
public class JweRecipientHeader
{
    /// <summary>
    /// Gets or sets the JWK Set URL. Points to a resource for a set of JSON-encoded public keys,
    /// one of which corresponds to the key used to encrypt the JWE.
    /// </summary>
    /// <value>The JWK Set URL.</value>
    [JsonPropertyName("jku")]
    public string Jku { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the algorithm ("alg") parameter identifying the cryptographic algorithm
    /// used to encrypt or determine the value of the Content Encryption Key.
    /// </summary>
    /// <value>The algorithm used to encrypt or determine the CEK.</value>
    [JsonPropertyName("alg")]
    public string Alg { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the key ID ("kid") parameter that is a hint indicating which key was used
    /// to secure the JWE to the recipient.
    /// </summary>
    /// <value>The key ID.</value>
    [JsonPropertyName("kid")]
    public string Kid { get; set; } = string.Empty;

    /// <summary>
    /// Converts the JWE recipient header into a dictionary representation.
    /// </summary>
    /// <returns>
    /// A dictionary containing the non-empty properties of the JWE recipient header.
    /// Empty properties are excluded from the resulting dictionary.
    /// </returns>
    public Dictionary<string, object> ToDictionary()
    {
        var dict = new Dictionary<string, object>();

        if (!string.IsNullOrEmpty(Alg))
        {
            dict["alg"] = Alg;
        }

        if (!string.IsNullOrEmpty(Jku))
        {
            dict["jku"] = Jku;
        }

        if (!string.IsNullOrEmpty(Kid))
        {
            dict["kid"] = Kid;
        }

        return dict;
    }
}