// <copyright file="PseudonymInTransitTests.cs" company="Riziv-Inami">
// Copyright (c) Riziv-Inami. All rights reserved.
// </copyright>

using Nihdi.Common.Pseudonymisation.Internal;

namespace Nihdi.Common.Pseudonymisation.Tests;

[TestClass]
public class PseudonymInTransitTests
{
    private static readonly string _sec1CompressedBase64 =
        "AgBCRmd68AVWDtBMWajDd2W63E_j7X0WmQdMnz9m3wFkoxH-muD692vlltWjKGCRxnftuT9nAGkmDRomoHZQc8G3hQ";

    private static readonly string _transitInfo =
        "eyJhbGciOiJkaXIiLCJlbmMiOiJBMjU2R0NNIiwia2lkIjoiMjAyMi0xMiIsImF1" +
        "ZCI6Imh0dHBzOi8vYXBpLWludC5laGVhbHRoLmZnb3YuYmUvcHNldWRvL3YxL2Rv" +
        "bWFpbnMvdWhtZXBfdjEifQ..osrl3KS4nkheJvcJ.pXN4Asfg8RGtsoV529YoFRW" +
        "P_XSXUViR-wxuvwYTvN9fMSDksq7qZMmmqDstyGyOidHKHrVvtqB0PFrek71P4K8" +
        "Rp0rDuvAc6RC2cbdwV08Ksw6t3Wf72H8c8QDKGKmYb84z_oH8TMnY26cAm0nC2Hb" +
        "18H-SXTh8xFXe3DK8y06wx4rAAXFZGsXayloJ6oweux_tvKQ4NSWi3gzhjVi0g-q" +
        "WR9TYZNj9NNyU9eeSDk9UsXJ8cugpvStu6oFOCbW4520fl6h5oaJ7Rye3.IEd7uL" +
        "w-ICHAwqIzfrjOFw";

    private static readonly IDomain _domain = CreateTestDomain("test", 8);

    [TestMethod]
    public void AsString()
    {
        var expectedSec1AndTransitInfo = _sec1CompressedBase64 + ":" + _transitInfo;
        var actualPseudonymInTransit = _domain.PseudonymInTransitFactory
            .FromSec1AndTransitInfo(expectedSec1AndTransitInfo).AsShortString();

        Assert.AreEqual(expectedSec1AndTransitInfo, actualPseudonymInTransit);
    }
}
