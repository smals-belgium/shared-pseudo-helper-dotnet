// <copyright file="PseudonymisationHelperDomainCacheTests.cs" company="Riziv-Inami">
// Copyright (c) Riziv-Inami. All rights reserved.
// </copyright>

namespace Nihdi.Common.Pseudonymisation.Tests.PortSpecific;

/// <summary>
/// Tests for the way <see cref="PseudonymisationHelper"/> loads a domain and keeps it.
/// <para>
/// Tests in this folder exist only in the C# port.
/// </para>
/// </summary>
[TestClass]
public class PseudonymisationHelperDomainCacheTests
{
    private const string _requestedKey = "uhmep_v1";

    // eHealth answers with a `domain` field that differs from the key that was asked for.
    private const string _answerFromAnotherDomain =
        "{\"domain\":\"other_v1\"," +
        "\"audience\":\"https://api-acpt.ehealth.fgov.be/pseudo/v1/domains/uhmep_v1\"," +
        "\"bufferSize\":8,\"crv\":\"P-521\",\"timeToLiveInTransit\":\"PT10M\"," +
        "\"jku\":[],\"secretKeys\":[]}";

    /// <summary>
    /// How long a test waits before it decides that loading a domain is stuck. The helper gives up
    /// on its own, so this only stops a broken helper from hanging the test run.
    /// </summary>
    private static readonly TimeSpan _loadTimeout = TimeSpan.FromSeconds(10);

    /// <summary>
    /// The timeout given to the helper in the test that waits for it. It is real time, so it is kept short.
    /// </summary>
    private static readonly TimeSpan _shortTimeout = TimeSpan.FromMilliseconds(200);

    /// <summary>
    /// The longest that test waits for the short timeout to be reached: well under the five seconds
    /// that the helper would use if the configured timeout were ignored.
    /// </summary>
    private static readonly TimeSpan _shortTimeoutGuard = TimeSpan.FromSeconds(4);

    [TestMethod]
    public void GetDomain_CachesUnderTheRequestedKey_EvenWhenEHealthNamesTheDomainDifferently()
    {
        // eHealth is not expected to answer with another name. The two names differ here only
        // so that the test can tell which one the helper uses to find the domain again.
        var client = new CountingPseudonymisationClient(_answerFromAnotherDomain);
        var helper = CreateHelper(client);

        var domain = helper.GetDomain(_requestedKey).Result;

        Assert.IsNotNull(domain);
        Assert.AreEqual("other_v1", domain.Key);
    }

    [TestMethod]
    public void RefreshDomain_GivesBackTheDomainItJustLoaded()
    {
        var helper = CreateHelper(new CountingPseudonymisationClient(_answerFromAnotherDomain));

        var refreshed = helper.RefreshDomain(_requestedKey).Result;

        Assert.AreSame(refreshed, helper.GetDomain(_requestedKey).Result);
    }

    [TestMethod]
    public void GetDomain_CalledTwice_CachesUnderTheRequestedKey()
    {
        var client = new CountingPseudonymisationClient(_answerFromAnotherDomain);
        var helper = CreateHelper(client);

        helper.GetDomain(_requestedKey).Wait();
        helper.GetDomain(_requestedKey).Wait();

        Assert.AreEqual(1, client.GetDomainCalls);
    }

    [TestMethod]
    public void GetDomain_WhenEHealthFails_ReportsThatFailureInsteadOfAMissingKey()
    {
        var client = new CountingPseudonymisationClient(
            () => Task.FromException<string>(new InvalidOperationException("eHealth is down")));
        var helper = CreateHelper(client);

        var raised = Assert.ThrowsException<AggregateException>(() => helper.GetDomain(_requestedKey).Wait());

        var failure = raised.ToString();
        StringAssert.Contains(failure, "eHealth is down");
        Assert.IsFalse(
            failure.Contains(nameof(KeyNotFoundException)),
            $"The reason of the failure was replaced by a missing key: {failure}");
    }

