// <copyright file="TransitInfoTests.cs" company="Riziv-Inami">
// Copyright (c) Riziv-Inami. All rights reserved.
// </copyright>

using Nihdi.Common.Pseudonymisation.Internal;

namespace Nihdi.Common.Pseudonymisation.Tests;

[TestClass]
public class TransitInfoTests
{
    private static readonly Domain _domain = CreateTestDomain("test", 8);

    [TestMethod]
    public void Header()
    {
        var raw =
        "eyJhdWQiOiJodHRwczovL2FwaS1hY3B0LmVoZWFsdGguZmdvdi5iZS9wc2V1ZG8v" +
        "djEvZG9tYWlucy9laGVhbHRoX3YxIiwiZW5jIjoiQTI1NkdDTSIsImV4cCI6MTcx" +
        "ODIwMzI4OCwiaWF0IjoxNzE4MjAyNjg4LCJhbGciOiJkaXIiLCJraWQiOiJiNTRj" +
        "ZTNlNC1lN2M1LTQ1NWYtODA4ZS02OWEwM2EzN2E4NWYifQ..zO-S0LyrwtQLb-x9" +
        ".oB87loxuJfNmQbif4hHLh2Mvot17jxeqpBfsjayqyXVKMXB8-QMZYBY1OgwmWU7" +
        "ZJKvbBU62f0I6FRZIoKMQjlPMoNNJmnc2FkaIpyi6TLAciZgdolJZwZgIN5_gdK" +
        "dURIJBFOH_MEyZCCAcK6TuYRM98aGPV2SMU06RUnqrWZa1eie93w4u.SNkaDxhvp" +
        "QaQL0aBumRLmQ";

        var transitInfo = new TransitInfo(_domain, raw);
        var headers = transitInfo.Header();
        Assert.AreEqual(6, headers.Count);
        Assert.AreEqual(
            "https://api-acpt.ehealth.fgov.be/pseudo/v1/domains/ehealth_v1",
            headers["aud"].ToString());

        Assert.AreEqual("A256GCM", headers["enc"]?.ToString());
        Assert.AreEqual(long.Parse("1718203288"), long.Parse(headers["exp"]?.ToString()!));
        Assert.AreEqual(long.Parse("1718202688"), long.Parse(headers["iat"]?.ToString()!));
        Assert.AreEqual("dir", headers["alg"]?.ToString());
        Assert.AreEqual("b54ce3e4-e7c5-455f-808e-69a03a37a85f", headers["kid"]?.ToString());
    }
}
