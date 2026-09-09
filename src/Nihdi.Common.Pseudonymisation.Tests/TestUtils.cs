// <copyright file="TestUtils.cs" company="Riziv-Inami">
// Copyright (c) Riziv-Inami. All rights reserved.
// </copyright>

namespace Nihdi.Common.Pseudonymisation.Tests;

using Nihdi.Common.Pseudonymisation.Internal;

public class TestUtils
{
    public static Domain CreateTestDomain(string key, int bufferSize)
    {
        return CreateTestDomain(key, bufferSize, null);
    }

    /// <summary>
    /// Same as <see cref="CreateTestDomain(string, int)"/>, but with an audience.
    /// The two-argument version leaves the audience null, so it cannot test header validation.
    /// </summary>
    /// <param name="key">The domain key.</param>
    /// <param name="bufferSize">The domain buffer size.</param>
    /// <param name="audience">The audience the domain expects in a transit info header.</param>
    /// <returns>A <see cref="Domain"/> instance.</returns>
    public static Domain CreateTestDomain(string key, int bufferSize, string? audience)
    {
        return new Domain(
            key,
            null,
            EcCurveNames.GetCurveFromString("P-521"),
            audience,
            bufferSize,
            null,
            null,
            null,
            null,
            null,
            null);
    }
}
