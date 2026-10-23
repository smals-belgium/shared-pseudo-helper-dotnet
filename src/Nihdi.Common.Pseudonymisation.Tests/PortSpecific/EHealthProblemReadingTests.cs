// <copyright file="EHealthProblemReadingTests.cs" company="Riziv-Inami">
// Copyright (c) Riziv-Inami. All rights reserved.
// </copyright>

using Nihdi.Common.Pseudonymisation.Exceptions;

namespace Nihdi.Common.Pseudonymisation.Tests.PortSpecific;

/// <summary>
/// Tests on how an error entry of eHealth is turned into an <see cref="IEHealthProblem"/>: every
/// field is read as text, whatever its JSON type, and an entry without `type` is reported as an
/// unexpected result that quotes the entry.
/// <para>
/// Tests in this folder exist only in the C# port.
/// </para>
/// </summary>
[TestClass]
public class EHealthProblemReadingTests
{
    private const string _domainKey = "test";

    [TestMethod]
    public void Problem_WithANumericStatus_ReadsTheStatusAsText()
    {
        // RFC 9457 defines `status` as a number. Reading it as a string would fail on a number.
        var problem = ProblemOf(
            "{\"type\":\"urn:problem-type:ehealth:pseudonymisation:invalid-input\"," +
            "\"title\":\"Invalid input\",\"status\":400,\"detail\":\"boom\"}");

        Assert.AreEqual("urn:problem-type:ehealth:pseudonymisation:invalid-input", problem.Type);
        Assert.AreEqual("Invalid input", problem.Title);
        Assert.AreEqual("400", problem.Status);
        Assert.AreEqual("boom", problem.Detail);
    }

    [TestMethod]
    public void Problem_WithMissingOrNullFields_ReadsThemAsEmptyText()
    {
        var problem = ProblemOf("{\"type\":\"urn:x\",\"title\":null}");

        Assert.AreEqual("urn:x", problem.Type);
        Assert.AreEqual(string.Empty, problem.Title);
        Assert.AreEqual(string.Empty, problem.Status);
        Assert.AreEqual(string.Empty, problem.Detail);
    }

    [TestMethod]
    public void Problem_WithoutAType_IsAnUnexpectedResultThatQuotesTheEntry()
    {
        var problem = ProblemOf("{\"error\":\"a+b <c> & d\",\"code\":7}");

        Assert.AreEqual("Unexpected Result", problem.Title);
        Assert.AreEqual("undefined", problem.Status);
        StringAssert.Contains(problem.Type, "unexpected-result");
        StringAssert.StartsWith(problem.Detail, "Unable to convert the response from eHealth into a EHealthProblem.");

        // The entry is quoted indented, and its characters are not escaped.
        StringAssert.Contains(problem.Detail, "\"error\": \"a+b <c> & d\"");
        StringAssert.Contains(problem.Detail, "\"code\": 7");
        Assert.IsTrue(problem.Detail.Contains('\n'), "The quoted entry should be indented over several lines.");
    }

    private static IEHealthProblem ProblemOf(string problemOutput)
    {
        var response = "{\"outputs\":[" + problemOutput + "," + problemOutput + "]}";
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
            new RecordingPseudonymisationClient(response),
            null);
        var multiple = new MultipleValue(domain, new List<object> { domain.ValueFactory.From("012345678910"), domain.ValueFactory.From("012345678911") });

        var results = multiple.Pseudonymize().Result;

        // The entry has no pseudonym, so reading it raises the problem it carries.
        var exception = Assert.ThrowsException<EHealthProblemException>(() => _ = results[0]);
        return exception.Problem;
    }
}
