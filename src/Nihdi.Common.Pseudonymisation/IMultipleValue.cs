// <copyright file="IMultipleValue.cs" company="Riziv-Inami">
// Copyright (c) Riziv-Inami. All rights reserved.
// </copyright>

namespace Nihdi.Common.Pseudonymisation;

using Nihdi.Common.Pseudonymisation.Internal;

/// <summary>
/// Collection of <see cref="Value"/>s, all belonging to the same
/// <see cref="IDomain"/>. <br/>
/// You cannot put more than 10 items in this collection.
/// </summary>
public interface IMultipleValue : IMultiplePoint<IValue>
{
    /// <summary>
    /// Pseudonymize all the <see cref="Value"/> in this collection into
    /// <see cref="IPseudonymInTransit"/>s of the same domain.
    /// Please note that the <see cref="IPseudonymInTransit"/>s, will be
    /// returned in the same order eHealth return it (it means that the
    /// first <see cref="IPseudonymInTransit"/> is linked to the first
    /// <see cref="Value"/> you gave, the second one to the secone done,...).
    /// </summary>
    /// <returns>A <see cref="Task{IMultiplePseudonymInTransit}"/> that
    /// contains the pseudonymized values.
    /// </returns>
    Task<IMultiplePseudonymInTransit> Pseudonymize();
}