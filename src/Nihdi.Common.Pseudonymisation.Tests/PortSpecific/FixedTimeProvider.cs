// <copyright file="FixedTimeProvider.cs" company="Riziv-Inami">
// Copyright (c) Riziv-Inami. All rights reserved.
// </copyright>

namespace Nihdi.Common.Pseudonymisation.Tests.PortSpecific;

/// <summary>
/// A <see cref="TimeProvider"/> whose clock never moves, used to issue and read a transit info at a
/// chosen moment.
/// <para>
/// This replaces <c>FakeTimeProvider</c> of <c>Microsoft.Extensions.TimeProvider.Testing</c>: that
/// package is not available on the internal NuGet feed, and does not support net48.
/// </para>
/// </summary>
internal sealed class FixedTimeProvider : TimeProvider
{
    private readonly DateTimeOffset _now;

    public FixedTimeProvider(DateTimeOffset now)
    {
        _now = now;
    }

    public override DateTimeOffset GetUtcNow()
    {
        return _now;
    }
}
