// <copyright file="IDateTimeService.cs" company="Riziv-Inami">
// Copyright (c) Riziv-Inami. All rights reserved.
// </copyright>

namespace Nihdi.Common.Pseudonymisation;

/// <summary>
/// Service to get the current date and time.
/// </summary>
public interface IDateTimeService
{
    /// <summary>
    /// Gets the current date and time.
    /// </summary>
    /// <value>The current date and time.</value>
    DateTime Now
    {
        get;
    }

    /// <summary>
    /// Gets the current date and time in UTC.
    /// </summary>
    /// <value>The current date and time in UTC.</value>
    DateTime UtcNow
    {
        get;
    }
}