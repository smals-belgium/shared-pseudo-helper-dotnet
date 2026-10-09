// <copyright file="MultipleResponseOutputsTests.cs" company="Riziv-Inami">
// Copyright (c) Riziv-Inami. All rights reserved.
// </copyright>

using Nihdi.Common.Pseudonymisation.Exceptions;

namespace Nihdi.Common.Pseudonymisation.Tests.PortSpecific;

/// <summary>
/// Tests for reading the response of the `multiple` endpoints: eHealth returns `outputs` as a
/// JSON array, with one entry per input, in the same order.
/// <para>
/// Tests in this folder exist only in the C# port.
/// </para>
/// </summary>
[TestClass]
public class MultipleResponseOutputsTests
{
    private const string _domainKey = "test";
    private const string _targetDomainKey = "target";

    // Coordinates of a valid P-521 point, taken from the existing tests.
    private const string _x =
        "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAMDEy" +
        "MzQ1Njc4OTEwDAAAAAAAAAAB";

    private const string _y =
        "ABxCeSYmGgumIwOuFyKmK1l8bilTjL+NK" +
        "Elpegi8mcsn3QB6grG5PHpgD8XCTPTGhDrT6HsGuEkEOVwZ23maSliV";

    private const string _transitInfoRaw =
        "eyJhbGciOiJkaXIiLCJlbmMiOiJBMjU2R0NNIiwia2lkIjoiMjAyMi0xMiIsImF1" +
        "ZCI6Imh0dHBzOi8vYXBpLWludC5laGVhbHRoLmZnb3YuYmUvcHNldWRvL3YxL2Rv" +
        "bWFpbnMvdWhtZXBfdjEifQ..osrl3KS4nkheJvcJ.pXN4Asfg8RGtsoV529YoFRW" +
        "P_XSXUViR-wxuvwYTvN9fMSDksq7qZMmmqDstyGyOidHKHrVvtqB0PFrek71P4K8" +
        "Rp0rDuvAc6RC2cbdwV08Ksw6t3Wf72H8c8QDKGKmYb84z_oH8TMnY26cAm0nC2Hb" +
        "18H-SXTh8xFXe3DK8y06wx4rAAXFZGsXayloJ6oweux_tvKQ4NSWi3gzhjVi0g-q" +
        "WR9TYZNj9NNyU9eeSDk9UsXJ8cugpvStu6oFOCbW4520fl6h5oaJ7Rye3.IEd7uL" +
        "w-ICHAwqIzfrjOFw";

    // An eHealth error entry: it has no `transitInfo`, so it is rejected.
    private const string _problemOutput =
        "{\"type\":\"urn:problem-type:ehealth:pseudonymisation:invalid-input\"," +
        "\"title\":\"Invalid input\",\"status\":\"400\",\"detail\":\"boom\"}";

    [TestMethod]
    public void Pseudonymize_WithSeveralValues_ReadsOutputsAsAnArray()
    {
        var client = new RecordingPseudonymisationClient(
            Outputs(PseudonymInTransitOutput(_domainKey), PseudonymInTransitOutput(_domainKey)));
        var domain = CreateDomain(_domainKey, client);
        var multiple = new MultipleValue(
            domain,
            new List<object> { domain.ValueFactory.From("012345678910"), domain.ValueFactory.From("012345678911") });

        var pseudonymsInTransit = multiple.Pseudonymize().Result;

        CollectionAssert.AreEqual(new[] { "PseudonymizeMultiple" }, client.Calls);
        Assert.AreEqual(2, pseudonymsInTransit.Size());
        Assert.IsInstanceOfType(pseudonymsInTransit[0], typeof(IPseudonymInTransit));
        Assert.IsInstanceOfType(pseudonymsInTransit[1], typeof(IPseudonymInTransit));
    }

    [TestMethod]
    public void Pseudonymize_WithSeveralValues_KeepsAnEHealthProblemAtItsPosition()
    {
        var client = new RecordingPseudonymisationClient(
            Outputs(PseudonymInTransitOutput(_domainKey), _problemOutput));
        var domain = CreateDomain(_domainKey, client);
        var multiple = new MultipleValue(
            domain,
            new List<object> { domain.ValueFactory.From("012345678910"), domain.ValueFactory.From("012345678911") });

        var pseudonymsInTransit = multiple.Pseudonymize().Result;

        Assert.AreEqual(2, pseudonymsInTransit.Size());
        Assert.IsInstanceOfType(pseudonymsInTransit[0], typeof(IPseudonymInTransit));
        Assert.ThrowsException<EHealthProblemException>(() => _ = pseudonymsInTransit[1]);
    }

    [TestMethod]
    public void ConvertTo_WithSeveralPseudonyms_ReadsOutputsAsAnArray()
    {
        // eHealth answers with pseudonyms of the target domain.
        var client = new RecordingPseudonymisationClient(
            Outputs(PseudonymInTransitOutput(_targetDomainKey), PseudonymInTransitOutput(_targetDomainKey)));
        var domain = CreateDomain(_domainKey, client);
        var toDomain = CreateDomain(_targetDomainKey, client);
        var pseudonym = domain.PseudonymFactory.FromXy(_x, _y);
        IMultiplePseudonym multiple = new MultiplePseudonym(domain, new List<IPseudonym> { pseudonym, pseudonym });

        var converted = multiple.ConvertTo(toDomain).Result;

        CollectionAssert.AreEqual(new[] { "ConvertMultipleTo" }, client.Calls);
        Assert.AreEqual(2, converted.Size());
        Assert.IsInstanceOfType(converted[0], typeof(IPseudonymInTransit));
        Assert.IsInstanceOfType(converted[1], typeof(IPseudonymInTransit));
    }

    private static string PseudonymInTransitOutput(string domainKey)
    {
        return "{\"domain\":\"" + domainKey + "\",\"x\":\"" + _x + "\",\"y\":\"" + _y +
            "\",\"transitInfo\":\"" + _transitInfoRaw + "\"}";
    }

    private static string Outputs(params string[] outputs)
    {
        return "{\"outputs\":[" + string.Join(",", outputs) + "]}";
    }

    private static Domain CreateDomain(string key, IPseudonymisationClient client)
    {
        // `crv` must be set, because building the request payload needs it.
        return new Domain(
            key,
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
