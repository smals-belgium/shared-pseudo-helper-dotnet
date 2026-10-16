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
    /// The timeout given to the helper in the test that waits for it. It is real time, so it is kept short.
    /// </summary>
    private static readonly TimeSpan _shortTimeout = TimeSpan.FromMilliseconds(200);

    /// <summary>
    /// The longest that test waits for the build to return: well under the five seconds that the helper
    /// would use if the configured timeout were ignored, and about twenty times the timeout itself.
    /// </summary>
    private static readonly TimeSpan _buildTimeout = TimeSpan.FromSeconds(4);

    [TestMethod]
    public void Build_WhenTheJwksSupplierFails_DoesNotThrow()
    {
        var helper = CreateHelper(cancellationToken => Task.FromException<string>(new InvalidOperationException("key server is down")));

        Assert.IsNotNull(helper);
    }

    [TestMethod]
    public void Build_WhenTheJwksSupplierNeverAnswers_GivesUpAndReturns()
    {
        // This supplier never answers. Only the give-up after the timeout lets the build return.
        var neverAnswers = new TaskCompletionSource<string>();
        var tokenOfTheSupplier = new TaskCompletionSource<CancellationToken>();

        var build = Task.Run(() => CreateHelper(
            cancellationToken =>
            {
                tokenOfTheSupplier.TrySetResult(cancellationToken);
                return neverAnswers.Task;
            },
            _shortTimeout));
        var finishedInTime = build.Wait(_buildTimeout);

        // Let the supplier go, so the test does not leave a task pending behind it.
        neverAnswers.TrySetResult("{\"keys\":[]}");

        Assert.IsTrue(finishedInTime, "Building the helper waited for the JWKS supplier with no time limit.");
        Assert.IsTrue(
            tokenOfTheSupplier.Task.Result.IsCancellationRequested,
            "The JWKS supplier was abandoned but never told to stop.");
    }

    [TestMethod]
    [DataRow(0)]
    [DataRow(-1)]
    public void Timeout_WhenNotGreaterThanZero_IsRefused(int milliseconds)
    {
        var builder = PseudonymisationHelper.Builder();

        Assert.ThrowsException<ArgumentOutOfRangeException>(
            () => builder.Timeout(TimeSpan.FromMilliseconds(milliseconds)));
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

    private static PseudonymisationHelper CreateHelper(
        Func<CancellationToken, Task<string>> jwksSupplier,
        TimeSpan? timeout = null)
    {
        return PseudonymisationHelper.Builder()
            .Timeout(timeout ?? TimeSpan.FromSeconds(5))
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
        public Task<string> GetDomain(string domainKey, CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<string> Identify(string domainKey, string payload, CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<string> IdentifyMultiple(string domainKey, string payload, CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<string> Pseudonymize(string domainKey, string payload, CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<string> PseudonymizeMultiple(string domainKey, string payload, CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<string> ConvertTo(string fromDomainKey, string toDomainKey, string payload, CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<string> ConvertMultipleTo(string fromDomainKey, string toDomainKey, string payload, CancellationToken cancellationToken) => throw new NotSupportedException();
    }
}
