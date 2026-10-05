// <copyright file="PseudonymisationHelperDomainCacheTests.cs" company="Riziv-Inami">
// Copyright (c) Riziv-Inami. All rights reserved.
// </copyright>

namespace Nihdi.Common.Pseudonymisation.Tests.PortSpecific;

/// <summary>
/// Tests for the key under which <see cref="PseudonymisationHelper"/> caches a domain.
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

    [TestMethod]
    public void GetDomain_WhenEHealthAnswersWithAnotherDomainName_StillReturnsTheDomain()
    {
        var client = new CountingPseudonymisationClient(_answerFromAnotherDomain);
        var helper = CreateHelper(client);

        var domain = helper.GetDomain(_requestedKey).Result;

        Assert.IsNotNull(domain);
        Assert.AreEqual("other_v1", domain!.Key);
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

    private static PseudonymisationHelper CreateHelper(IPseudonymisationClient client)
    {
        return PseudonymisationHelper.Builder()
            .PseudonymisationClient(client)
            .Build()
            ?? throw new InvalidOperationException("The builder returned no helper.");
    }

    /// <summary>
    /// A pseudonymisation client that always answers the same domain and counts the calls.
    /// </summary>
    private sealed class CountingPseudonymisationClient : IPseudonymisationClient
    {
        private readonly string _domainJson;

        public CountingPseudonymisationClient(string domainJson)
        {
            _domainJson = domainJson;
        }

        public int GetDomainCalls
        {
            get; private set;
        }

        public Task<string> GetDomain(string domainKey)
        {
            GetDomainCalls++;
            return Task.FromResult(_domainJson);
        }

        public Task<string> Identify(string domainKey, string payload) => throw new NotSupportedException();

        public Task<string> IdentifyMultiple(string domainKey, string payload) => throw new NotSupportedException();

        public Task<string> Pseudonymize(string domainKey, string payload) => throw new NotSupportedException();

        public Task<string> PseudonymizeMultiple(string domainKey, string payload) => throw new NotSupportedException();

        public Task<string> ConvertTo(string fromDomainKey, string toDomainKey, string payload) => throw new NotSupportedException();

        public Task<string> ConvertMultipleTo(string fromDomainKey, string toDomainKey, string payload) => throw new NotSupportedException();
    }
}
