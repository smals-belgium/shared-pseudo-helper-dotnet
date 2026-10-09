// <copyright file="PointEqualityTests.cs" company="Riziv-Inami">
// Copyright (c) Riziv-Inami. All rights reserved.
// </copyright>

namespace Nihdi.Common.Pseudonymisation.Tests.PortSpecific;

/// <summary>
/// Tests for <see cref="Point"/> equality. Two points are equal when they have the same domain
/// and the same X coordinate; Y is not compared.
/// <para>
/// Tests in this folder exist only in the C# port.
/// </para>
/// </summary>
[TestClass]
public class PointEqualityTests
{
    // A valid P-521 point, taken from the existing tests.
    private const string _x =
        "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAMDEy" +
        "MzQ1Njc4OTEwDAAAAAAAAAAB";

    private const string _y =
        "ABxCeSYmGgumIwOuFyKmK1l8bilTjL+NK" +
        "Elpegi8mcsn3QB6grG5PHpgD8XCTPTGhDrT6HsGuEkEOVwZ23maSliV";

    private static readonly Domain _domain = CreateTestDomain("test", 8);

    [TestMethod]
    public void Equals_OnTwoPointsBuiltFromTheSameCoordinates_IsTrue()
    {
        var one = _domain.PseudonymFactory.FromXy(_x, _y);
        var other = _domain.PseudonymFactory.FromXy(_x, _y);

        Assert.IsTrue(one.Equals(other));
        Assert.AreEqual(one.GetHashCode(), other.GetHashCode());
    }

    [TestMethod]
    public void Equals_OnTwoPointsWithTheSameXAndOppositeY_IsTrueAndHashesAlike()
    {
        var negated = Negate();

        var one = _domain.PseudonymFactory.FromXy(_x, _y);
        var other = _domain.PseudonymFactory.FromXy(Encode(negated.XCoord), Encode(negated.YCoord));

        // Only X matters, so these are the same pseudonym, and they must have the same hash code.
        Assert.AreEqual(one.X(), other.X(), "the fixture must produce two points sharing the same X");
        Assert.IsTrue(one.Equals(other));
        Assert.AreEqual(one.GetHashCode(), other.GetHashCode(), $"X={one.X()}");
    }

    [TestMethod]
    public void Equals_OnTwoPointsWithDifferentX_IsFalse()
    {
        var doubled = Double();

        var one = _domain.PseudonymFactory.FromXy(_x, _y);
        var other = _domain.PseudonymFactory.FromXy(Encode(doubled.XCoord), Encode(doubled.YCoord));

        Assert.AreNotEqual(one.X(), other.X(), "the fixture must produce two points with different X");
        Assert.IsFalse(one.Equals(other), $"one.X()={one.X()} other.X()={other.X()}");
    }

    private static Org.BouncyCastle.Math.EC.ECPoint BasePoint()
    {
        var curve = EcCurveNames.GetCurveFromString("P-521");
        return curve.CreatePoint(
            new BigInteger(1, Convert.FromBase64String(_x)),
            new BigInteger(1, Convert.FromBase64String(_y)));
    }

    private static Org.BouncyCastle.Math.EC.ECPoint Negate() => BasePoint().Negate().Normalize();

    private static Org.BouncyCastle.Math.EC.ECPoint Double() => BasePoint().Twice().Normalize();

    private static string Encode(Org.BouncyCastle.Math.EC.ECFieldElement coordinate)
        => Convert.ToBase64String(coordinate.GetEncoded());
}
