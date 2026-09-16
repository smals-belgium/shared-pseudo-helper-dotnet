// <copyright file="TransitInfoEqualityTests.cs" company="Riziv-Inami">
// Copyright (c) Riziv-Inami. All rights reserved.
// </copyright>

namespace Nihdi.Common.Pseudonymisation.Tests.PortSpecific;

/// <summary>
/// Tests for <see cref="TransitInfo"/> equality, hash code and ToString, and for the
/// <see cref="PseudonymInTransit"/> members that use them.
/// <para>
/// Tests in this folder exist only in the C# port.
/// </para>
/// </summary>
[TestClass]
public class TransitInfoEqualityTests
{
    private const string TransitInfoRaw =
        "eyJhdWQiOiJodHRwczovL2FwaS1hY3B0LmVoZWFsdGguZmdvdi5iZS9wc2V1ZG8v" +
        "djEvZG9tYWlucy9laGVhbHRoX3YxIiwiZW5jIjoiQTI1NkdDTSIsImV4cCI6MTcx" +
        "ODIwMzI4OCwiaWF0IjoxNzE4MjAyNjg4LCJhbGciOiJkaXIiLCJraWQiOiJiNTRj" +
        "ZTNlNC1lN2M1LTQ1NWYtODA4ZS02OWEwM2EzN2E4NWYifQ..zO-S0LyrwtQLb-x9" +
        ".oB87loxuJfNmQbif4hHLh2Mvot17jxeqpBfsjayqyXVKMXB8-QMZYBY1OgwmWU7" +
        "ZJKvbBU62f0I6FRZIoKMQjlPMoNNJmnc2FkaIpyi6TLAciZgdolJZwZgIN5_gdK" +
        "dURIJBFOH_MEyZCCAcK6TuYRM98aGPV2SMU06RUnqrWZa1eie93w4u.SNkaDxhvp" +
        "QaQL0aBumRLmQ";

    private const string PseudonymInTransitRaw =
        "BAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAADAx" +
        "MjM0NTY3ODkxMAwAAAAAAAAAAQHjvYbZ2eX0Wdz8UejdWdSmg5HWrHNActe2loX3" +
        "Q2Y02CL_hX1ORsOFn_A6PbMLOXvFLBeE-Ue2-8aj5iSGZbWnag" +
        ":" +
        "eyJhbGciOiJkaXIiLCJlbmMiOiJBMjU2R0NNIiwia2lkIjoiMjAyMi0xMiIsImF1" +
        "ZCI6Imh0dHBzOi8vYXBpLWludC5laGVhbHRoLmZnb3YuYmUvcHNldWRvL3YxL2Rv" +
        "bWFpbnMvdWhtZXBfdjEifQ..osrl3KS4nkheJvcJ.pXN4Asfg8RGtsoV529YoFRW" +
        "P_XSXUViR-wxuvwYTvN9fMSDksq7qZMmmqDstyGyOidHKHrVvtqB0PFrek71P4K8" +
        "Rp0rDuvAc6RC2cbdwV08Ksw6t3Wf72H8c8QDKGKmYb84z_oH8TMnY26cAm0nC2Hb" +
        "18H-SXTh8xFXe3DK8y06wx4rAAXFZGsXayloJ6oweux_tvKQ4NSWi3gzhjVi0g-q" +
        "WR9TYZNj9NNyU9eeSDk9UsXJ8cugpvStu6oFOCbW4520fl6h5oaJ7Rye3.IEd7uL" +
        "w-ICHAwqIzfrjOFw";

    private static readonly Domain _domain = CreateTestDomain("test", 8);

    [TestMethod]
    public void Equals_OnTwoTransitInfosBuiltFromTheSameString_IsTrue()
    {
        var one = new TransitInfo(_domain, TransitInfoRaw);
        var other = new TransitInfo(_domain, TransitInfoRaw);

        Assert.IsTrue(one.Equals(other));
    }

    [TestMethod]
    public void GetHashCode_OnTwoTransitInfosBuiltFromTheSameString_IsTheSame()
    {
        var one = new TransitInfo(_domain, TransitInfoRaw);
        var other = new TransitInfo(_domain, TransitInfoRaw);

        Assert.AreEqual(one.GetHashCode(), other.GetHashCode());
    }

    [TestMethod]
    public void Equals_OnTwoTransitInfosBuiltFromDifferentStrings_IsFalse()
    {
        var one = new TransitInfo(_domain, TransitInfoRaw);
        var other = new TransitInfo(_domain, TransitInfoRaw.Replace("zO-S0LyrwtQLb-x9", "zO-S0LyrwtQLb-x8"));

        Assert.IsFalse(one.Equals(other));
    }

    [TestMethod]
    public void ToString_WhenThePayloadIsNotDecrypted_ReportsTheHeaderAndSaysSo()
    {
        var transitInfo = new TransitInfo(_domain, TransitInfoRaw);

        var text = transitInfo.ToString();

        StringAssert.Contains(text, "\"header\"");
        StringAssert.Contains(text, "b54ce3e4-e7c5-455f-808e-69a03a37a85f");
        StringAssert.Contains(text, "\"not decrypted\"");
    }

    [TestMethod]
    public void PseudonymInTransit_Equals_OnTwoInstancesBuiltFromTheSameString_IsTrue()
    {
        var one = _domain.PseudonymInTransitFactory.FromSec1AndTransitInfo(PseudonymInTransitRaw);
        var other = _domain.PseudonymInTransitFactory.FromSec1AndTransitInfo(PseudonymInTransitRaw);

        Assert.IsTrue(one.Equals(other));
        Assert.AreEqual(one.GetHashCode(), other.GetHashCode());
    }

    [TestMethod]
    public void PseudonymInTransit_ToString_EmbedsTheTransitInfoAsJson()
    {
        var pseudonymInTransit = _domain.PseudonymInTransitFactory.FromSec1AndTransitInfo(PseudonymInTransitRaw);

        StringAssert.Contains(pseudonymInTransit.ToString(), "\"transitInfo\": {\"header\": ");
    }
}
