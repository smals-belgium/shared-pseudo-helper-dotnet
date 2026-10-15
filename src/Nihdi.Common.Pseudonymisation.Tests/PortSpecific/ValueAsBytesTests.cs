// <copyright file="ValueAsBytesTests.cs" company="Riziv-Inami">
// Copyright (c) Riziv-Inami. All rights reserved.
// </copyright>

namespace Nihdi.Common.Pseudonymisation.Tests.PortSpecific;

/// <summary>
/// Tests for reading a value back as bytes, after it has been turned into a point of the curve.
/// <para>
/// Tests in this folder exist only in the C# port.
/// </para>
/// </summary>
[TestClass]
public class ValueAsBytesTests
{
    [TestMethod]
    public void AsBytes_WithAShortValue_ReturnsTheValue()
    {
        AssertRoundTrip(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 });
    }

    [TestMethod]
    public void AsBytes_WithAValueOfMaximumSize_ReturnsTheValue()
    {
        // 56 bytes is the largest value that fits in a P-521 point with a buffer of 8 bytes.
        AssertRoundTrip(Enumerable.Range(1, 56).Select(i => (byte)i).ToArray());
    }

    [TestMethod]
    public void AsBytes_WithAValueStartingWithZeros_KeepsTheZeros()
    {
        // The leading zeros cannot be told apart from the padding of the X coordinate:
        // only the length byte stored after the value says where the value starts.
        AssertRoundTrip(new byte[] { 0, 0, 42 });
    }

    private static void AssertRoundTrip(byte[] bytes)
    {
        var domain = new Domain(
            "test",
            "P-521",
            EcCurveNames.GetCurveFromString("P-521"),
            null,
            8,
            null,
            null,
            null,
            null,
            null,
            null);

        var value = domain.ValueFactory.From(bytes);

        CollectionAssert.AreEqual(bytes, value.AsBytes());
    }
}
