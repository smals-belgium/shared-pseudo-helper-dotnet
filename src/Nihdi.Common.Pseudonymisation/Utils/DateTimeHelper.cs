// <copyright file="DateTimeHelper.cs" company="Riziv-Inami">
// Copyright (c) Riziv-Inami. All rights reserved.
// </copyright>

namespace Nihdi.Common.Pseudonymisation.Utils;

/// <summary>
/// Provides helper methods for working with DateTime and epoch time.
/// </summary>
public static class DateTimeHelper
{
    /// <summary>
    /// Converts epoch seconds to a DateTimeOffset.
    /// </summary>
    /// <param name="epochSeconds">The epoch seconds to convert.</param>
    /// <returns>The corresponding DateTimeOffset.</returns>
    public static DateTimeOffset FromEpochSecondsToDateTimeOffset(long epochSeconds)
    {
        return DateTimeOffset.FromUnixTimeSeconds(epochSeconds);
    }

    /// <summary>
    /// Converts epoch seconds to a DateTime in UTC.
    /// </summary>
    /// <param name="epochSeconds">The epoch seconds to convert.</param>
    /// <returns>The corresponding DateTime in UTC.</returns>
    public static DateTime FromEpochSecondsToDateTime(long epochSeconds)
    {
        return DateTimeOffset.FromUnixTimeSeconds(epochSeconds).UtcDateTime;
    }
}
