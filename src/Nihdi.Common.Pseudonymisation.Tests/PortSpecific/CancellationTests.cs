// <copyright file="CancellationTests.cs" company="Riziv-Inami">
// Copyright (c) Riziv-Inami. All rights reserved.
// </copyright>

namespace Nihdi.Common.Pseudonymisation.Tests.PortSpecific;

/// <summary>
/// Tests for the cancellation token of the asynchronous operations, and for the timeout of the
/// domain load.
/// <para>
/// Two rules are pinned here. A call that belongs to one caller (<c>Pseudonymize</c>,
/// <c>Identify</c>, <c>ConvertTo</c>) stops when that caller cancels, and the pseudonymisation client
/// is told to stop; those calls have no time limit of their own. A load shared by several callers
/// (a domain) is never cancelled by one of them: a caller that cancels only stops waiting, and the
/// load is bounded by the timeout alone.
/// </para>
/// <para>
/// Tests in this folder exist only in the C# port.
/// </para>
/// </summary>
[TestClass]
public class CancellationTests
{
    private const string _requestedKey = "uhmep_v1";

    private const string _domainKey = "test";

    private const string _targetDomainKey = "target";

    // Coordinates of a valid P-521 point, taken from the existing tests.
    private const string _x =
        "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAMDEy" +
        "MzQ1Njc4OTEwDAAAAAAAAAAB";

    private const string _y =
        "ABxCeSYmGgumIwOuFyKmK1l8bilTjL+NK" +
        "Elpegi8mcsn3QB6grG5PHpgD8XCTPTGhDrT6HsGuEkEOVwZ23maSliV";

    private const string _pseudonymInTransitRaw =
        "BAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAADAx" +
        "MjM0NTY3ODkxMAwAAAAAAAAAAQHjvYbZ2eX0Wdz8UejdWdSmg5HWrHNActe2loX3" +
        "Q2Y02CL_hX1ORsOFn_A6PbMLOXvFLBeE-Ue2-8aj5iSGZbWnag" +
        ":" +
        "eyJhbGciOiJkaXIiLCJlbmMiOiJBMjU2R0NNIiwia2lkIjoiMjAyMi0xMiIsImF1" +
        "ZCI6Imh0dHBzOi8vYXBpLWludC5laGVhbHRoLmZnb3YuYmUvcHNldWRvL3YxL2Rv" +
        "bWFpbnMvdWhtZXBfdjEifQ..osrl3KS4nkheJvcJ.pXN4Asfg8RGtsoV529YoFRW" +
        "P_XSXUViR-wxuvwYTvN9fMSDksq7qZMmmqDstyGyOidHKHrVvtqB0PFrek71P4K8" +
        "Rp0rDuvAc6RC2cbdwV08Ksw6t3Wf72H8c8QDKGKmYb84z_oH8TMnY26cAm0nC2Hb" +
        "18H-SXTh8xFXe3DK8y06wx4rAAXFZGsXayloJ6oweux_tvKQ4NSWi3gzhjVi0g-q" +
        "WR9TYZNj9NNyU9eeSDk9UsXJ8cugpvStu6oFOCbW4520fl6h5oaJ7Rye3.IEd7uL" +
        "w-ICHAwqIzfrjOFw";

    private const string _domainJson =
        "{\"domain\":\"uhmep_v1\"," +
        "\"audience\":\"https://api-acpt.ehealth.fgov.be/pseudo/v1/domains/uhmep_v1\"," +
        "\"bufferSize\":8,\"crv\":\"P-521\",\"timeToLiveInTransit\":\"PT10M\"," +
        "\"jku\":[],\"secretKeys\":[]}";

    /// <summary>
    /// How long a test waits before it decides that something is stuck. Time moves by the fake
    /// clock and by explicit cancellation, so this only stops a broken helper from hanging the run.
    /// </summary>
    private static readonly TimeSpan _hangGuard = TimeSpan.FromSeconds(10);

    /// <summary>
    /// The timeout given to the helper in the test that waits for it. It is real time, so it is kept short.
    /// </summary>
    private static readonly TimeSpan _shortTimeout = TimeSpan.FromMilliseconds(200);