    [TestMethod]
    public void GetDomain_WhenEHealthNeverAnswers_GivesUp()
    {
        // This call to eHealth never answers. Only the give-up after the timeout lets the load end.
        var neverAnswers = new TaskCompletionSource<string>();
        var helper = CreateHelper(new CountingPseudonymisationClient(() => neverAnswers.Task), _shortTimeout);

        var load = helper.GetDomain(_requestedKey);
        var endedInTime = Task.WhenAny(load, Task.Delay(_shortTimeoutGuard)).Result == load;

        // Let eHealth go, so the test does not leave a task pending behind it.
        neverAnswers.TrySetResult(_answerFromAnotherDomain);

        Assert.IsTrue(endedInTime, "Loading the domain waited for eHealth with no time limit.");
        Assert.IsTrue(load.IsFaulted, "Loading the domain should have failed once the time limit was reached.");
        Assert.IsInstanceOfType(load.Exception!.InnerException, typeof(TimeoutException));
    }

    [TestMethod]
    public void RefreshDomain_OfOneDomain_DoesNotWaitForAnotherDomain()
    {
        // eHealth is slow for the first domain asked, and answers at once for the second.
        var slowAnswer = NewPendingAnswer();
        var client = new CountingPseudonymisationClient(
            () => slowAnswer.Task,
            () => Task.FromResult(_answerFromAnotherDomain));
        var helper = CreateHelper(client);

        var slow = helper.RefreshDomain("slow_v1");
        var quick = helper.RefreshDomain(_requestedKey);
        var quickEndedFirst = quick.Wait(TimeSpan.FromSeconds(2)) && !slow.IsCompleted;

        // Let eHealth go, so the test does not leave a task pending behind it.
        slowAnswer.TrySetResult(_answerFromAnotherDomain);

        Assert.IsTrue(quickEndedFirst, "Loading one domain waited for the slow answer of another domain.");
    }

    [TestMethod]
    public void GetDomain_AskedAgainWhileTheFirstLoadIsRunning_CallsEHealthOnlyOnce()
    {
        var answer = NewPendingAnswer();
        var client = new CountingPseudonymisationClient(() => answer.Task);
        var helper = CreateHelper(client);

        var first = helper.GetDomain(_requestedKey);
        var second = helper.GetDomain(_requestedKey);
        answer.SetResult(_answerFromAnotherDomain);

        Assert.AreSame(first.Result, second.Result);
        Assert.AreEqual(1, client.GetDomainCalls);
    }

    [TestMethod]
    public void GetDomain_AskedByManyThreadsAtTheSameMoment_CallsEHealthOnlyOnce()
    {
        const int threads = 16;
        var answer = NewPendingAnswer();
        var client = new CountingPseudonymisationClient(() => answer.Task);
        var helper = CreateHelper(client);

        // The barrier holds every thread until all of them are ready, so that they ask together.
        using var allReady = new Barrier(threads);
        var asked = Enumerable.Range(0, threads)
            .Select(unused => Task.Factory.StartNew(
                () =>
                {
                    allReady.SignalAndWait();
                    return helper.GetDomain(_requestedKey);
                },
                TaskCreationOptions.LongRunning))
            .ToArray();
        var loads = asked.Select(ask => ask.Result).ToArray();
        answer.SetResult(_answerFromAnotherDomain);

        Assert.AreEqual(1, loads.Select(load => load.Result).Distinct().Count(), "Every caller must get the same domain.");
        Assert.AreEqual(1, client.GetDomainCalls);
    }

    [TestMethod]
    public void RefreshDomain_AskedAgainWhileARefreshIsRunning_DoesNotCallEHealthAgain()
    {
        var slowAnswer = NewPendingAnswer();
        var client = new CountingPseudonymisationClient(
            () => Task.FromResult(_answerFromAnotherDomain),
            () => slowAnswer.Task);
        var helper = CreateHelper(client);
        helper.GetDomain(_requestedKey).Wait();

        var first = helper.RefreshDomain(_requestedKey);
        var second = helper.RefreshDomain(_requestedKey);
        slowAnswer.SetResult(_answerFromAnotherDomain);

        Assert.AreSame(first.Result, second.Result);
        Assert.AreEqual(2, client.GetDomainCalls, "One call for the first load, one for the refresh.");
    }

