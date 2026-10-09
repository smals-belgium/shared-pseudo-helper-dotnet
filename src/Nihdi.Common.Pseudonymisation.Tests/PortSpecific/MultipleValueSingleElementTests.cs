// <copyright file="MultipleValueSingleElementTests.cs" company="Riziv-Inami">
// Copyright (c) Riziv-Inami. All rights reserved.
// </copyright>

namespace Nihdi.Common.Pseudonymisation.Tests.PortSpecific;

/// <summary>
/// Tests for the single-element case of <see cref="MultipleValue"/>.
/// <para>
/// Tests in this folder exist only in the C# port.
/// </para>
/// </summary>
[TestClass]
public class MultipleValueSingleElementTests
{
    private const string _domainKey = "test";

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

    private const string _pseudonymizeResponse =
        "{\"domain\":\"" + _domainKey + "\",\"x\":\"" + _x + "\",\"y\":\"" + _y +
        "\",\"transitInfo\":\"" + _transitInfoRaw + "\"}";

    [TestMethod]
    public void Pseudonymize_WithASingleElement_YieldsAPseudonymInTransitAndNotATask()
    {
        var client = new RecordingPseudonymisationClient(_pseudonymizeResponse);
        var multiple = CreateSingleElementCollection(client);

        var pseudonymsInTransit = multiple.Pseudonymize().Result;

        Assert.AreEqual(1, pseudonymsInTransit.Size());

        // Reading the element casts it to IPseudonymInTransit. This fails if a Task was stored
        // instead of the pseudonym.
        Assert.IsInstanceOfType(pseudonymsInTransit[0], typeof(IPseudonymInTransit));
    }

    [TestMethod]
    public void Pseudonymize_WithASingleElement_SurfacesAFailedCall()
    {
        var client = new RecordingPseudonymisationClient();
        var multiple = CreateSingleElementCollection(client);

        // If a Task were stored instead of its result, the error would be hidden and the
        // collection returned as if the call had worked. The error is carried by the returned
        // task: Pseudonymize() itself does not wait for eHealth.
        Assert.ThrowsException<AggregateException>(() => multiple.Pseudonymize().Wait());
    }

    [TestMethod]
    public void Pseudonymize_WithASingleElement_CallsTheSingleEndpointOnly()
    {
        var client = new RecordingPseudonymisationClient(_pseudonymizeResponse);
        var multiple = CreateSingleElementCollection(client);

        multiple.Pseudonymize().Wait();

        CollectionAssert.AreEqual(new[] { "Pseudonymize" }, client.Calls);
    }

    private static IMultipleValue CreateSingleElementCollection(IPseudonymisationClient client)
    {
        // `crv` must be set, because building the request payload needs it.
        var domain = new Domain(
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

        var value = domain.ValueFactory.From("012345678910");
        return new MultipleValue(domain, new List<object> { value });
    }
}
