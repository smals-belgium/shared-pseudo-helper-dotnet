// <copyright file="MultiplePseudonymSingleElementTests.cs" company="Riziv-Inami">
// Copyright (c) Riziv-Inami. All rights reserved.
// </copyright>

using Nihdi.Common.Pseudonymisation.Exceptions;

namespace Nihdi.Common.Pseudonymisation.Tests.PortSpecific;

/// <summary>
/// Tests for the single-element case of <see cref="MultiplePseudonym"/>.
/// <para>
/// Tests in this folder exist only in the C# port.
/// </para>
/// </summary>
[TestClass]
public class MultiplePseudonymSingleElementTests
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

    // An eHealth error response: it has no `transitInfo`, so it is rejected.
    private const string _problemResponse =
        "{\"type\":\"urn:problem-type:ehealth:pseudonymisation:invalid-input\"," +
        "\"title\":\"Invalid input\",\"status\":\"400\",\"detail\":\"boom\"}";

    [TestMethod]
    public void ConvertTo_WithASingleElement_CallsTheSingleEndpointOnly()
    {
        var client = new RecordingPseudonymisationClient();
        var multiple = CreateSingleElementCollection(client, out var toDomain);

        Ignoring(((IMultiplePseudonym)multiple).ConvertTo(toDomain));

        CollectionAssert.AreEqual(new[] { "ConvertTo" }, client.Calls);
    }

    [TestMethod]
    public void ConvertTo_WithASingleElement_TurnsAnEHealthProblemIntoAnElement()
    {
        var client = new RecordingPseudonymisationClient(_problemResponse);
        var multiple = CreateSingleElementCollection(client, out var toDomain);

        var converted = ((IMultiplePseudonym)multiple).ConvertTo(toDomain).Result;

        Assert.AreEqual(1, converted.Size());
        Assert.ThrowsException<EHealthProblemException>(() => _ = converted[0]);
    }

    private static MultiplePseudonym CreateSingleElementCollection(
        IPseudonymisationClient client,
        out IDomain toDomain)
    {
        var domain = CreateDomain(_domainKey, client);
        toDomain = CreateDomain(_targetDomainKey, client);

        var pseudonym = domain.PseudonymFactory.FromXy(_x, _y);
        return new MultiplePseudonym(domain, new List<IPseudonym> { pseudonym });
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

    /// <summary>
    /// Waits for the task and ignores any error. The fake client always fails; this test only
    /// checks which endpoints were called.
    /// </summary>
    /// <param name="task">The task to wait for.</param>
    private static void Ignoring(Task? task)
    {
        try
        {
            task?.Wait();
        }
        catch (AggregateException)
        {
            // Expected: the fake client always fails.
        }
    }
}