    [TestMethod]
    public void GetDomain_WhileARefreshIsRunning_GivesTheDomainLoadedBeforeWithoutWaiting()
    {
        var slowAnswer = NewPendingAnswer();
        var client = new CountingPseudonymisationClient(
            () => Task.FromResult(_answerFromAnotherDomain),
            () => slowAnswer.Task);
        var helper = CreateHelper(client);
        var loadedBefore = helper.GetDomain(_requestedKey).Result;

        var refresh = helper.RefreshDomain(_requestedKey);
        var askedDuringTheRefresh = helper.GetDomain(_requestedKey);

        Assert.IsTrue(askedDuringTheRefresh.IsCompleted, "Asking for the domain waited for the refresh to end.");
        Assert.AreSame(loadedBefore, askedDuringTheRefresh.Result);

        slowAnswer.SetResult(_answerFromAnotherDomain);

        Assert.AreNotSame(loadedBefore, refresh.Result);
        Assert.AreSame(refresh.Result, helper.GetDomain(_requestedKey).Result);
    }

    [TestMethod]
    public void GetDomain_AfterAFailedRefresh_StillGivesTheDomainLoadedBefore()
    {
        var client = new CountingPseudonymisationClient(
            () => Task.FromResult(_answerFromAnotherDomain),
            () => Task.FromException<string>(new InvalidOperationException("eHealth is down")));
        var helper = CreateHelper(client);
        var loadedBefore = helper.GetDomain(_requestedKey).Result;

        Assert.ThrowsException<AggregateException>(() => helper.RefreshDomain(_requestedKey).Wait());

        Assert.AreSame(loadedBefore, helper.GetDomain(_requestedKey).Result);
    }

    [TestMethod]
    public void GetDomain_AfterAFailedFirstLoad_KeepsFailingWithoutCallingEHealthUntilARefreshIsAsked()
    {
        // A first load that failed is not tried again on its own: the failure stays until
        // RefreshDomain is called. This is what the Java library does and documents.
        var client = new CountingPseudonymisationClient(
            () => Task.FromException<string>(new InvalidOperationException("eHealth is down")),
            () => Task.FromResult(_answerFromAnotherDomain));
        var helper = CreateHelper(client);

        Assert.ThrowsException<AggregateException>(() => helper.GetDomain(_requestedKey).Wait());
        Assert.ThrowsException<AggregateException>(() => helper.GetDomain(_requestedKey).Wait());
        Assert.AreEqual(1, client.GetDomainCalls);

        var refreshed = helper.RefreshDomain(_requestedKey).Result;

        Assert.AreSame(refreshed, helper.GetDomain(_requestedKey).Result);
        Assert.AreEqual(2, client.GetDomainCalls);
    }

    /// <summary>
    /// An answer of eHealth that has not arrived yet: the test decides when it does.
    /// </summary>
    private static TaskCompletionSource<string> NewPendingAnswer()
    {
        // Without this option, SetResult would run the helper's own code before returning.
        return new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
    }

    private static PseudonymisationHelper CreateHelper(IPseudonymisationClient client, TimeSpan? timeout = null)
    {
        var builder = PseudonymisationHelper.Builder().PseudonymisationClient(client);

        if (timeout != null)
        {
            builder.Timeout(timeout.Value);
        }

        return builder
            .Build()
            ?? throw new InvalidOperationException("The builder returned no helper.");
    }

    /// <summary>
    /// A pseudonymisation client that answers what it was told to and counts the calls.
    /// Each call takes the next answer; once they are used up, the last one is given again.
    /// </summary>
    private sealed class CountingPseudonymisationClient : IPseudonymisationClient
    {
        private readonly Func<Task<string>>[] _answers;
        private int _getDomainCalls;

        public CountingPseudonymisationClient(string domainJson)
            : this(() => Task.FromResult(domainJson))
        {
        }

        public CountingPseudonymisationClient(params Func<Task<string>>[] answers)
        {
            _answers = answers;
        }

        public int GetDomainCalls => _getDomainCalls;

        public Task<string> GetDomain(string domainKey, CancellationToken cancellationToken)
        {
            var call = Interlocked.Increment(ref _getDomainCalls);
            return _answers[Math.Min(call, _answers.Length) - 1]();
        }

        public Task<string> Identify(string domainKey, string payload, CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<string> IdentifyMultiple(string domainKey, string payload, CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<string> Pseudonymize(string domainKey, string payload, CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<string> PseudonymizeMultiple(string domainKey, string payload, CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<string> ConvertTo(string fromDomainKey, string toDomainKey, string payload, CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<string> ConvertMultipleTo(string fromDomainKey, string toDomainKey, string payload, CancellationToken cancellationToken) => throw new NotSupportedException();
    }
}
