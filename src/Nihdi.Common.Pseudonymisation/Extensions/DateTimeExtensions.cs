// <copyright file="DateTimeExtensions.cs" company="Riziv-Inami">
// Copyright (c) Riziv-Inami. All rights reserved.
// </copyright>

namespace Nihdi.Common.Pseudonymisation.Extensions;

/// <summary>
/// Extension methods for the <see cref="DateTime"/> class.
/// </summary>
public static class DateTimeExtensions
{
    /// <summary>
    /// Converts the given <see cref="DateTime"/> to Unix time seconds.
    /// </summary>
    /// <param name="dateTime">The DateTime to convert.</param>
    /// <returns>The number of seconds elapsed since 1970-01-01T00:00:00Z.</returns>
    public static long ToUnixTimeSeconds(this DateTime dateTime)
    {
        DateTime utcDateTime = dateTime.ToUniversalTime();

        return ((DateTimeOffset)utcDateTime).ToUnixTimeSeconds();
    }

    /// <summary>
    /// Determines whether the first DateTime is chronologically after the second DateTime.
    /// </summary>
    /// <param name="dateTime1">The first DateTime to compare.</param>
    /// <param name="dateTime2">The second DateTime to compare against.</param>
    /// <returns>true if the first DateTime is later than the second DateTime; otherwise, false.</returns>
    public static bool IsAfter(this DateTime dateTime1, DateTime dateTime2)
    {
        return dateTime1 > dateTime2;
    }

    /// <summary>
    /// Determines whether the first DateTime is chronologically before the second DateTime.
    /// </summary>
    /// <param name="dateTime1">The first DateTime to compare.</param>
    /// <param name="dateTime2">The second DateTime to compare against.</param>
    /// <returns>true if the first DateTime is earlier than the second DateTime; otherwise, false.</returns>
    public static bool IsBefore(this DateTime dateTime1, DateTime dateTime2)
    {
        return dateTime1 < dateTime2;
    }
}