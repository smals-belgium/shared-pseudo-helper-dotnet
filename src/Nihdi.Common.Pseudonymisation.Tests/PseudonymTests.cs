// <copyright file="PseudonymTests.cs" company="Riziv-Inami">
// Copyright (c) Riziv-Inami. All rights reserved.
// </copyright>

using Nihdi.Common.Pseudonymisation.Internal;
using Org.BouncyCastle.Math;

namespace Nihdi.Common.Pseudonymisation.Tests;

[TestClass]
public class PseudonymTests
{
    private static readonly IDomain _domain = CreateTestDomain("test", 8);

    private static readonly string _one = Convert.ToBase64String(BigInteger.One.ToByteArray());

    private static readonly string _two = Convert.ToBase64String(BigInteger.Two.ToByteArray());

    [TestMethod]
    public void X_Base64_Should_Return_X_Encoded_Base64()
    {
        var pseudonym = _domain.PseudonymFactory.FromXy(_one, _two);
        var output = pseudonym.X();
        Assert.AreEqual(
            BigInteger.One,
            new BigInteger(Convert.FromBase64String(output)));
    }

    [TestMethod]
    public void Y_Base64_Should_Return_Y_Encoded_Base64()
    {
        var pseudonym = _domain.PseudonymFactory.FromXy(_one, _two);
        var output = pseudonym.Y();
        Assert.AreEqual(
            BigInteger.Two,
            new BigInteger(Convert.FromBase64String(output)));
    }

    [TestMethod]
    public void From_Sec1()
    {
        var sec1 = "AwAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAADAxMjM0NTY3ODkxMAwAAAAAAAAAAQ";
        var x = "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAMDEyMzQ1Njc4OTEwDAAAAAAAAAAB";
        var y = "ABxCeSYmGgumIwOuFyKmK1l8bilTjL+NKElpegi8mcsn3QB6grG5PHpgD8XCTPTGhDrT6HsGuEkEOVwZ23maSliV";
        var pseudonym = _domain.PseudonymFactory.FromXy(x, y);

        Assert.AreEqual(sec1, pseudonym.AsShortString());
    }
}
