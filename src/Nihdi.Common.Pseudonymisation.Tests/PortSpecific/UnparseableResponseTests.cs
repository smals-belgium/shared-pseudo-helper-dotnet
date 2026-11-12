// <copyright file="UnparseableResponseTests.cs" company="Riziv-Inami">
// Copyright (c) Riziv-Inami. All rights reserved.
// </copyright>

using System.Text.Json;
using Nihdi.Common.Pseudonymisation.Exceptions;

namespace Nihdi.Common.Pseudonymisation.Tests.PortSpecific;

/// <summary>
/// Tests on a response of eHealth that is not JSON (the HTML error page of a proxy, an
/// empty body): the caller gets an <see cref="EHealthProblemException"/>, like for any other
/// problem, with the raw response quoted and the reading error as its cause.
/// <para>
/// Tests in this folder exist only in the C# port.
/// </para>
/// </summary>
[TestClass]
public class UnparseableResponseTests
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

    [DataTestMethod]
    [DataRow("<html>502 Bad Gateway</html>")]
    [DataRow("   ")]
    public void Parse_WithAResponseThatIsNotJson_ThrowsTheUnparseableResultProblem(string rawResponse)
    {
        var exception = Assert.ThrowsException<EHealthProblemException>(() => JsonHelper.Parse(rawResponse));

        Assert.AreEqual("urn:problem-type:smals:pseudo-helper:unparseable-result", exception.Problem.Type);
        Assert.AreEqual("Unparseable Result", exception.Problem.Title);
        Assert.AreEqual("undefined", exception.Problem.Status);
        StringAssert.StartsWith(exception.Problem.Detail, "Unable to parse the response from eHealth.");
        StringAssert.EndsWith(exception.Problem.Detail, "Response was: \n" + rawResponse);
        Assert.AreEqual(exception.Problem.Detail, exception.Message);
    }

    [TestMethod]
    public void Parse_WithAnEmptyResponse_ThrowsTheUnparseableResultProblem()
    {
        var exception = Assert.ThrowsException<EHealthProblemException>(() => JsonHelper.Parse(string.Empty));

        StringAssert.Contains(exception.Problem.Type, "unparseable-result");
    }

    [TestMethod]
    public void Parse_WithMalformedJson_KeepsTheReadingErrorAsTheCause()
    {
        var exception = Assert.ThrowsException<EHealthProblemException>(() => JsonHelper.Parse("{\"outputs\":"));

        Assert.IsInstanceOfType(exception.InnerException, typeof(JsonException));
    }

    [DataTestMethod]
    [DataRow("[1,2]")]
    [DataRow("null")]
    [DataRow("42")]
    public void Parse_WithJsonThatIsNotAnObject_ThrowsTheUnexpectedResultProblem(string rawResponse)
    {
        // The text parses, so it is not an unparseable result.
        var exception = Assert.ThrowsException<EHealthProblemException>(() => JsonHelper.Parse(rawResponse));

        Assert.AreEqual("urn:problem-type:smals:pseudo-helper:unexpected-result", exception.Problem.Type);
        Assert.AreEqual("Unexpected Result", exception.Problem.Title);
        Assert.AreEqual("undefined", exception.Problem.Status);
        StringAssert.EndsWith(exception.Problem.Detail, "Response was: \n" + rawResponse);
        Assert.IsNull(exception.InnerException);
    }

    [TestMethod]
    public void Parse_WithAJsonObject_ReturnsIt()
    {
        var response = JsonHelper.Parse("{\"outputs\":[]}");

        Assert.IsNotNull(response["outputs"]);
    }

    [TestMethod]
    public void Pseudonymize_WithASingleValue_ReportsAnUnparseableResponseAsAProblem()
    {
        var domain = CreateDomain(_domainKey, new RecordingPseudonymisationClient("<html>"));
        var value = domain.ValueFactory.From("012345678910");

        var exception = Assert.ThrowsException<EHealthProblemException>(
            () => value.Pseudonymize().GetAwaiter().GetResult());

        StringAssert.Contains(exception.Problem.Type, "unparseable-result");
    }

    [TestMethod]
    public void ConvertTo_WithASinglePseudonym_ReportsAnUnparseableResponseAsAProblem()
    {
        var client = new RecordingPseudonymisationClient(string.Empty);
        var domain = CreateDomain(_domainKey, client);
        var toDomain = CreateDomain(_targetDomainKey, client);
        var pseudonym = domain.PseudonymFactory.FromXy(_x, _y);

        var exception = Assert.ThrowsException<EHealthProblemException>(
            () => pseudonym.ConvertTo(toDomain).GetAwaiter().GetResult());

        StringAssert.Contains(exception.Problem.Type, "unparseable-result");
    }

    [TestMethod]
    public void Pseudonymize_WithSeveralValues_ReportsAnUnparseableResponseAsAProblem()
    {
        var domain = CreateDomain(_domainKey, new RecordingPseudonymisationClient("<html>"));
        var multiple = new MultipleValue(
            domain,
            new List<object> { domain.ValueFactory.From("012345678910"), domain.ValueFactory.From("012345678911") });

        var exception = Assert.ThrowsException<EHealthProblemException>(
            () => multiple.Pseudonymize().GetAwaiter().GetResult());

        StringAssert.Contains(exception.Problem.Type, "unparseable-result");
    }

    [TestMethod]
    public void ConvertTo_WithSeveralPseudonyms_ReportsAnUnparseableResponseAsAProblem()
    {
        var client = new RecordingPseudonymisationClient("<html>");
        var domain = CreateDomain(_domainKey, client);
        var toDomain = CreateDomain(_targetDomainKey, client);
        var pseudonym = domain.PseudonymFactory.FromXy(_x, _y);
        IMultiplePseudonym multiple = new MultiplePseudonym(domain, new List<IPseudonym> { pseudonym, pseudonym });

        var exception = Assert.ThrowsException<EHealthProblemException>(
            () => multiple.ConvertTo(toDomain).GetAwaiter().GetResult());

        StringAssert.Contains(exception.Problem.Type, "unparseable-result");
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
