// <copyright file="PointFactory.cs" company="Riziv-Inami">
// Copyright (c) Riziv-Inami. All rights reserved.
// </copyright>

namespace Nihdi.Common.Pseudonymisation.Internal;

using Nihdi.Common.Pseudonymisation;

/// <summary>
/// Abstract base class for factories that create points within a specific domain.
/// </summary>
/// <remarks>
/// The PointFactory serves as a foundation for specialized point creation mechanisms.
/// Derived classes must implement the specific point creation logic.
/// </remarks>
public abstract class PointFactory
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PointFactory"/> class.
    /// </summary>
    /// <param name="domain">The domain to be used by the factory.</param>
    protected PointFactory(IDomain domain)
    {
        Domain = (Domain)domain;
    }

    /// <summary>
    /// Gets the domain to which this factory belongs.
    /// </summary>
    /// <value>
    /// The domain to which this factory belongs.
    /// </value>
    protected Domain Domain
    {
        get;
    }
}