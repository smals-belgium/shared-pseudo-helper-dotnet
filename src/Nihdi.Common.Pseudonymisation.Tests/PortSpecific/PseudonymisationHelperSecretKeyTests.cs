// <copyright file="PseudonymisationHelperSecretKeyTests.cs" company="Riziv-Inami">
// Copyright (c) Riziv-Inami. All rights reserved.
// </copyright>

namespace Nihdi.Common.Pseudonymisation.Tests.PortSpecific;

/// <summary>
/// Tests for what happens when the helper owns a domain but cannot get at one of its secret keys.
/// <para>
/// A domain whose keys could not be decrypted is unusable, so building it must fail loudly and
/// name the key and the domain. Staying silent hands back a domain that fails much later, far
/// from the cause.
/// </para>
/// <para>
/// Tests in this folder exist only in the C# port.
/// </para>
/// </summary>
[TestClass]
public class PseudonymisationHelperSecretKeyTests
{
    private const string _ourJku = "https://my-jwks.net/keys";
    private const string _someoneElsesJku = "https://other-jwks.net/keys";
    private const string _domainKey = "uhmep_v1";
    private const string _secretKeyKid = "the-secret-key-id";

    /// <summary>A JWKS holding no key at all: these tests never get as far as decrypting.</summary>
    private const string _emptyJwks = "{\"keys\":[]}";

    [TestMethod]
    public void RefreshDomain_WhenNoRecipientIsAddressedToUs_FailsAndNamesTheKeyAndDomain()
    {
        var recipients = "[" + RecipientAddressedTo(_someoneElsesJku) + "]";

        var failure = FailureOfRefreshDomain(DomainJson(recipients));

        StringAssert.Contains(failure, _secretKeyKid);
        StringAssert.Contains(failure, _domainKey);
    }

    [TestMethod]
    public void RefreshDomain_WhenNoRecipientIsAddressedToUs_SaysSoWithoutBuryingItUnderAGenericWrapper()
    {
        var recipients = "[" + RecipientAddressedTo(_someoneElsesJku) + "]";

        var thrown = LibraryFailureOfRefreshDomain(DomainJson(recipients));

        // The caller must learn which key failed from the message it is handed, without walking
        // the InnerException chain. The sibling test above passes either way, because it reads
        // the whole chain: this one is what tells them apart.
        StringAssert.Contains(thrown.Message, _secretKeyKid);
        StringAssert.Contains(thrown.Message, _domainKey);
    }

    private static string RecipientAddressedTo(string jku)
    {
        return "{\"encrypted_key\":\"AA\",\"header\":{\"jku\":\"" + jku + "\",\"alg\":\"RSA-OAEP-256\",\"kid\":\"their-kid\"}}";
    }

    private static string DomainJson(string recipients)
    {
        return "{\"domain\":\"" + _domainKey + "\"," +
               "\"audience\":\"https://api-acpt.ehealth.fgov.be/pseudo/v1/domains/uhmep_v1\"," +
               "\"bufferSize\":8,\"crv\":\"P-521\",\"timeToLiveInTransit\":\"PT10M\"," +
               "\"jku\":[\"" + _ourJku + "\"]," +
               "\"secretKeys\":[{\"kid\":\"" + _secretKeyKid + "\",\"active\":true,\"encoded\":{" +
               "\"protected\":\"eyJlbmMiOiJBMjU2R0NNIn0\",\"iv\":\"AAAAAAAAAAAAAAAA\"," +
               "\"ciphertext\":\"AAAA\",\"tag\":\"AAAAAAAAAAAAAAAAAAAAAA\"," +
               "\"recipients\":" + recipients + "}}]}";
    }

    /// <summary>
    /// Refreshes the domain and returns everything the failure says, wrappers included: a failed
    /// task piles several exceptions on top of the real cause, and which one ends up outermost
    /// is not what these tests are about.
    /// </summary>
    /// <param name="domainJson">The answer eHealth gives for the domain.</param>
    /// <returns>The type names and messages of the whole exception chain.</returns>
    private static string FailureOfRefreshDomain(string domainJson)
    {
        return RaisedByRefreshDomain(domainJson).ToString();
    }

    /// <summary>
    /// Refreshes the domain and returns the exception the library itself threw, with the task
    /// plumbing peeled off: `ContinueWith` and `Wait` pile `AggregateException` on top, and those
    /// layers say nothing about the failure.
    /// </summary>
    /// <param name="domainJson">The answer eHealth gives for the domain.</param>
    /// <returns>The first exception in the chain that the library, not the task machinery, threw.</returns>
    private static Exception LibraryFailureOfRefreshDomain(string domainJson)
    {
        var raised = RaisedByRefreshDomain(domainJson);

        while (raised is AggregateException aggregate)
        {
            raised = aggregate.Flatten().InnerExceptions[0];
        }

        return raised;
    }

    private static Exception RaisedByRefreshDomain(string domainJson)
    {
        var helper = PseudonymisationHelper.Builder()
            .JwksUrl(new Uri(_ourJku))
            .JwkSupplier(() => Task.FromResult(_emptyJwks))
            .PrivateKeySupplier(hash => throw new NotSupportedException(hash))
            .PseudonymisationClient(new SingleDomainPseudonymisationClient(domainJson))
            .Build()
            ?? throw new InvalidOperationException("The builder returned no helper.");

        try
        {
            helper.RefreshDomain(_domainKey).Wait();
        }
        catch (Exception raised)
        {
            return raised;
        }

        throw new AssertFailedException("Refreshing the domain was expected to fail, but it did not.");
    }

    /// <summary>
    /// A pseudonymisation client that always answers the same domain.
    /// </summary>
    private sealed class SingleDomainPseudonymisationClient : IPseudonymisationClient
    {
        private readonly string _domainJson;

        public SingleDomainPseudonymisationClient(string domainJson)
        {
            _domainJson = domainJson;
        }

        public Task<string> GetDomain(string domainKey) => Task.FromResult(_domainJson);

        public Task<string> Identify(string domainKey, string payload) => throw new NotSupportedException();

        public Task<string> IdentifyMultiple(string domainKey, string payload) => throw new NotSupportedException();

        public Task<string> Pseudonymize(string domainKey, string payload) => throw new NotSupportedException();

        public Task<string> PseudonymizeMultiple(string domainKey, string payload) => throw new NotSupportedException();

        public Task<string> ConvertTo(string fromDomainKey, string toDomainKey, string payload) => throw new NotSupportedException();

        public Task<string> ConvertMultipleTo(string fromDomainKey, string toDomainKey, string payload) => throw new NotSupportedException();
    }
}
