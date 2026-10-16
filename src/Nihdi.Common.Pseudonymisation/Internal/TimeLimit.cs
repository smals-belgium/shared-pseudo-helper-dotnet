// <copyright file="TimeLimit.cs" company="Riziv-Inami">
// Copyright (c) Riziv-Inami. All rights reserved.
// </copyright>

namespace Nihdi.Common.Pseudonymisation.Internal;

/// <summary>
/// Puts a time limit on a call to eHealth or to the JWKS supplier, and tells the call to stop
/// when the limit is reached or the caller gives up.
/// </summary>
internal static class TimeLimit
{
    /// <summary>
    /// How long the helper waits by default.
    /// </summary>
    internal static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(5);

    /// <summary>
    /// Checks a delay given by the application.
    /// </summary>
    /// <param name="timeout">The delay to check.</param>
    /// <returns>The same delay.</returns>
    internal static TimeSpan Validate(TimeSpan timeout)
    {
        if (timeout <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(timeout), timeout, "The timeout must be greater than zero.");
        }

        return timeout;
    }

    /// <summary>
    /// Starts a call and gives up waiting for it after the delay. The call is told to stop, through
    /// the token it receives. A call that ignores its token is only abandoned: it may still be
    /// running when the returned task has failed.
    /// </summary>
    /// <typeparam name="T">The type of the awaited result.</typeparam>
    /// <param name="call">Starts the call. It runs at once, so a call that throws on the spot fails the caller.</param>
    /// <param name="timeout">How long to wait before giving up.</param>
    /// <param name="timeProvider">The clock that measures the delay.</param>
    /// <returns>The result of the call. The task fails with a <see cref="TimeoutException"/> when the delay is over.</returns>
    internal static Task<T> Run<T>(
        Func<CancellationToken, Task<T>> call,
        TimeSpan timeout,
        TimeProvider timeProvider)
    {
        // Nothing cancels a token by itself when the delay is over: without this one, the request
        // would run on, for as long as the HTTP client allows. It is cancelled by Wait.
        var stopTheCall = new CancellationTokenSource();

        Task<T> pending;

        try
        {
            pending = call(stopTheCall.Token);
        }
        catch
        {
            // call() threw before returning a task, so Wait will never run to dispose the source.
            stopTheCall.Dispose();
            throw;
        }

        return Wait(pending, stopTheCall, timeout, timeProvider);
    }

    /// <summary>
    /// Waits for a task with the token of one caller, without touching the task itself.
    /// Used on a task shared by several callers: the token of one of them stops that caller
    /// from waiting, and nobody else.
    /// </summary>
    /// <typeparam name="T">The type of the awaited result.</typeparam>
    /// <param name="shared">The task shared by the callers.</param>
    /// <param name="callerToken">The token of this caller.</param>
    /// <returns>The shared task itself when the token cannot be cancelled, otherwise a task that ends with this caller's cancellation.</returns>
    internal static Task<T> WaitFor<T>(Task<T> shared, CancellationToken callerToken)
    {
        return shared.WaitAsync(Timeout.InfiniteTimeSpan, TimeProvider.System, callerToken);
    }

    private static async Task<T> Wait<T>(
        Task<T> pending,
        CancellationTokenSource stopTheCall,
        TimeSpan timeout,
        TimeProvider timeProvider)
    {
        try
        {
            return await pending.WaitAsync(timeout, timeProvider, CancellationToken.None).ConfigureAwait(false);
        }
        catch (TimeoutException)
        {
            stopTheCall.Cancel();
            throw;
        }
        finally
        {
            stopTheCall.Dispose();
        }
    }
}
