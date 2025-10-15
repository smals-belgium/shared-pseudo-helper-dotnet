// <copyright file="MultiplePseudonymInTransitTests.cs" company="Riziv-Inami">
// Copyright (c) Riziv-Inami. All rights reserved.
// </copyright>

using FluentAssertions;
using Nihdi.Common.Pseudonymisation.Internal;

namespace Nihdi.Common.Pseudonymisation.Tests;

[TestClass]
public class MultiplePseudonymInTransitTests
{
    private static readonly IDomain _domain =
        new Domain(
            "test",
            null,
            EcCurveNames.GetCurveFromString("P-521"),
            null,
            8,
            null,
            null,
            null,
            null,
            null,
            null);

    private static readonly string _pseudonymInTransitRaw =
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

    [TestMethod]
    public void Create_without_collection_add_10_pseudonyms()
    {
        var multiple = new MultiplePseudonymInTransit(_domain);
        var pseudonymInTransit = _domain
            .PseudonymInTransitFactory
            .FromSec1AndTransitInfo(_pseudonymInTransitRaw);

        for (var i = 0; i < 10; i++)
        {
            multiple.Add(pseudonymInTransit);
        }

        Assert.AreEqual(10, multiple.Size());
    }

    [TestMethod]
    public void Create_without_collection_add_11_pseudonyms_should_throw_exception()
    {
        var multiple = new MultiplePseudonymInTransit(_domain);
        var pseudonymInTransit = _domain
            .PseudonymInTransitFactory
            .FromSec1AndTransitInfo(_pseudonymInTransitRaw);

        for (var i = 0; i < 10; i++)
        {
            multiple.Add(pseudonymInTransit);
        }

        Assert.AreEqual(10, multiple.Size());

        var action = () => multiple.Add(pseudonymInTransit);
        action.Should().Throw<ArgumentOutOfRangeException>();
    }

    [TestMethod]
    public void Create_with_collection_add_10_pseudonyms()
    {
        var pseudonymInTransit =
            _domain
            .PseudonymInTransitFactory
            .FromSec1AndTransitInfo(_pseudonymInTransitRaw);

        var pseudonymInTransitList = new List<object>();

        for (var i = 0; i < 10; i++)
        {
            pseudonymInTransitList.Add(pseudonymInTransit);
        }

        var multiple = new MultiplePseudonymInTransit(_domain, pseudonymInTransitList!);

        Assert.AreEqual(10, multiple.Size());
    }

    [TestMethod]
    public void Create_with_collection_add_11_pseudonyms_Should_throw_exception()
    {
        var pseudonymInTransit =
            _domain
            .PseudonymInTransitFactory
            .FromSec1AndTransitInfo(_pseudonymInTransitRaw);

        var pseudonymInTransitList = new List<object>();

        for (var i = 0; i < 10; i++)
        {
            pseudonymInTransitList.Add(pseudonymInTransit);
        }

        var multiple = new MultiplePseudonymInTransit(_domain, pseudonymInTransitList!);
        var action = () => multiple.Add(pseudonymInTransit);
        Assert.AreEqual(10, multiple.Size());
        action.Should().Throw<ArgumentOutOfRangeException>();
    }
}
