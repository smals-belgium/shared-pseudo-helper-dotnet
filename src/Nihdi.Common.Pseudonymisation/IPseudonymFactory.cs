// <copyright file="IPseudonymFactory.cs" company="Riziv-Inami">
// Copyright (c) Riziv-Inami. All rights reserved.
// </copyright>

namespace Nihdi.Common.Pseudonymisation;

// tag::interface[]

/// <summary>
/// Factory interface for creating <see cref="IPseudonym"/> instances.
/// </summary>
public interface IPseudonymFactory
{
    /// <summary>
    /// Creates a pseudonym from the given X coordinate in Base64 string format.
    /// The Y coordinate will be computed and one of the two possible values will be randomly chosen.
    /// The Y coordinate can be randomly chosen because only X is important in the context of
    /// eHealth pseudonymisation.
    /// </summary>
    /// <param name="xAsBase64String">Base64 string representation of the X coordinate.</param>
    /// <returns>A <see cref="IPseudonym"/> instance created from the given X coordinate.</returns>
    IPseudonym FromX(string xAsBase64String);

    /// <summary>
    /// Creates a <see cref="IPseudonym"/> from the given X and Y coordinates in Base64 string format.
    /// </summary>
    /// <param name="xAsBase64String">Base64 string representation of the X coordinate.</param>
    /// <param name="yAsBase64String">Base64 string representation of the Y coordinate.</param>
    /// <returns>A <see cref="IPseudonym"/> instance created from the given X and Y coordinates.</returns>
    IPseudonym FromXy(string xAsBase64String, string yAsBase64String);

    /// <summary>
    /// Creates an empty collection for multiple pseudonyms.
    /// </summary>
    /// <returns>An empty <see cref="IMultiplePseudonym"/> instance.</returns>
    IMultiplePseudonym Multiple();

    /// <summary>
    /// Creates a <see cref="IMultiplePseudonym"/> containing the items of the given
    /// <see cref="IList{IPseudonym}"/> collection.
    /// The items (references) of the given collection are copied to the new instance.
    /// Changes done to the given collection after this call will not be reflected
    /// in the created instance.
    /// </summary>
    /// <param name="pseudonyms"><see cref="IList{IPseudonym}"/> collection of pseudonyms.</param>
    /// <returns>A <see cref="IMultiplePseudonym"/> instance containing the given pseudonyms.</returns>
    IMultiplePseudonym Multiple(IList<IPseudonym> pseudonyms);
}

// end::interface[]
