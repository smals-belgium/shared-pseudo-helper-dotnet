// <copyright file="SecretKeyFromEHealth.cs" company="Riziv-Inami">
// Copyright (c) Riziv-Inami. All rights reserved.
// </copyright>

namespace Nihdi.Common.Pseudonymisation;

using System.Text.Json.Serialization;
using Nihdi.Common.Pseudonymisation.Jose;

/// <summary>
/// Represents a secret key derived from eHealth data.
/// </summary>
public class SecretKeyFromEHealth
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SecretKeyFromEHealth"/> class.
    /// </summary>
    /// <param name="kid">The key ID.</param>
    /// <param name="active">Indicates whether the key is active.</param>
    /// <param name="jwe">The JWE token.</param>
    [JsonConstructor]
    public SecretKeyFromEHealth(string kid, bool active, JweToken jwe)
    {
        Kid = kid ?? throw new ArgumentNullException(nameof(kid));
        Active = active;
        Jwe = jwe ?? throw new ArgumentNullException(nameof(jwe));
    }

    /// <summary>
    /// Gets or sets the key ID.
    /// </summary>
    /// <value>The key ID.</value>
    [JsonPropertyName("kid")]
    public string Kid
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets a value indicating whether the key is active.
    /// </summary>
    /// <value>Indicates whether the key is active.</value>
    [JsonPropertyName("active")]
    public bool Active
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the JWE token.
    /// </summary>
    /// <value>The JWE token.</value>
    [JsonPropertyName("encoded")]
    public JweToken Jwe
    {
        get; set;
    }
}