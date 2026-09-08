// <copyright file="PseudonymInTransitFactoryTests.cs" company="Riziv-Inami">
// Copyright (c) Riziv-Inami. All rights reserved.
// </copyright>

using Nihdi.Common.Pseudonymisation.Internal;

namespace Nihdi.Common.Pseudonymisation.Tests;

[TestClass]
public class PseudonymInTransitFactoryTests
{
    private static readonly string _sec1CompressedBase64 =
        "AwAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAADAx" +
        "MjM0NTY3ODkxMAwAAAAAAAAAAQ";

    private static readonly string _sec1Base64 =
        "BAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAADAx" +
        "MjM0NTY3ODkxMAwAAAAAAAAAAQAcQnkmJhoLpiMDrhcipitZfG4pU4y_jShJaXoI" +
        "vJnLJ90AeoKxuTx6YA_Fwkz0xoQ60-h7BrhJBDlcGdt5mkpYlQ";

    private static readonly string _transitInfoRaw =
       "eyJhbGciOiJkaXIiLCJlbmMiOiJBMjU2R0NNIiwia2lkIjoiMjAyMi0xMiIsImF1" +
        "ZCI6Imh0dHBzOi8vYXBpLWludC5laGVhbHRoLmZnb3YuYmUvcHNldWRvL3YxL2Rv" +
        "bWFpbnMvdWhtZXBfdjEifQ..osrl3KS4nkheJvcJ.pXN4Asfg8RGtsoV529YoFRW" +
        "P_XSXUViR-wxuvwYTvN9fMSDksq7qZMmmqDstyGyOidHKHrVvtqB0PFrek71P4K8" +
        "Rp0rDuvAc6RC2cbdwV08Ksw6t3Wf72H8c8QDKGKmYb84z_oH8TMnY26cAm0nC2Hb" +
        "18H-SXTh8xFXe3DK8y06wx4rAAXFZGsXayloJ6oweux_tvKQ4NSWi3gzhjVi0g-q" +
        "WR9TYZNj9NNyU9eeSDk9UsXJ8cugpvStu6oFOCbW4520fl6h5oaJ7Rye3.IEd7uL" +
        "w-ICHAwqIzfrjOFw";

    private static readonly string _x =
        "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAMDEy" +
        "MzQ1Njc4OTEwDAAAAAAAAAAB";

    private static readonly string _y = "ABxCeSYmGgumIwOuFyKmK1l8bilTjL+NK" +
        "Elpegi8mcsn3QB6grG5PHpgD8XCTPTGhDrT6HsGuEkEOVwZ23maSliV";

    private static readonly IDomain _domain = CreateTestDomain("test", 8);

    [TestMethod]
    public void FromXYAndTransitInfo_asString()
    {
        var pseudonymInTransit = _domain.PseudonymInTransitFactory.FromXYAndTransitInfo(_x, _y, _transitInfoRaw);
        Assert.AreEqual(_transitInfoRaw, pseudonymInTransit.GetTransitInfo().AsString());
        Assert.AreEqual(_sec1Base64, pseudonymInTransit.Pseudonym().AsString());
    }

    [TestMethod]
    public void FromXYAndTransitInfo_asShortString()
    {
        var pseudonymInTransit = _domain.PseudonymInTransitFactory
            .FromXYAndTransitInfo(_x, _y, _transitInfoRaw);
        Assert.AreEqual(_transitInfoRaw, pseudonymInTransit.GetTransitInfo().AsString());
        Assert.AreEqual(_sec1CompressedBase64, pseudonymInTransit.Pseudonym().AsShortString());
    }

    [TestMethod]
    public void FromSec1AndTransitInfo_Compressed_AsString()
    {
        var pseudonymInTransit = _domain.PseudonymInTransitFactory
            .FromSec1AndTransitInfo(_sec1CompressedBase64 + ":" + _transitInfoRaw);
        Assert.AreEqual(_transitInfoRaw, pseudonymInTransit.GetTransitInfo().AsString());
        Assert.AreEqual(_sec1CompressedBase64, pseudonymInTransit.Pseudonym().AsShortString());
    }

    [TestMethod]
    public void FromSec1AndTransitInfo_AsString()
    {
        var pseudonymInTransit = _domain.PseudonymInTransitFactory
            .FromSec1AndTransitInfo(_sec1Base64 + ":" + _transitInfoRaw);
        Assert.AreEqual(_transitInfoRaw, pseudonymInTransit.GetTransitInfo().AsString());
        Assert.AreEqual(_sec1Base64, pseudonymInTransit.Pseudonym().AsString());
    }
}
