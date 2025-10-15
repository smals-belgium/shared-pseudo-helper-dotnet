// <copyright file="IPseudonymInTransit.cs" company="Riziv-Inami">
// Copyright (c) Riziv-Inami. All rights reserved.
// </copyright>

namespace Nihdi.Common.Pseudonymisation;

using Nihdi.Common.Pseudonymisation.Internal;

// tag:interface[]

/// <summary>
/// A pseudonym in transit is a pseudonym that is encrypted for transit between two domains.
/// </summary>
public interface IPseudonymInTransit : IPoint
{
    /// <summary>
    ///  Returns the <see cref="IPseudonym"/> of this <see cref="IPseudonymInTransit"/>.
    /// </summary>
    /// <returns>
    /// The <see cref="IPseudonym"/> of this <see cref="IPseudonymInTransit"/>.
    /// </returns>
    IPseudonym Pseudonym();

    /// <summary>
    /// Gets the <see cref="ITransitInfo"/> of this <see cref="IPseudonymInTransit"/>.
    /// </summary>
    /// <returns>The <see cref="ITransitInfo"/>.</returns>
    ITransitInfo GetTransitInfo();

    /// <summary>
    /// Returns the standard string representation of this <see cref="IPseudonymInTransit"/>.
    /// It returns the Base64 URL representation of the uncompressed SEC 1 representation of the point
    /// followed by `:` and by the string representation of the <see cref="ITransitInfo" /> (JWE compact).
    /// Only use this method instead of <see cref="AsShortString"/>  when the length of the string is not very important,
    /// because it spares the recipient of this <see cref="IPseudonymInTransit"/> to compute the point's Y coordinate.
    /// </summary>.
    /// <returns>The standard String representation of this <see cref="IPseudonymInTransit"/>.</returns>
    string AsString();

    /// <summary>
    /// Returns the short string representation of this <see cref="IPseudonymInTransit"/>.
    /// It returns the Base64 URL representation of the compressed SEC 1 representation of the point
    /// followed by `:` and by the string representation of the <see cref="ITransitInfo"/> (JWE compact).
    /// Only use this method instead of <see cref="AsString"/>  when you need to shorten the string (to prevent a too long URL, for example).
    /// The drawback is that the recipient of this <see cref="IPseudonymInTransit"/> will have to compute the Y coordinate of the point.
    /// </summary>.
    /// <returns>The standard String representation of this <see cref="IPseudonymInTransit"/>.</returns>
    string AsShortString();

    /// <summary>
    /// Identify (de-pseudonymise) this <see cref="IPseudonymInTransit"/>.
    /// </summary>
    /// <returns>The identified <see cref="IPseudonym"/> as <see cref="Task{IValue}"/>.</returns>
    Task<IValue> Identify();

    /// <summary>
    /// Decrypt the pseudonym in transit.
    /// <c>iat</c> and <c>exp</c> must be valid: it calls <see cref="AtRest(bool)"/> with <c>true</c>.
    /// </summary>
    /// <returns>The pseudonym at rest.</returns>
    IPseudonym? AtRest();

    /// <summary>
    /// Decrypt the pseudonym in transit.
    /// In regular case, you should not use this method: you should use
    /// <see cref="AtRest()"/>  instead.
    /// Only use this method if you need to recover an expired
    ///  <see cref="IPseudonymInTransit"/> , for example.
    /// </summary>
    /// <param name="validateIatAndExp">Must iat and exp be validated ?.</param>
    /// <returns>The pseudonym at rest.</returns>
    IPseudonym? AtRest(bool validateIatAndExp);

    /// <summary>
    /// Convert this <see cref="IPseudonymInTransit"/> into a <see cref="IPseudonymInTransit"/> for the given domain.
    /// <paramref name="toDomain" /> the target domain for the returned <see cref="IPseudonymInTransit"/>.
    /// </summary>
    /// <param name="toDomain">The target domain for the returned <see cref="IPseudonymInTransit"/>.</param>
    /// <returns>A <see cref="IPseudonymInTransit"/> for the given domain,
    /// matching this <see cref="IPseudonymInTransit"/>.
    /// </returns>
    Task<IPseudonymInTransit> ConvertTo(IDomain toDomain);
}

// end::interface[]