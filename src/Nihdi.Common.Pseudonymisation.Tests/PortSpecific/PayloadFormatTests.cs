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

        // JObject indents its output by default, which would make every request to eHealth larger.
        StringAssert.Contains(payload, "\"crv\":\"P-521\"");
        Assert.IsFalse(payload.Contains('\n'), $"The payload should be compact but contains a newline: {payload}");
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
