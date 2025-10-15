// <copyright file="DateTimeService.cs" company="Riziv-Inami">
// Copyright (c) Riziv-Inami. All rights reserved.
// </copyright>

namespace Nihdi.Common.Pseudonymisation.Internal;

/// <summary>
/// Service to get the current date and time.
/// </summary>
public class DateTimeService : IDateTimeService
{
    /// <inheritdoc/>
    public DateTime Now
    {
        get
        {
            return DateTime.Now;
        }
    }

    /// <inheritdoc/>
    public DateTime UtcNow
    {
        get
        {
            return DateTime.UtcNow;
        }
    }
}