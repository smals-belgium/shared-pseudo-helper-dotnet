// <copyright file="PseudonymisationHelperJwksTests.cs" company="Riziv-Inami">
// Copyright (c) Riziv-Inami. All rights reserved.
// </copyright>

namespace Nihdi.Common.Pseudonymisation.Tests.PortSpecific;

/// <summary>
/// Tests for the way <see cref="PseudonymisationHelper"/> fetches its JWK set.
/// <para>
/// Building a helper must not fail because the key server is down, and must not wait for it
/// forever: an application that starts while the key server is down still has to start.
/// </para>
/// <para>
/// Tests in this folder exist only in the C# port.
/// </para>
/// </summary>
[TestClass]
public class PseudonymisationHelperJwksTests
{
    private const string _jwksUrl = "https://my-jwks.net/keys";

    /// <summary>
    /// How long a test waits before it decides that building the helper is stuck.
    /// The helper gives up on the key server after five seconds, so ten is a safe margin
    /// while still failing if the wait has no bound at all.
    /// </summary>
    private static readonly TimeSpan _buildTimeout = TimeSpan.FromSeconds(10);

    [TestMethod]
    public void Build_WhenTheJwksSupplierFails_DoesNotThrow()
    {
        var helper = CreateHelper(() => Task.FromException<string>(new InvalidOperationException("key server is down")));

        Assert.IsNotNull(helper);
    }

    [TestMethod]
    public void Build_WhenTheJwksSupplierNeverAnswers_GivesUpAndReturns()
    {
        // This supplier never answers. Only the five-second give-up lets the build return.
        var neverAnswers = new TaskCompletionSource<string>();

        var build = Task.Run(() => CreateHelper(() => neverAnswers.Task));
        var finishedInTime = build.Wait(_buildTimeout);

        // Let the supplier go, so the test does not leave a task pending behind it.
        neverAnswers.TrySetResult("{\"keys\":[]}");

        Assert.IsTrue(finishedInTime, "Building the helper waited for the JWKS supplier with no time limit.");
    }

    [TestMethod]
    public void FlagJwksForRefresh_WithoutASupplier_DoesNothing()
    {
        var helper = PseudonymisationHelper.Builder()
            .PseudonymisationClient(new StubPseudonymisationClient())
            .Build()
            ?? throw new InvalidOperationException("The builder returned no helper.");

        helper.FlagJwksForRefresh();
    }

    private static PseudonymisationHelper CreateHelper(Func<Task<string>> jwksSupplier)
    {
        return PseudonymisationHelper.Builder()
            .JwksUrl(new Uri(_jwksUrl))
            .JwkSupplier(jwksSupplier)
            .PrivateKeySupplier(hash => throw new NotSupportedException(hash))
            .PseudonymisationClient(new StubPseudonymisationClient())
            .Build()
            ?? throw new InvalidOperationException("The builder returned no helper.");
    }

    /// <summary>
    /// A pseudonymisation client that is never called: these tests only build the helper.
    /// </summary>
    private sealed class StubPseudonymisationClient : IPseudonymisationClient
    {
        public Task<string> GetDomain(string domainKey) => throw new NotSupportedException();

        public Task<string> Identify(string domainKey, string payload) => throw new NotSupportedException();

        public Task<string> IdentifyMultiple(string domainKey, string payload) => throw new NotSupportedException();

        public Task<string> Pseudonymize(string domainKey, string payload) => throw new NotSupportedException();

        public Task<string> PseudonymizeMultiple(string domainKey, string payload) => throw new NotSupportedException();

        public Task<string> ConvertTo(string fromDomainKey, string toDomainKey, string payload) => throw new NotSupportedException();

        public Task<string> ConvertMultipleTo(string fromDomainKey, string toDomainKey, string payload) => throw new NotSupportedException();
    }
}
