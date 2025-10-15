// <copyright file="IMultiplePoint.cs" company="Riziv-Inami">
// Copyright (c) Riziv-Inami. All rights reserved.
// </copyright>

namespace Nihdi.Common.Pseudonymisation;

/// <summary>
/// Represents a collection of cryptographic points used in pseudonymisation operations.
/// </summary>
/// <remarks>
/// The collection has a limited capacity and only stores points of the specified type.
/// </remarks>
/// <typeparam name="TPoint">The type of points stored in the collection, must implement <see cref="IPoint"/>.</typeparam>
public interface IMultiplePoint<TPoint>
    where TPoint : IPoint
{
    /// <summary>
    /// Gets the <see cref="IDomain"/> to which this collection belongs.
    /// </summary>
    /// <value>
    /// The <see cref="IDomain"/> to which this collection belongs.
    /// </value>
    IDomain Domain
    {
        get;
    }

    /// <summary>
    /// Returns the element at the given index.
    ///  If the eHealth response for the element at this index was a problem,
    ///   then a <see cref="Nihdi.Common.Pseudonymisation.Exceptions.EHealthProblemException"/> is thrown.
    /// </summary>
    /// <param name="index">The element index.</param>
    /// <returns>The element at the given index.</returns>
    TPoint this[int index] { get; }

    /// <summary>
    /// Returns the number of elements.
    /// <![CDATA[The size will always be >= 0 and <= 10.]]>
    /// </summary>
    /// <returns>The number of elements in this collection.</returns>
    int Size();

    /// <summary>
    /// Add an element.
    /// </summary>
    /// <param name="point">Element to add.</param>
    /// <returns><see langword="true"/> if the given item has been added.</returns>
    bool Add(TPoint point);
}