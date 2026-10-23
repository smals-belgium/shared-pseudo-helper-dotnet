// <copyright file="IMultiplePseudonym.cs" company="Riziv-Inami">
// Copyright (c) Riziv-Inami. All rights reserved.
// </copyright>

namespace Nihdi.Common.Pseudonymisation;

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
    /// <see cref="IMultiplePseudonymInTransit"/> in the target domain.
    /// </summary>
    /// <param name="targetDomain">The target <see cref="IDomain"/> for the conversion.</param>
    /// <param name="cancellationToken">Cancels the call: the pending request is told to stop.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    Task<IMultiplePseudonymInTransit> ConvertTo(IDomain targetDomain, CancellationToken cancellationToken = default);
}
