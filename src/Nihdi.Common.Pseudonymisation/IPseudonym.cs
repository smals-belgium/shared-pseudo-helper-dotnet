// <copyright file="IPseudonym.cs" company="Riziv-Inami">
// Copyright (c) Riziv-Inami. All rights reserved.
// </copyright>

namespace Nihdi.Common.Pseudonymisation;

 // tag::interface[]

/// <summary>
/// Wrapper around an elliptic curve point that provides useful methods to manipulate eHealth pseudonyms.
/// </summary>
public interface IPseudonym : IPoint
{
    /// <summary>
    ///  Base64 URL encoded uncompressed SEC1 Elliptic-Curve-Point-to-Octet-String Conversion of this point.
    /// </summary>
    /// <returns>@return Base64 URL encoded the uncompressed SEC1 Elliptic-Curve-Point-to-Octet-String Conversion of this point.</returns>
    string AsString();

    /// <summary>
    /// Compressed SEC 1 representation of this point.
    /// </summary>
    /// <returns>SEC 1 representation of this point.</returns>
    string AsShortString();

    /// <summary>
    /// Convert this <see cref="IPseudonym"/> into a <see cref="IPseudonymInTransit"/>.
    /// </summary>
    /// <param name="toDomain">The target domain for the returned <see cref="IPseudonymInTransit"/>.</param>
    /// <returns>
    /// A <see cref="IPseudonymInTransit"/> for the given domain, matching this <see cref="IPseudonym"/>.
    /// </returns>
    Task<IPseudonymInTransit> ConvertTo(IDomain toDomain);

    /// <summary>
    /// Create <see cref="IPseudonymInTransit"/> from this <see cref="IPseudonym"/>
    /// Use this method to convert a pseudonym at rest into a <see cref="IPseudonymInTransit"/> that you can send externally.
    /// The scalar in transitInfo is encoded in Base64.
    /// </summary>
    /// <returns>A <see cref="IPseudonymInTransit"/> with X and Y blinded by a scalar (which is encrupted an put in transitInfo).</returns>
    IPseudonymInTransit InTransit();

    /// <summary>
    /// Create <see cref="IPseudonymInTransit"/> from this <see cref="IPseudonym"/>
    /// Use this method to convert a pseudonym at rest into a <see cref="IPseudonymInTransit"/> that you can send externally.
    /// The scalar in transitInfo is encoded in Base64.
    /// </summary>
    /// <param name="transitInfoCustomizer">Customizer to customize the transit info (for example to add a target domain).</param>
    /// <returns>
    /// A <see cref="IPseudonymInTransit"/> with X and Y blinded by a scalar (which is encrupted an put in transitInfo).
    /// </returns>
    IPseudonymInTransit InTransit(ITransitInfoCustomizer transitInfoCustomizer);
}

// end::interface[]