    /// <summary>
    /// The longest a test waits for the short timeout to be reached. It is well under the five seconds
    /// that the helper would use if the configured timeout were ignored, and about twenty times the
    /// timeout itself, so that a loaded machine does not fail the test.
    /// </summary>
    private static readonly TimeSpan _shortTimeoutGuard = TimeSpan.FromSeconds(4);

    /// <summary>
    /// Every operation that belongs to one caller, on each of its paths: the single endpoint and,
    /// for the collections, the single-element shortcut and the batch endpoint.
    /// </summary>
    /// <returns>The name of each operation, then a way to start it.</returns>
    public static IEnumerable<object[]> OperationsOfOneCaller()
    {
        yield return Operation(
            "Value.Pseudonymize",
            (domain, toDomain, token) => domain.ValueFactory.From("012345678910").Pseudonymize(token));

        yield return Operation(
            "Pseudonym.ConvertTo",
            (domain, toDomain, token) => domain.PseudonymFactory.FromXy(_x, _y).ConvertTo(toDomain, token));

        yield return Operation(
            "PseudonymInTransit.Identify",
            (domain, toDomain, token) => domain.PseudonymInTransitFactory.FromSec1AndTransitInfo(_pseudonymInTransitRaw).Identify(token));

        yield return Operation(
            "PseudonymInTransit.ConvertTo",
            (domain, toDomain, token) => domain.PseudonymInTransitFactory.FromSec1AndTransitInfo(_pseudonymInTransitRaw).ConvertTo(toDomain, token));

        yield return Operation(
            "MultipleValue.Pseudonymize, one element",
            (domain, toDomain, token) => new MultipleValue(domain, Values(domain, 1)).Pseudonymize(token));

        yield return Operation(
            "MultipleValue.Pseudonymize, two elements",
            (domain, toDomain, token) => new MultipleValue(domain, Values(domain, 2)).Pseudonymize(token));

        yield return Operation(
            "MultiplePseudonym.ConvertTo, one element",
            (domain, toDomain, token) => ((IMultiplePseudonym)new MultiplePseudonym(domain, Pseudonyms(domain, 1))).ConvertTo(toDomain, token));

        yield return Operation(
            "MultiplePseudonym.ConvertTo, two elements",
            (domain, toDomain, token) => ((IMultiplePseudonym)new MultiplePseudonym(domain, Pseudonyms(domain, 2))).ConvertTo(toDomain, token));

        yield return Operation(
            "MultiplePseudonymInTransit.ConvertTo, one element",
            (domain, toDomain, token) => ((IMultiplePseudonymInTransit)new MultiplePseudonymInTransit(domain, PseudonymsInTransit(domain, 1))).ConvertTo(toDomain, token)!);

        yield return Operation(
            "MultiplePseudonymInTransit.ConvertTo, two elements",
            (domain, toDomain, token) => ((IMultiplePseudonymInTransit)new MultiplePseudonymInTransit(domain, PseudonymsInTransit(domain, 2))).ConvertTo(toDomain, token)!);

        yield return Operation(
            "MultiplePseudonymInTransit.Identify, one element",
            (domain, toDomain, token) => new MultiplePseudonymInTransit(domain, PseudonymsInTransit(domain, 1)).Identify(token));

        yield return Operation(
            "MultiplePseudonymInTransit.Identify, two elements",
            (domain, toDomain, token) => new MultiplePseudonymInTransit(domain, PseudonymsInTransit(domain, 2)).Identify(token));
    }

    [TestMethod]
    [DynamicData(nameof(OperationsOfOneCaller), DynamicDataSourceType.Method)]
    public void Operation_WhenTheCallerCancels_StopsAndTellsTheClient(
        string name,
        Func<Domain, Domain, CancellationToken, Task> start)
    {
        var client = new TokenObservingClient();
        using var caller = new CancellationTokenSource();

        var operation = start(CreateDomain(_domainKey, client), CreateDomain(_targetDomainKey, client), caller.Token);

        Assert.AreEqual(1, client.Tokens.Count, $"{name}: expected exactly one call to the client.");
        Assert.IsFalse(client.Tokens[0].IsCancellationRequested, $"{name}: the client was told to stop before anyone cancelled.");

        caller.Cancel();

        AssertEndsWith<OperationCanceledException>(operation, name);
        Assert.IsTrue(client.Tokens[0].IsCancellationRequested, $"{name}: the client was never told to stop.");
    }

