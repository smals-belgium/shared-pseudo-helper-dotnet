// <copyright file="SecretKeysFromEHealth.cs" company="Riziv-Inami">
// Copyright (c) Riziv-Inami. All rights reserved.
// </copyright>

namespace Nihdi.Common.Pseudonymisation;

using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
/// Represents a collection of secret keys retrieved from the eHealth platform.
/// </summary>
public class SecretKeysFromEHealth
{
    /// <summary>
    /// Gets or sets the list of secret keys.
    /// </summary>
    /// <value>A list of <see cref="SecretKeyFromEHealth"/> objects.</value>
    [JsonPropertyName("secretKeys")]
    public List<SecretKeyFromEHealth>? SecretKeys
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets any additional data not explicitly modeled by other properties.
    /// </summary>
    /// <value>A dictionary containing additional data.</value>
    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalData
    {
        get; set;
    }
}