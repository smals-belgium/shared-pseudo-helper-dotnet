// <copyright file="MultipleValuePseudonymizeTests.cs" company="Riziv-Inami">
// Copyright (c) Riziv-Inami. All rights reserved.
// </copyright>

namespace Nihdi.Common.Pseudonymisation.Tests.PortSpecific;

/// <summary>
/// Tests for <see cref="MultipleValue.Pseudonymize"/>: it must not hold the calling thread while
/// eHealth answers.
/// <para>
/// Tests in this folder exist only in the C# port.
/// </para>
/// </summary>
[TestClass]
public class MultipleValuePseudonymizeTests
{
    private const string _domainKey = "test";

    // How long the test waits for Pseudonymize() to return its task. The call itself does no
    // network access, so it returns at once; the margin is only there for a loaded build agent.
    private static readonly TimeSpan _returnTimeout = TimeSpan.FromSeconds(5);

    [DataTestMethod]
    [DataRow(1, DisplayName = "one value, sent to the single endpoint")]
    [DataRow(2, DisplayName = "two values, sent to the `multiple` endpoint")]
    public void Pseudonymize_WhileEHealthHasNotAnswered_ReturnsATaskThatIsNotCompleted(int valueCount)
    {
        var answerFromEHealth = new TaskCompletionSource<string>();
        var client = new RecordingPseudonymisationClient(answerFromEHealth.Task);
        var multiple = CreateCollection(client, valueCount);

        // Pseudonymize() runs on another thread, so that this test fails instead of hanging when
        // the call waits for eHealth.
        Task<IMultiplePseudonymInTransit>? returned = null;
        var call = Task.Run(() => { returned = multiple.Pseudonymize(); });
        if (!call.Wait(_returnTimeout))
        {
            // Releases the thread that is still waiting inside Pseudonymize().
            answerFromEHealth.TrySetException(new NotSupportedException("end of test"));
            Assert.Fail("Pseudonymize() did not return: it holds the calling thread until eHealth answers.");
        }

        // The answer of eHealth is left pending: completing it here, even to clean up, would run
        // the rest of Pseudonymize() on a made-up answer.
        Assert.IsFalse(
            returned!.IsCompleted,
            "eHealth has not answered yet, so the returned task cannot be completed.");
    }

    private static IMultipleValue CreateCollection(IPseudonymisationClient client, int valueCount)
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

        var values = new List<object>();
        for (int i = 0; i < valueCount; i++)
        {
            values.Add(domain.ValueFactory.From("01234567891" + i));
        }

        return new MultipleValue(domain, values);
    }
}
