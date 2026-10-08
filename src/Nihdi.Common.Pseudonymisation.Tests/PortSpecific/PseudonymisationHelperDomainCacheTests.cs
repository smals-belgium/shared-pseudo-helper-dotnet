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
    /// How long a test waits before it decides that loading a domain is stuck.
    /// The helper gives up on eHealth after five seconds, so ten is a safe margin while still
    /// failing if the wait has no bound at all.
    /// </summary>
    private static readonly TimeSpan _loadTimeout = TimeSpan.FromSeconds(10);

    [TestMethod]
    public void GetDomain_WhenEHealthAnswersWithAnotherDomainName_StillReturnsTheDomain()
    {
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
        // This call to eHealth never answers. Only the five-second give-up lets the load end.
        var neverAnswers = new TaskCompletionSource<string>();
        var helper = CreateHelper(new CountingPseudonymisationClient(() => neverAnswers.Task));

        var load = helper.GetDomain(_requestedKey);
        var endedInTime = Task.WhenAny(load, Task.Delay(_loadTimeout)).Result == load;

        // Let eHealth go, so the test does not leave a task pending behind it.
        neverAnswers.TrySetResult(_answerFromAnotherDomain);

        Assert.IsTrue(endedInTime, "Loading the domain waited for eHealth with no time limit.");
        Assert.IsTrue(load.IsFaulted, "Loading the domain should have failed once the time limit was reached.");
    }

    private static PseudonymisationHelper CreateHelper(IPseudonymisationClient client)
    {
        return PseudonymisationHelper.Builder()
            .PseudonymisationClient(client)
            .Build()
            ?? throw new InvalidOperationException("The builder returned no helper.");
    }

    /// <summary>
    /// A pseudonymisation client that answers what it was told to and counts the calls.
    /// </summary>
    private sealed class CountingPseudonymisationClient : IPseudonymisationClient
    {
        private readonly Func<Task<string>> _answer;

        public CountingPseudonymisationClient(string domainJson)
            : this(() => Task.FromResult(domainJson))
        {
        }

        public CountingPseudonymisationClient(Func<Task<string>> answer)
        {
            _answer = answer;
        }

        public int GetDomainCalls
        {
            get; private set;
        }

        public Task<string> GetDomain(string domainKey)
        {
            GetDomainCalls++;
            return _answer();
        }

        public Task<string> Identify(string domainKey, string payload) => throw new NotSupportedException();

        public Task<string> IdentifyMultiple(string domainKey, string payload) => throw new NotSupportedException();

        public Task<string> Pseudonymize(string domainKey, string payload) => throw new NotSupportedException();

        public Task<string> PseudonymizeMultiple(string domainKey, string payload) => throw new NotSupportedException();

        public Task<string> ConvertTo(string fromDomainKey, string toDomainKey, string payload) => throw new NotSupportedException();

        public Task<string> ConvertMultipleTo(string fromDomainKey, string toDomainKey, string payload) => throw new NotSupportedException();
    }
}
