// <copyright file="IPoint.cs" company="Riziv-Inami">
// Copyright (c) Riziv-Inami. All rights reserved.
// </copyright>

namespace Nihdi.Common.Pseudonymisation;

/// <summary>
/// Interface representing a point on an elliptic curve.
/// </summary>
/// <remarks>
/// Two points are equal when they belong to the same domain and have the same X coordinate.
/// </remarks>
public interface IPoint
{
    /// <summary>
    /// Gets get the domain that owns this point.
    /// </summary>
    /// <value>The domain that owns this point.</value>
    IDomain Domain
    {
        get;
    }

    /// <summary>
    /// Returns binary representation of the X coordinate (as a byte array converted in a Base64 String).
    /// </summary>
    /// <returns>binary representation of the X coordinate (as a byte array converted in a Base64 String).</returns>
    string X();

    /// <summary>
    /// Returns binary representation of the Y coordinate (as a byte array converted in a Base64 String).
    /// </summary>
    /// <returns>binary representation of the Y coordinate (as a byte array converted in a Base64 String).</returns>
    string Y();
}