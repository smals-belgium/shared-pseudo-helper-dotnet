// <copyright file="TestUtils.cs" company="Riziv-Inami">
// Copyright (c) Riziv-Inami. All rights reserved.
// </copyright>

namespace Nihdi.Common.Pseudonymisation.Tests;

using Nihdi.Common.Pseudonymisation.Internal;

public class TestUtils
{
    public static Domain CreateTestDomain(string key, int bufferSize)
    {
        return new Domain(
            key,
            null,
            EcCurveNames.GetCurveFromString("P-521"),
            null,
            bufferSize,
            null,
            null,
            null,
            null,
            null,
            null);
    }
}