    [TestMethod]
    [DynamicData(nameof(OperationsOfOneCaller), DynamicDataSourceType.Method)]
    public void Operation_WhenTheTokenIsAlreadyCancelled_TheClientKnowsFromTheStart(
        string name,
        Func<Domain, Domain, CancellationToken, Task> start)
    {
        var client = new TokenObservingClient();

        var operation = start(CreateDomain(_domainKey, client), CreateDomain(_targetDomainKey, client), new CancellationToken(true));

        AssertEndsWith<OperationCanceledException>(operation, name);
        Assert.IsTrue(
            client.Tokens.All(token => token.IsCancellationRequested),
            $"{name}: the client started a request that nobody wants any more.");
    }

    [TestMethod]
    [DynamicData(nameof(OperationsOfOneCaller), DynamicDataSourceType.Method)]
    public void Operation_WhenTheClientIgnoresTheToken_TheCallerStillStopsWaiting(
        string name,
        Func<Domain, Domain, CancellationToken, Task> start)
    {
        var client = new TokenObservingClient(honoursTheToken: false);
        using var caller = new CancellationTokenSource();

        var operation = start(CreateDomain(_domainKey, client), CreateDomain(_targetDomainKey, client), caller.Token);

        caller.Cancel();

        // The request of such a client keeps running: only the wait of the caller ends.
        AssertEndsWith<OperationCanceledException>(operation, name);
        Assert.IsFalse(client.Answer.Task.IsCompleted, $"{name}: the client ended its request although it ignores the token.");
    }

    [TestMethod]
    public void GetDomain_WhenOneCallerCancels_DoesNotCancelTheLoadOfTheOthers()
    {
        var client = new TokenObservingClient();
        var helper = CreateHelper(client);
        using var firstCaller = new CancellationTokenSource();

        var first = helper.GetDomain(_requestedKey, firstCaller.Token);
        var second = helper.GetDomain(_requestedKey);

        firstCaller.Cancel();

        AssertEndsWith<OperationCanceledException>(first, "the caller that cancelled");
        Assert.IsFalse(second.IsCompleted, "The load ended with the caller that started it.");
        Assert.IsFalse(client.Tokens[0].IsCancellationRequested, "A caller told eHealth to stop for everybody.");

        client.Answer.SetResult(_domainJson);

        // The other caller gets its domain, and so does the next one: no cancelled task was cached.
        Assert.AreEqual(_requestedKey, second.Result.Key);
        Assert.AreSame(second.Result, helper.GetDomain(_requestedKey).Result);
        Assert.AreEqual(1, client.Tokens.Count);
    }

    [TestMethod]
    public void RefreshDomain_WhenOneCallerCancels_DoesNotCancelTheLoadOfTheOthers()
    {
        var client = new TokenObservingClient();
        var helper = CreateHelper(client);
        using var firstCaller = new CancellationTokenSource();

        var first = helper.RefreshDomain(_requestedKey, firstCaller.Token);
        var second = helper.RefreshDomain(_requestedKey);

        firstCaller.Cancel();

        AssertEndsWith<OperationCanceledException>(first, "the caller that cancelled");
        Assert.IsFalse(client.Tokens[0].IsCancellationRequested, "A caller told eHealth to stop for everybody.");

        client.Answer.SetResult(_domainJson);

        Assert.AreEqual(_requestedKey, second.Result.Key);
        Assert.AreEqual(1, client.Tokens.Count);
    }

    [TestMethod]
    public void GetDomain_WhenTheTokenIsAlreadyCancelled_StillLoadsTheDomainForTheOthers()
    {
        var client = new TokenObservingClient();
        var helper = CreateHelper(client);

        var first = helper.GetDomain(_requestedKey, new CancellationToken(true));
        var second = helper.GetDomain(_requestedKey);

        AssertEndsWith<OperationCanceledException>(first, "the caller that cancelled");

        client.Answer.SetResult(_domainJson);

        Assert.AreEqual(_requestedKey, second.Result.Key);
    }

    [TestMethod]
    public void GetDomain_WhenTheTimeoutIsReached_FailsAndTellsTheClient()
    {
        var client = new TokenObservingClient();
        var helper = CreateHelper(client, _shortTimeout);

        var load = helper.GetDomain(_requestedKey);

        // Ending within the guard, which is under the default timeout, shows that the configured timeout is used.
        AssertEndsWith<TimeoutException>(load, "the domain load", _shortTimeoutGuard);
        Assert.IsTrue(client.Tokens[0].IsCancellationRequested, "eHealth was abandoned but never told to stop.");
    }

