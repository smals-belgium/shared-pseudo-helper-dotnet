// <copyright file="MultipleValueTests.cs" company="Riziv-Inami">
// Copyright (c) Riziv-Inami. All rights reserved.
// </copyright>

using FluentAssertions;
using Nihdi.Common.Pseudonymisation.Internal;
using static Nihdi.Common.Pseudonymisation.Tests.TestUtils;

namespace Nihdi.Common.Pseudonymisation.Tests;

[TestClass]
public class MultipleValueTests
{
    private static readonly IDomain _domain = CreateTestDomain("test", 8);

    [TestMethod]
    public void Create_without_collection_add_10_values()
    {
        var multiple = new MultipleValue(_domain);
        var value = _domain.ValueFactory.From("0");

        for (int j = 0; j < 10; j++)
        {
            multiple.Add(value);
        }

        Assert.AreEqual(10, multiple.Size());
    }

    [TestMethod]
    public void Create_without_collection_add_11_values_should_throw_Exception()
    {
        var multiple = new MultipleValue(_domain);
        var value = _domain.ValueFactory.From("0");

        for (int j = 0; j < 10; j++)
        {
            multiple.Add(value);
        }

        Assert.AreEqual(10, multiple.Size());
        var action = () => multiple.Add(value);
        action.Should().ThrowExactly<ArgumentOutOfRangeException>();
    }

    [TestMethod]
    public void Create_with_collection_add_10_values()
    {
        var value = _domain.ValueFactory.From("0");
        var valueList = new List<object>();

        for (int j = 0; j < 10; j++)
        {
            valueList.Add(value);
        }

        var multiple = new MultipleValue(_domain, valueList);
        Assert.AreEqual(10, multiple.Size());
    }

    [TestMethod]
    public void Create_with_collection_add_11_values_should_throw_Exception()
    {
        var value = _domain.ValueFactory.From("0");
        var valueList = new List<object>();

        for (int j = 0; j < 10; j++)
        {
            valueList.Add(value);
        }

        var multiple = new MultipleValue(_domain, valueList);
        Assert.AreEqual(10, multiple.Size());

        var action = () => multiple.Add(value);
        action.Should().ThrowExactly<ArgumentOutOfRangeException>();
    }
}
