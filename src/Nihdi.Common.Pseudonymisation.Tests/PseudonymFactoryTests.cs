// <copyright file="PseudonymFactoryTests.cs" company="Riziv-Inami">
// Copyright (c) Riziv-Inami. All rights reserved.
// </copyright>

using Nihdi.Common.Pseudonymisation.Internal;

namespace Nihdi.Common.Pseudonymisation.Tests;

[TestClass]
public class PseudonymFactoryTests
{
    private static readonly IDomain _domain = CreateTestDomain("test", 8);

    private static readonly string _sec1Compressed = "AwAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAD" +
        "AxMjM0NTY3ODkxMAwAAAAAAAAAAQ";

    private static readonly string _sec1 =
        "BAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA" +
        "DAxMjM0NTY3ODkxMAwAAAAAAAAAAQAcQnkmJhoLpiMDrhcipitZfG4pU4y_jS" +
        "hJaXoIvJnLJ90AeoKxuTx6YA_Fwkz0xoQ60-h7BrhJBDlcGdt5mkpYlQ";

    private static readonly string _x =
        "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAMDE" +
        "yMzQ1Njc4OTEwDAAAAAAAAAAB";

    private static readonly string _xShortened =
        "MDEyMzQ1Njc4OTEwDAAAAAAAAAAB";

    private static readonly string _y =
        "ABxCeSYmGgumIwOuFyKmK1l8bilTjL+NKElpegi8mcsn3QB6grG5PHpgD8XCTPT" +
        "GhDrT6HsGuEkEOVwZ23maSliV";

    [TestMethod]
    public void FromXY_AsShortString()
    {
        var pseudonym = _domain.PseudonymFactory.FromXy(_x, _y);
        Assert.AreEqual(_sec1Compressed, pseudonym.AsShortString());
    }

    [TestMethod]
    public void FromXY_AsString()
    {
        var pseudonym = _domain.PseudonymFactory.FromXy(_x, _y);
        Assert.AreEqual(_sec1, pseudonym.AsString());
    }

    [TestMethod]
    public void FromX()
    {
        var pseudonym = _domain.PseudonymFactory.FromX(_x);
        Assert.AreEqual(_y, pseudonym.Y());
    }

    [TestMethod]
    public void FromX_Small()
    {
        var pseudonym = _domain.PseudonymFactory.FromX(_xShortened);
        Assert.AreEqual(_y, pseudonym.Y());
    }

    [TestMethod]
    public void Multiple_No_Collection_Add_10_Pseudonyms()
    {
        var multiple = _domain.PseudonymFactory.Multiple();
        Assert.AreEqual(0, multiple.Size());
    }

    [TestMethod]
    public void Multiple_With_Collection_Containing_10_Pseudonyms()
    {
        var pseudonym1 = _domain.PseudonymFactory.FromXy(_x, _y);
        var pseudonym2 = _domain.PseudonymFactory.FromXy(_x, _y);
        var pseudonymList = new List<IPseudonym> { pseudonym1, pseudonym2 };

        var multiple = _domain.PseudonymFactory.Multiple(pseudonymList);
        Assert.AreEqual(2, multiple.Size());
        Assert.AreSame(multiple[0], pseudonym1);
        Assert.AreSame(multiple[1], pseudonym2);
    }
}
