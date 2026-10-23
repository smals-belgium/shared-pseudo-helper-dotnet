// <copyright file="PayloadFormatTests.cs" company="Riziv-Inami">
// Copyright (c) Riziv-Inami. All rights reserved.
// </copyright>

namespace Nihdi.Common.Pseudonymisation.Tests.PortSpecific;

/// <summary>
/// Tests on the request bodies sent to eHealth.
/// <para>
/// Tests in this folder exist only in the C# port.
/// </para>
/// </summary>
[TestClass]
public class PayloadFormatTests
{
    private const string _domainKey = "test";

    // Coordinates of a valid P-521 point, taken from the existing tests. `_y` contains a `+`.
    private const string _x =
        "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAMDEy" +
        "MzQ1Njc4OTEwDAAAAAAAAAAB";

    private const string _y =
        "ABxCeSYmGgumIwOuFyKmK1l8bilTjL+NK" +
        "Elpegi8mcsn3QB6grG5PHpgD8XCTPTGhDrT6HsGuEkEOVwZ23maSliV";

    [TestMethod]
    public void CreatePayload_ProducesCompactJson()
    {
        var client = new RecordingPseudonymisationClient();
        var domain = CreateDomain(client);

        // The call fails because the fake client always fails, but the payload is recorded first,
        // and the payload is what this test checks.
        try
        {
            domain.ValueFactory.From("012345678910").Pseudonymize().Wait();
        }
        catch (AggregateException)
        {
            // Expected: the fake client always fails.
        }

        Assert.AreEqual(1, client.Payloads.Count);
        var payload = client.Payloads[0];

        // An indented body would make every request to eHealth larger.
        StringAssert.Contains(payload, "\"crv\":\"P-521\"");
        Assert.IsFalse(payload.Contains('\n'), $"The payload should be compact but contains a newline: {payload}");
    }

    [TestMethod]
    public void CreatePayload_KeepsThePlusSignsOfTheBase64CoordinatesUnescaped()
    {
        var domain = CreateDomain(new RecordingPseudonymisationClient());

        // The coordinates are standard base64, so they contain `+`. The default JSON encoder would
        // write it as a Unicode escape, which differs from what the Java helper sends.
        var pseudonym = domain.PseudonymFactory.FromXy(_x, _y);
        Assert.IsTrue(pseudonym.Y().Contains('+'), "The test needs a coordinate that contains a `+`.");

        var payload = domain.CreatePayloadString(pseudonym);

        StringAssert.Contains(payload, $"\"y\":\"{pseudonym.Y()}\"");
        Assert.IsFalse(payload.Contains("\\u"), $"The payload should not contain a Unicode escape: {payload}");
    }

    private static Domain CreateDomain(IPseudonymisationClient client)
    {
        // `crv` must be set, because building the request payload needs it.
        return new Domain(
            _domainKey,
            "P-521",
            EcCurveNames.GetCurveFromString("P-521"),
            null,
            8,
            null,
            null,
            null,
            null,
            client,
            null);
    }
}
