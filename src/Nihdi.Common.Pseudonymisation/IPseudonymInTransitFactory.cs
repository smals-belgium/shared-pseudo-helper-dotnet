// <copyright file="IPseudonymInTransitFactory.cs" company="Riziv-Inami">
// Copyright (c) Riziv-Inami. All rights reserved.
// </copyright>

namespace Nihdi.Common.Pseudonymisation;

using System.Collections.ObjectModel;

// tag::interface[]

/// <summary>
/// Defines a factory for creating pseudonym-in-transit objects with various initialization methods.
/// </summary>
/// <remarks>
/// This factory provides methods to create individual pseudonyms from different data formats
/// as well as collections of multiple pseudonyms.
/// </remarks>
public interface IPseudonymInTransitFactory
{
    /// <summary>
    /// Creates a pseudonym-in-transit object from X and Y coordinates and transit information.
    /// </summary>
    /// <param name="x">The Base64 string representation of the X coordinate of the pseudonym.</param>
    /// <param name="y">The Base64 string representation of the Y coordinate of the pseudonym.</param>
    /// <param name="transitInfo">The standard JWE compact representation (Base64 URL encoded String) of the transitInfo.
    /// which contains the scalar that will be used to unblind the given pseudonym.
    /// </param>
    /// <returns>A new <see cref="IPseudonymInTransit"/> instance created from the given coordinates
    /// and transit info.
    /// </returns>
    IPseudonymInTransit FromXYAndTransitInfo(string x, string y, string transitInfo);

    /// <summary>
    /// Creates a <see cref="IPseudonymInTransit"/>  from the given SEC 1 representation
    /// of the elliptic curve point and transit info.
    /// </summary>
    /// <param name="sec1AndTransitInfo">Base64 URL string representation (without padding)
    ///  of the SEC 1 encoded point (can be SEC 1 compressed or uncompressed format),
    /// followed by <c>:</c>, and by the standard JWE compact representation
    /// (Base64 URL encoded String) of the transitInfo which contains the scalar
    ///  will be used to unblind the given point coordinates (pseudonym).
    /// </param>
    /// <returns>
    /// A <see cref="IPseudonymInTransit"/> instance created from the given <c>sec1AndTransitInfo</c>.
    /// </returns>
    IPseudonymInTransit FromSec1AndTransitInfo(string sec1AndTransitInfo);

    /// <summary>
    /// Creates an empty collection for multiple pseudonyms in transit.
    /// </summary>
    /// <returns>A new <see cref="IMultiplePseudonymInTransit"/> collection instance.</returns>
    IMultiplePseudonymInTransit Multiple();

    /// <summary>
    /// Creates a <see cref="IMultiplePseudonymInTransit"/> containing the items
    /// given in the provided <see cref="Collection{IPseudonymInTransit}"/>.
    /// The items (references) of the given collection are copied to the new instance.
    /// Changes done to the given collection after this call will not be reflected
    /// in the created instance.
    /// </summary>
    /// <param name="pseudonymsInTransit"><see cref="Collection{IPseudonymInTransit}"/>
    /// of pseudonyms to copy to the returned <see cref="IMultiplePseudonymInTransit"/>.</param>
    /// <returns>A new <see cref="IMultiplePseudonymInTransit"/> collection instance containing the provided pseudonyms.</returns>
    IMultiplePseudonymInTransit Multiple(Collection<IPseudonymInTransit> pseudonymsInTransit);
}

// end::interface[]