// <copyright file="IMultiplePseudonym.cs.cs" company="Riziv-Inami">
// Copyright (c) Riziv-Inami. All rights reserved.
// </copyright>

namespace Nihdi.Common.Pseudonymisation;

using Nihdi.Common.Pseudonymisation.Internal;

/// <summary>
/// Collection of  <see cref="IPseudonym"/>s, all belonging to the same
/// <see cref="IDomain"/>.
/// <br/>
/// You cannot put more than 10 <see cref="IPseudonym"/>s in this collection.
/// </summary>
public interface IMultiplePseudonym : IMultiplePoint<IPseudonym>
{
    /// <summary>
    /// Convert all <see cref="IPseudonym"/>s in this collection to
    /// <see cref="MultiplePseudonymInTransit"/> in the target domain.
    /// </summary>
    /// <param name="targetDomain">The target <see cref="IDomain"/> for the conversion.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    Task<MultiplePseudonymInTransit> ConvertTo(IDomain targetDomain);
}
