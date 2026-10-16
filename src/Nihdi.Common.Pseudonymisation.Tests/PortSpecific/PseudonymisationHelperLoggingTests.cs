// <copyright file="PseudonymisationHelperLoggingTests.cs" company="Riziv-Inami">
// Copyright (c) Riziv-Inami. All rights reserved.
// </copyright>

using Microsoft.Extensions.Logging;

namespace Nihdi.Common.Pseudonymisation.Tests.PortSpecific;

/// <summary>
/// Tests for where <see cref="PseudonymisationHelper"/> writes its logs: through the logger
/// factory of the application, and nowhere when the application gives none.
/// <para>
/// Tests in this folder exist only in the C# port.
/// </para>
/// </summary>
[TestClass]
public class PseudonymisationHelperLoggingTests
{
    private const string _jwksUrl = "https://my-jwks.net/keys";

    /// <summary>
    /// How long a test watches the console. The console logger writes from a background thread,
    /// so a message can arrive after the helper is built.
    /// </summary>
    private static readonly TimeSpan _consoleWatchTime = TimeSpan.FromSeconds(1);

    [TestMethod]
    public void Build_WithALoggerFactory_LogsTheJwksFailureThroughIt()
    {
        var loggerFactory = new RecordingLoggerFactory();
        var keyServerFailure = new InvalidOperationException("key server is down");

        CreateHelperWithAFailingJwksSupplier(keyServerFailure, loggerFactory);

        // The failure is found by the exception the supplier threw, not by the wording of the message.
        var entries = loggerFactory.Entries.Where(e => CausesOf(e.Exception).Contains(keyServerFailure)).ToList();
        Assert.AreNotEqual(0, entries.Count, "The JWKS failure was not logged through the given logger factory.");
        foreach (var entry in entries)
        {
            Assert.AreEqual(LogLevel.Error, entry.Level);
            Assert.AreEqual(typeof(PseudonymisationHelper).FullName, entry.Category);
        }
    }

    [TestMethod]
    public void Build_WithoutALoggerFactory_WritesNothingToTheConsole()
    {
        var console = new StringWriter();
        var previousOut = Console.Out;
        var previousError = Console.Error;
        Console.SetOut(console);
        Console.SetError(console);
        try
        {
            CreateHelperWithAFailingJwksSupplier(new InvalidOperationException("key server is down"), null);
            Thread.Sleep(_consoleWatchTime);
        }
        finally
        {
            Console.SetOut(previousOut);
            Console.SetError(previousError);
        }

        Assert.AreEqual(string.Empty, console.ToString());
    }

    private static void CreateHelperWithAFailingJwksSupplier(Exception keyServerFailure, ILoggerFactory? loggerFactory)
    {
        _ = PseudonymisationHelper.Builder()
            .JwksUrl(new Uri(_jwksUrl))
            .JwkSupplier(cancellationToken => Task.FromException<string>(keyServerFailure))
            .PrivateKeySupplier(hash => throw new NotSupportedException(hash))
            .PseudonymisationClient(new StubPseudonymisationClient())
            .LoggerFactory(loggerFactory)
            .Build();
    }

    /// <summary>
    /// Lists an exception and every exception it wraps, including each one of an <see cref="AggregateException"/>.
    /// </summary>
    private static IEnumerable<Exception> CausesOf(Exception? exception)
    {
        if (exception == null)
        {
            yield break;
        }

        yield return exception;

        var wrapped = exception is AggregateException aggregate
            ? aggregate.InnerExceptions
            : (IEnumerable<Exception>)new[] { exception.InnerException! }.Where(e => e != null);

        foreach (var cause in wrapped.SelectMany(CausesOf))
        {
            yield return cause;
        }
    }

    private sealed class LogEntry
    {
        public LogEntry(string category, LogLevel level, Exception? exception)
        {
            Category = category;
            Level = level;
            Exception = exception;
        }

        public string Category { get; }

        public LogLevel Level { get; }

        public Exception? Exception { get; }
    }

    /// <summary>
    /// A logger factory that keeps every message, so a test can check what was logged.
    /// </summary>
    private sealed class RecordingLoggerFactory : ILoggerFactory
    {
        private readonly List<LogEntry> _entries = new();

        public IReadOnlyList<LogEntry> Entries
        {
            get
            {
                lock (_entries)
                {
                    return _entries.ToList();
                }
            }
        }

        public ILogger CreateLogger(string categoryName) => new RecordingLogger(this, categoryName);

        public void AddProvider(ILoggerProvider provider)
        {
        }

        public void Dispose()
        {
        }

        private void Add(LogEntry entry)
        {
            lock (_entries)
            {
                _entries.Add(entry);
            }
        }

        private sealed class RecordingLogger : ILogger
        {
            private readonly RecordingLoggerFactory _factory;
            private readonly string _category;

            public RecordingLogger(RecordingLoggerFactory factory, string category)
            {
                _factory = factory;
                _category = category;
            }

            public IDisposable? BeginScope<TState>(TState state)
                where TState : notnull => null;

            public bool IsEnabled(LogLevel logLevel) => true;

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
            {
                _factory.Add(new LogEntry(_category, logLevel, exception));
            }
        }
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
