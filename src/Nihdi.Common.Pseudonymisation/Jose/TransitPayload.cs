// <copyright file="TransitPayload.cs" company="Riziv-Inami">
// Copyright (c) Riziv-Inami. All rights reserved.
// </copyright>

namespace Nihdi.Common.Pseudonymisation.Jose;

using System.Text.Json.Serialization;

/// <summary>
/// Represents a payload for transit in JSON Web Token (JWT) operations.
/// </summary>
/// <remarks>
/// This class encapsulates JWT standard claims and additional custom properties
/// that can be included in a token payload.
/// </remarks>
public class TransitPayload
{
    /// <summary>
    /// Gets or sets the "Issued At" timestamp.
    /// </summary>
    /// <remarks>
    /// Represents the time at which the JWT was issued, as a Unix timestamp.
    /// </remarks>
    /// <value>The Unix timestamp representing the issuance time.</value>
    public long Iat
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the "Expiration Time" timestamp.
    /// </summary>
    /// <remarks>
    /// Identifies the expiration time after which the JWT must not be accepted for processing,
    /// as a Unix timestamp.
    /// </remarks>
    /// <value>The Unix timestamp representing the expiration time.</value>
    public long Exp
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets an optional scalar value in the payload.
    /// </summary>
    /// <value>The optional scalar value or null if not specified.</value>
    public string? Scalar
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets additional properties that can be included in the payload.
    /// </summary>
    /// <remarks>
    /// This collection captures any properties in the JWT payload that are not explicitly
    /// modeled by other properties in this class.
    /// </remarks>
    /// <value>A dictionary containing additional properties not explicitly defined in this class.</value>
    [JsonExtensionData]
    public Dictionary<string, object> AdditionalProperties { get; set; } = new Dictionary<string, object>();

    /// <summary>
    /// Converts the payload to a dictionary representation.
    /// </summary>
    /// <value>The dictionary representation of the payload.</value>
    /// <returns>A dictionary containing all properties of the payload, suitable for serialization.</returns>
    public Dictionary<string, object> ToDictionary()
    {
        var dictionary = new Dictionary<string, object>
        {
            { "iat", Iat },
            { "exp", Exp },
        };

        if (Scalar != null)
        {
            dictionary.Add("scalar", Scalar.ToString());
        }

        foreach (var kvp in AdditionalProperties)
        {
            dictionary[kvp.Key] = kvp.Value;
        }

        return dictionary;
    }
}
