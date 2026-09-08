// <copyright file="MultiplePseudonymTests.cs" company="Riziv-Inami">
// Copyright (c) Riziv-Inami. All rights reserved.
// </copyright>

using FluentAssertions;
using Nihdi.Common.Pseudonymisation.Internal;

namespace Nihdi.Common.Pseudonymisation.Tests;

[TestClass]
public class MultiplePseudonymTests
{
    private static readonly IDomain _domain = CreateTestDomain("test", 8);

    private static readonly string _x = "BAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAADAx";

    private static readonly string _y = "AVKesmomj6qNSJpqPxEaIegblCAzf8k4gh0V7h/NxrgJ5WDUIha39yfdNeX1maUWrRBwiCNztHkE/ugNiRfFq5WU";

    [TestMethod]
    public void Create_without_collection_add_10_pseudonyms()
    {
        var multiple = new MultiplePseudonym(_domain);
        var pseudonym = _domain.PseudonymFactory.FromXy(_x, _y);

        for (int i = 0; i < 10; i++)
        {
            multiple.Add(pseudonym);
        }

        Assert.AreEqual(10, multiple.Size());
    }

    [TestMethod]
    public void Create_without_collection_add_11_pseudonyms_should_throw_exception()
    {
        var multiple = new MultiplePseudonym(_domain);
        var pseudonym = _domain.PseudonymFactory.FromXy(_x, _y);

        for (int i = 0; i < 10; i++)
        {
            multiple.Add(pseudonym);
        }

        Assert.AreEqual(10, multiple.Size());

        var action = () => multiple.Add(pseudonym);
        action.Should().Throw<ArgumentOutOfRangeException>();
    }

    [TestMethod]
    public void Create_with_collection_add_10_pseudonyms()
    {
        var pseudonym = _domain.PseudonymFactory.FromXy(_x, _y);
        var pseudonymList = new List<object>();

        for (var i = 0; i < 10; i++)
        {
            pseudonymList.Add(pseudonym);
        }

        var multiple = new MultiplePseudonymInTransit(_domain, pseudonymList!);

        Assert.AreEqual(10, multiple.Size());
    }

    [TestMethod]
    public void Create_with_collection_add_11_pseudonyms_should_throw_exception()
    {
        var pseudonym = _domain.PseudonymFactory.FromXy(_x, _y);
        var pseudonymList = new List<IPseudonym>();

        for (var i = 0; i < 10; i++)
        {
            pseudonymList.Add(pseudonym);
        }

        var multiple = new MultiplePseudonym(_domain, pseudonymList!);

        var action = () => multiple.Add(pseudonym);
        Assert.AreEqual(10, multiple.Size());
        action.Should().Throw<ArgumentOutOfRangeException>();
    }
}
