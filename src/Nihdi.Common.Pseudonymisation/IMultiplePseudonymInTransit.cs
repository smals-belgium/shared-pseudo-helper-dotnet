// <copyright file="IMultiplePseudonymInTransit.cs" company="Riziv-Inami">
// Copyright (c) Riziv-Inami. All rights reserved.
// </copyright>

namespace Nihdi.Common.Pseudonymisation;

/// <summary>
/// Collection of <see cref="IPseudonym"/>s, all belonging to the same ""
/// <see cref="IDomain"/>. <br/>
/// You cannot put more than 10 <see cref="IPseudonym"/>s in this collection.
/// </summary>
public interface IMultiplePseudonymInTransit : IMultiplePoint<IPseudonymInTransit>
{
    /// <summary>
    /// Convert all the <see cref="IPseudonym"/>s of this collection into
    /// <see cref="IPseudonymInTransit"/> of the given domain. <br/>
    /// Please not that the <see cref="IPseudonymInTransit"/> is linked to the
    /// first <see cref="IPseudonym"/> you gave, the second one to the second
    /// one,...
    /// </summary>
    /// <param name="toDomain">
    /// The key of the target <see cref="IDomain"/> for the returned
    /// <see cref="IPseudonymInTransit"/>s.
    /// </param>
    /// <returns>
    /// A <see cref="Task{IMultiplePseudonymInTransit}"/> that contains the
    /// converted <see cref="IPseudonym"/>s for the given
    /// <see cref="IDomain"/>.
    /// </returns>
    Task<IMultiplePseudonymInTransit>? ConvertTo(IDomain toDomain);

    /// <summary>
    /// Identify all <see cref="IPseudonymInTransit"/>s in this collection.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.
    /// The task result contains the identified <see cref="IPseudonymInTransit"/>s.
    /// </returns>
    Task<IMultipleValue> Identify();
}
