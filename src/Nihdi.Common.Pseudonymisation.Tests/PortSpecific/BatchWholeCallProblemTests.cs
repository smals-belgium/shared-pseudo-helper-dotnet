// <copyright file="BatchWholeCallProblemTests.cs" company="Riziv-Inami">
// Copyright (c) Riziv-Inami. All rights reserved.
// </copyright>

using Nihdi.Common.Pseudonymisation.Exceptions;

namespace Nihdi.Common.Pseudonymisation.Tests.PortSpecific;

/// <summary>
/// Tests for a `multiple` call that eHealth refuses as a whole (missing right, unknown path...):
/// the answer is a problem and has no `outputs`. The caller must receive that problem, not an
/// unrelated exception.
/// <para>
/// Tests in this folder exist only in the C# port.
/// </para>
/// </summary>
[TestClass]
public class BatchWholeCallProblemTests
{
    private const string _domainKey = "test";
    private const string _targetDomainKey = "target";

    // Answer received from the acceptance environment for a client without the `convert` right.
    private const string _missingScope =
        "{\"type\":\"https://www.gcloud.belgium.be/rest/problems/missingScope\"," +
        "\"title\":\"Missing Scope\",\"status\":403," +
        "\"detail\":\"All of the following scopes needed: convert\"," +
        "\"requiredScopes\":[\"convert\"]}";

    // Coordinates of a valid P-521 point, taken from the existing tests.
    private const string _x =
        "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAMDEy" +
        "MzQ1Njc4OTEwDAAAAAAAAAAB";

    private const string _y =
        "ABxCeSYmGgumIwOuFyKmK1l8bilTjL+NK" +
        "Elpegi8mcsn3QB6grG5PHpgD8XCTPTGhDrT6HsGuEkEOVwZ23maSliV";

    private const string _pseudonymInTransitRaw =
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

    [TestMethod]
    public void Pseudonymize_WhenTheWholeCallIsRefused_ThrowsTheProblem()
    {
        var domain = CreateDomain(_domainKey, new RecordingPseudonymisationClient(_missingScope));
        var multiple = new MultipleValue(
            domain,
            new List<object> { domain.ValueFactory.From("012345678910"), domain.ValueFactory.From("012345678911") });

        var exception = Assert.ThrowsException<AggregateException>(() => multiple.Pseudonymize().Wait());

        AssertIsMissingScope(exception);
    }

    [TestMethod]
    public void ConvertTo_OnPseudonyms_WhenTheWholeCallIsRefused_ThrowsTheProblem()
    {
        var client = new RecordingPseudonymisationClient(_missingScope);
        var domain = CreateDomain(_domainKey, client);
        var toDomain = CreateDomain(_targetDomainKey, client);
        var pseudonym = domain.PseudonymFactory.FromXy(_x, _y);
        IMultiplePseudonym multiple = new MultiplePseudonym(domain, new List<IPseudonym> { pseudonym, pseudonym });

        var exception = Assert.ThrowsException<AggregateException>(() => multiple.ConvertTo(toDomain).Wait());

        AssertIsMissingScope(exception);
    }

    [TestMethod]
    public void ConvertTo_OnPseudonymsInTransit_WhenTheWholeCallIsRefused_ThrowsTheProblem()
    {
        var client = new RecordingPseudonymisationClient(_missingScope);
        var domain = CreateDomain(_domainKey, client);
        var toDomain = CreateDomain(_targetDomainKey, client);
        IMultiplePseudonymInTransit multiple = TwoPseudonymsInTransit(domain);

        var exception = Assert.ThrowsException<AggregateException>(() => multiple.ConvertTo(toDomain).Wait());

        AssertIsMissingScope(exception);
    }

    [TestMethod]
    public void Identify_WhenTheWholeCallIsRefused_ThrowsTheProblem()
    {
        var domain = CreateDomain(_domainKey, new RecordingPseudonymisationClient(_missingScope));
        IMultiplePseudonymInTransit multiple = TwoPseudonymsInTransit(domain);

        var exception = Assert.ThrowsException<AggregateException>(() => multiple.Identify().Wait());

        AssertIsMissingScope(exception);
    }

    private static MultiplePseudonymInTransit TwoPseudonymsInTransit(Domain domain)
    {
        var pseudonymInTransit = domain.PseudonymInTransitFactory.FromSec1AndTransitInfo(_pseudonymInTransitRaw);
        return new MultiplePseudonymInTransit(domain, new List<object> { pseudonymInTransit, pseudonymInTransit });
    }

    private static void AssertIsMissingScope(AggregateException exception)
    {
        var problem = exception.InnerException as EHealthProblemException;
        Assert.IsNotNull(problem, $"Expected an EHealthProblemException but got {exception.InnerException}");
        Assert.AreEqual("https://www.gcloud.belgium.be/rest/problems/missingScope", problem.Problem.Type);
        Assert.AreEqual("403", problem.Problem.Status);
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