    private static object[] Operation(string name, Func<Domain, Domain, CancellationToken, Task> start)
    {
        return new object[] { name, start };
    }

    private static List<object> Values(Domain domain, int count)
    {
        return Enumerable.Range(0, count).Select(unused => (object)domain.ValueFactory.From("012345678910")).ToList();
    }

    private static List<IPseudonym> Pseudonyms(Domain domain, int count)
    {
        return Enumerable.Range(0, count).Select(unused => (IPseudonym)domain.PseudonymFactory.FromXy(_x, _y)).ToList();
    }

    private static List<object> PseudonymsInTransit(Domain domain, int count)
    {
        return Enumerable.Range(0, count)
            .Select(unused => (object)domain.PseudonymInTransitFactory.FromSec1AndTransitInfo(_pseudonymInTransitRaw))
            .ToList();
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
    /// Waits for the task, which must end with the given exception. Awaiting would give that
    /// exception as it is, which is what a caller catches; <c>Wait</c> wraps it, so it is unwrapped here.
    /// </summary>
    /// <typeparam name="TException">The exception the task must end with.</typeparam>
    /// <param name="task">The task to wait for.</param>
    /// <param name="what">What the task is, for the failure message.</param>
    /// <param name="guard">How long to wait; the general guard when not given.</param>
    private static void AssertEndsWith<TException>(Task task, string what, TimeSpan? guard = null)
        where TException : Exception
    {
        try
        {
            Assert.IsTrue(task.Wait(guard ?? _hangGuard), $"{what}: still running.");
        }
        catch (AggregateException ex)
        {
            var inner = ex.Flatten().InnerExceptions.Single();
            Assert.IsInstanceOfType(inner, typeof(TException), $"{what}: ended with {inner}.");

            // What an `await` rethrows is the first inner exception: it must be the right one itself.
            // A cancelled task carries no exception object, and `await` rethrows its cancellation.
            if (task.IsFaulted)
            {
                Assert.IsInstanceOfType(task.Exception!.InnerException, typeof(TException), $"{what}: the exception is wrapped.");
            }

            return;
        }

        Assert.Fail($"{what}: ended without {typeof(TException).Name}.");
    }

    /// <summary>
    /// A pseudonymisation client whose calls are pending until the test answers. It gives up the
    /// call when it is told to stop, as a real client does when its request is cancelled, unless
    /// it is built to ignore the token, as a careless one does.
    /// </summary>
    private sealed class TokenObservingClient : IPseudonymisationClient
    {
        private readonly bool _honoursTheToken;

        public TokenObservingClient(bool honoursTheToken = true)
        {
            _honoursTheToken = honoursTheToken;
        }

        public List<CancellationToken> Tokens { get; } = new List<CancellationToken>();

        public TaskCompletionSource<string> Answer { get; } =
            new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);

        public Task<string> GetDomain(string domainKey, CancellationToken cancellationToken) => Start(cancellationToken);

        public Task<string> Identify(string domainKey, string payload, CancellationToken cancellationToken) => Start(cancellationToken);

        public Task<string> IdentifyMultiple(string domainKey, string payload, CancellationToken cancellationToken) => Start(cancellationToken);

        public Task<string> Pseudonymize(string domainKey, string payload, CancellationToken cancellationToken) => Start(cancellationToken);

        public Task<string> PseudonymizeMultiple(string domainKey, string payload, CancellationToken cancellationToken) => Start(cancellationToken);

        public Task<string> ConvertTo(string fromDomainKey, string toDomainKey, string payload, CancellationToken cancellationToken) => Start(cancellationToken);

        public Task<string> ConvertMultipleTo(string fromDomainKey, string toDomainKey, string payload, CancellationToken cancellationToken) => Start(cancellationToken);

        private Task<string> Start(CancellationToken cancellationToken)
        {
            Tokens.Add(cancellationToken);
            if (_honoursTheToken)
            {
                cancellationToken.Register(() => Answer.TrySetCanceled(cancellationToken));
            }

            return Answer.Task;
        }
    }
}
