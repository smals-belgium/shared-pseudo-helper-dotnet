// <copyright file="MultiplePoint.cs" company="Riziv-Inami">
// Copyright (c) Riziv-Inami. All rights reserved.
// </copyright>

namespace Nihdi.Common.Pseudonymisation.Internal;

using Nihdi.Common.Pseudonymisation.Exceptions;

/// <summary>
/// Represents a collection of cryptographic points used in pseudonymisation operations.
/// </summary>
/// <typeparam name="TPoint">A type that implements <see cref="IPoint"/>.</typeparam>
public abstract class MultiplePoint<TPoint> : IMultiplePoint<TPoint>
    where TPoint : IPoint
{
    private const int _maxCollectionSize = 10;

    /// <summary>
    /// Initializes a new instance of the <see cref="MultiplePoint{TPoint}"/> class.
    /// Gets the list of points.
    /// </summary>
    /// <param name="domain">The domain to which this collection belongs.</param>
    protected MultiplePoint(IDomain domain)
    {
        this.Domain = (Domain)domain;
        Points = new List<object>(_maxCollectionSize);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MultiplePoint{TPoint}"/> class.
    /// </summary>
    /// <param name="domain">The domain to which this collection belongs.</param>
    /// <param name="points">The collection of points to be included in this multiple point.</param>
    protected MultiplePoint(IDomain domain, ICollection<object> points)
    {
        if (points.Count > _maxCollectionSize)
        {
            throw new ArgumentOutOfRangeException(
                nameof(MultiplePoint<TPoint>.Points),
                $"Collection size cannot exceed {_maxCollectionSize}");
        }

        this.Domain = (Domain)domain;
        this.Points = new List<object>(points);
    }

    /// <inheritdoc/>
    IDomain IMultiplePoint<TPoint>.Domain => Domain;

    /// <summary>
    /// Gets the domain to which this collection belongs.
    /// </summary>
    /// <value>
    /// The <see cref="Domain"/> instance.
    /// </value>
    protected Domain Domain
    {
        get;
        private set;
    }

    /// <summary>
    /// Gets the list of points in this collection.
    /// </summary>
    /// <value>
    /// The list of points in this collection.
    /// </value>
    protected List<object> Points
    {
        get;
    }

    /// <inheritdoc/>
    public TPoint this[int index]
    {
        get
        {
            var pointOfProblem = Points[index];

            if (pointOfProblem is IEHealthProblem)
            {
                throw new EHealthProblemException((EHealthProblem)pointOfProblem);
            }

            return (TPoint)pointOfProblem;
        }
    }

    /// <summary>
    /// Checks if given size is a valid size.
    /// </summary>
    /// <param name="size">The size to validate.</param>
    public abstract void CheckCollectionSize(int size);

    /// <summary>
    /// Validate if the given point can be added.
    /// </summary>
    /// <param name="point">The <see cref="IPoint"/>to validate.</param>
    /// <returns>The given <see cref="IPoint"/>.</returns>
    public abstract TPoint Validate(TPoint point);

    /// <inheritdoc/>
    public bool Add(TPoint point)
    {
        CheckCollectionSize(Points.Count + 1);
        Points.Add(Validate(point));
        return true;
    }

    /// <summary>
    /// Add a problem to the collection.
    /// </summary>
    /// <param name="problem">The problem to add.</param>
    public void Add(IEHealthProblem problem)
    {
        CheckCollectionSize(Points.Count + 1);
        Points.Add(problem);
    }

    /// <inheritdoc/>
    public int Size()
    {
        return Points.Count;
    }
}
