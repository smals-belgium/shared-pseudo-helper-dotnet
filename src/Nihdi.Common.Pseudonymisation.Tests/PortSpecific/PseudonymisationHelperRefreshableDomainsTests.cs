// <copyright file="PseudonymisationHelperRefreshableDomainsTests.cs" company="Riziv-Inami">
// Copyright (c) Riziv-Inami. All rights reserved.
// </copyright>

namespace Nihdi.Common.Pseudonymisation.Tests.PortSpecific;

/// <summary>
/// Tests for the set of domains that <see cref="PseudonymisationHelper"/> reports as the ones
/// the application has to refresh.
/// <para>
/// Tests in this folder exist only in the C# port.
/// </para>
/// </summary>
[TestClass]
public class PseudonymisationHelperRefreshableDomainsTests
{
    private const string _jwksUrl = "https://example.org/jwks";

    [TestMethod]
    public void RefreshableDomains_WhenADomainWeOwnIsLoaded_ListsIt()
    {
        var helper = CreateHelper(new OwnedDomainsClient(Task.CompletedTask));

        helper.GetDomain("owned_v1").Wait();

        CollectionAssert.AreEquivalent(new[] { "owned_v1" }, helper.RefreshableDomains.ToArray());
    }

    [TestMethod]
    public void RefreshableDomains_WhenManyDomainsAreLoadedAtTheSameMoment_ListsThemAll()
    {
        const int domainCount = 16;
        const int roundCount = 300;
        var keys = Enumerable.Range(0, domainCount).Select(index => $"owned_{index}_v1").ToArray();

        // Losing a domain needs two loads to end within the same instant, which does not happen
        // on every try. Hence the many rounds, each one on a new helper.
        for (var round = 0; round < roundCount; round++)
        {
            // eHealth answers nothing until the test says so: every load is then waiting at the
            // same place, and they all end together, each one on its own thread.
            var answerArrives = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var helper = CreateHelper(new OwnedDomainsClient(answerArrives.Task));

            var loads = keys.Select(key => helper.GetDomain(key)).ToArray();
            answerArrives.SetResult(true);
            Task.WaitAll(loads);

            var listed = helper.RefreshableDomains;
            Assert.AreEqual(
                domainCount,
                listed.Count,
                $"Round {round}: missing {string.Join(", ", keys.Except(listed))}.");
        }
    }

    private static PseudonymisationHelper CreateHelper(IPseudonymisationClient client)
    {
        return PseudonymisationHelper.Builder()
            .JwksUrl(new Uri(_jwksUrl))
            .JwkSupplier(cancellationToken => Task.FromResult("{\"keys\":[]}"))
            .PseudonymisationClient(client)
            .Build()
            ?? throw new InvalidOperationException("The builder returned no helper.");
    }

    /// <summary>
    /// A pseudonymisation client for which every domain asked is one we own: its answer names
    /// our JWKS URL among the owners. The domains have no secret key, so nothing is decrypted.
    /// </summary>
    private sealed class OwnedDomainsClient : IPseudonymisationClient
    {
        private readonly Task _answerArrives;

        public OwnedDomainsClient(Task answerArrives)
        {
            _answerArrives = answerArrives;
        }

        public async Task<string> GetDomain(string domainKey, CancellationToken cancellationToken)
        {
            await _answerArrives.ConfigureAwait(false);

            return "{\"domain\":\"" + domainKey + "\"," +
                "\"audience\":\"https://api-acpt.ehealth.fgov.be/pseudo/v1/domains/" + domainKey + "\"," +
                "\"bufferSize\":8,\"crv\":\"P-521\",\"timeToLiveInTransit\":\"PT10M\"," +
                "\"jku\":[\"" + _jwksUrl + "\"],\"secretKeys\":[]}";
        }

        public Task<string> Identify(string domainKey, string payload, CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<string> IdentifyMultiple(string domainKey, string payload, CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<string> Pseudonymize(string domainKey, string payload, CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<string> PseudonymizeMultiple(string domainKey, string payload, CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<string> ConvertTo(string fromDomainKey, string toDomainKey, string payload, CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<string> ConvertMultipleTo(string fromDomainKey, string toDomainKey, string payload, CancellationToken cancellationToken) => throw new NotSupportedException();
    }
}
