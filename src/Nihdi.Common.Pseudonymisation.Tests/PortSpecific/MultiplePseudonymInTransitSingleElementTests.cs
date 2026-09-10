// <copyright file="MultiplePseudonymInTransitSingleElementTests.cs" company="Riziv-Inami">
// Copyright (c) Riziv-Inami. All rights reserved.
// </copyright>

using Nihdi.Common.Pseudonymisation.Exceptions;

namespace Nihdi.Common.Pseudonymisation.Tests.PortSpecific;

/// <summary>
/// Tests for the single-element case of <see cref="MultiplePseudonymInTransit"/>.
/// <para>
/// Tests in this folder exist only in the C# port.
/// </para>
/// </summary>
[TestClass]
public class MultiplePseudonymInTransitSingleElementTests
{
    private const string PseudonymInTransitRaw =
        "BAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAADAx" +
        "MjM0NTY3ODkxMAwAAAAAAAAAAQHjvYbZ2eX0Wdz8UejdWdSmg5HWrHNActe2loX3" +
        "Q2Y02CL_hX1ORsOFn_A6PbMLOXvFLBeE-Ue2-8aj5iSGZbWnag" +
        ":" +
        "eyJhbGciOiJkaXIiLCJlbmMiOiJBMjU2R0NNIiwia2lkIjoiMjAyMi0xMiIsImF1" +
        "ZCI6Imh0dHBzOi8vYXBpLWludC5laGVhbHRoLmZnb3YuYmUvcHNldWRvL3YxL2Rv" +
        "bWFpbnMvdWhtZXBfdjEifQ..osrl3KS4nkheJvcJ.pXN4Asfg8RGtsoV529YoFRW" +
        "P_XSXUViR-wxuvwYTvN9fMSDksq7qZMmmqDstyGyOidHKHrVvtqB0PFrek71P4K8" +
        "Rp0rDuvAc6RC2cbdwV08Ksw6t3Wf72H8c8QDKGKmYb84z_oH8TMnY26cAm0nC2Hb" +
        "18H-SXTh8xFXe3DK8y06wx4rAAXFZGsXayloJ6oweux_tvKQ4NSWi3gzhjVi0g-q" +
        "WR9TYZNj9NNyU9eeSDk9UsXJ8cugpvStu6oFOCbW4520fl6h5oaJ7Rye3.IEd7uL" +
        "w-ICHAwqIzfrjOFw";

    // An eHealth error response: it has no `transitInfo` and no `x`/`y`, so it is rejected.
    private const string ProblemResponse =
        "{\"type\":\"urn:problem-type:ehealth:pseudonymisation:invalid-input\"," +
        "\"title\":\"Invalid input\",\"status\":\"400\",\"detail\":\"boom\"}";

    [TestMethod]
    public void ConvertTo_WithASingleElement_CallsTheSingleEndpointOnly()
    {
        var client = new RecordingPseudonymisationClient();
        var multiple = CreateSingleElementCollection(client, out var toDomain);

        Ignoring(((IMultiplePseudonymInTransit)multiple).ConvertTo(toDomain));

        CollectionAssert.AreEqual(new[] { "ConvertTo" }, client.Calls);
    }

    [TestMethod]
    public void Identify_WithASingleElement_CallsTheSingleEndpointOnly()
    {
        var client = new RecordingPseudonymisationClient();
        var multiple = CreateSingleElementCollection(client, out _);

        Ignoring(multiple.Identify());

        CollectionAssert.AreEqual(new[] { "Identify" }, client.Calls);
    }

    [TestMethod]
    public void ConvertTo_WithASingleElement_TurnsAnEHealthProblemIntoAnElement()
    {
        var client = new RecordingPseudonymisationClient(ProblemResponse);
        var multiple = CreateSingleElementCollection(client, out var toDomain);

        var task = ((IMultiplePseudonymInTransit)multiple).ConvertTo(toDomain);
        Assert.IsNotNull(task);
        var converted = task.Result;

        Assert.AreEqual(1, converted.Size());
        Assert.ThrowsException<EHealthProblemException>(() => _ = converted[0]);
    }

    [TestMethod]
    public void Identify_WithASingleElement_TurnsAnEHealthProblemIntoAnElement()
    {
        var client = new RecordingPseudonymisationClient(ProblemResponse);
        var multiple = CreateSingleElementCollection(client, out _);

        var identified = multiple.Identify().Result;

        Assert.AreEqual(1, identified.Size());
        Assert.ThrowsException<EHealthProblemException>(() => _ = identified[0]);
    }

    [TestMethod]
    public void ConvertTo_WithAnEmptyCollection_CallsNothing()
    {
        var client = new RecordingPseudonymisationClient();
        var domain = CreateDomain("test", client);
        var toDomain = CreateDomain("target", client);
        var multiple = new MultiplePseudonymInTransit(domain);

        Ignoring(((IMultiplePseudonymInTransit)multiple).ConvertTo(toDomain));

        CollectionAssert.AreEqual(Array.Empty<string>(), client.Calls);
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

    private static MultiplePseudonymInTransit CreateSingleElementCollection(
        IPseudonymisationClient client,
        out IDomain toDomain)
    {
        var domain = CreateDomain("test", client);
        toDomain = CreateDomain("target", client);

        var pseudonymInTransit = domain.PseudonymInTransitFactory.FromSec1AndTransitInfo(PseudonymInTransitRaw);
        return new MultiplePseudonymInTransit(domain, new List<object> { pseudonymInTransit });
    }

    /// <summary>
    /// Waits for the task and ignores any error. The fake client always fails; these tests only
    /// check which endpoints were called.
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

    /// <summary>
    /// A fake pseudonymisation client that records which endpoints are called.
    /// </summary>
    private sealed class RecordingPseudonymisationClient : IPseudonymisationClient
    {
        private readonly string? _response;

        public RecordingPseudonymisationClient(string? response = null)
        {
            _response = response;
        }

        public List<string> Calls { get; } = new List<string>();

        public Task<string> GetDomain(string domainKey) => Record(nameof(GetDomain));

        public Task<string> Identify(string domainKey, string payload) => Record(nameof(Identify));

        public Task<string> IdentifyMultiple(string domainKey, string payload) => Record(nameof(IdentifyMultiple));

        public Task<string> Pseudonymize(string domainKey, string payload) => Record(nameof(Pseudonymize));

        public Task<string> PseudonymizeMultiple(string domainKey, string payload) => Record(nameof(PseudonymizeMultiple));

        public Task<string> ConvertTo(string fromDomainKey, string toDomainKey, string payload) => Record(nameof(ConvertTo));

        public Task<string> ConvertMultipleTo(string fromDomainKey, string toDomainKey, string payload) => Record(nameof(ConvertMultipleTo));

        private Task<string> Record(string name)
        {
            Calls.Add(name);
            return _response != null
                ? Task.FromResult(_response)
                : Task.FromException<string>(new NotSupportedException(name));
        }
    }
}
