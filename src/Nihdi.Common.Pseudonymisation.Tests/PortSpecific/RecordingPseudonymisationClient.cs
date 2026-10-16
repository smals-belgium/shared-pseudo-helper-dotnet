// <copyright file="RecordingPseudonymisationClient.cs" company="Riziv-Inami">
// Copyright (c) Riziv-Inami. All rights reserved.
// </copyright>

namespace Nihdi.Common.Pseudonymisation.Tests.PortSpecific;

/// <summary>
/// A fake pseudonymisation client that records which endpoints are called.
/// <para>
/// Tests use it to check that a single element is sent to the single endpoint and not to the
/// `multiple` one. The return value alone cannot show this.
/// </para>
/// </summary>
internal sealed class RecordingPseudonymisationClient : IPseudonymisationClient
{
    private readonly Task<string>? _response;

    /// <summary>
    /// Initializes a new instance of the <see cref="RecordingPseudonymisationClient"/> class.
    /// </summary>
    /// <param name="response">
    /// The response returned by every endpoint. When null, every call fails, which is fine for
    /// tests that only check <see cref="Calls"/>.
    /// </param>
    public RecordingPseudonymisationClient(string? response = null)
    {
        _response = response != null ? Task.FromResult(response) : null;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RecordingPseudonymisationClient"/> class.
    /// </summary>
    /// <param name="response">
    /// The task returned by every endpoint. A test passes a task that is not completed to stand
    /// for an eHealth that has not answered yet.
    /// </param>
    public RecordingPseudonymisationClient(Task<string> response)
    {
        _response = response;
    }

    /// <summary>
    /// Gets the endpoints called so far, in order.
    /// </summary>
    public List<string> Calls { get; } = new List<string>();

    /// <summary>
    /// Gets the request bodies received so far, in order, so that a test can check what is
    /// sent to eHealth.
    /// </summary>
    public List<string> Payloads { get; } = new List<string>();

    public Task<string> GetDomain(string domainKey, CancellationToken cancellationToken) => Record(nameof(GetDomain));

    public Task<string> Identify(string domainKey, string payload, CancellationToken cancellationToken) => Record(nameof(Identify), payload);

    public Task<string> IdentifyMultiple(string domainKey, string payload, CancellationToken cancellationToken) => Record(nameof(IdentifyMultiple), payload);

    public Task<string> Pseudonymize(string domainKey, string payload, CancellationToken cancellationToken) => Record(nameof(Pseudonymize), payload);

    public Task<string> PseudonymizeMultiple(string domainKey, string payload, CancellationToken cancellationToken) => Record(nameof(PseudonymizeMultiple), payload);

    public Task<string> ConvertTo(string fromDomainKey, string toDomainKey, string payload, CancellationToken cancellationToken) => Record(nameof(ConvertTo), payload);

    public Task<string> ConvertMultipleTo(string fromDomainKey, string toDomainKey, string payload, CancellationToken cancellationToken) => Record(nameof(ConvertMultipleTo), payload);

    private Task<string> Record(string name, string? payload = null)
    {
        Calls.Add(name);
        if (payload != null)
        {
            Payloads.Add(payload);
        }

        return _response ?? Task.FromException<string>(new NotSupportedException(name));
    }
}
