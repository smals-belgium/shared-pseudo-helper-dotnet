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
    private readonly string? _response;

    /// <summary>
    /// Initializes a new instance of the <see cref="RecordingPseudonymisationClient"/> class.
    /// </summary>
    /// <param name="response">
    /// The response returned by every endpoint. When null, every call fails, which is fine for
    /// tests that only check <see cref="Calls"/>.
    /// </param>
    public RecordingPseudonymisationClient(string? response = null)
    {
        _response = response;
    }

    /// <summary>
    /// Gets the endpoints called so far, in order.
    /// </summary>
    public List<string> Calls { get; } = new List<string>();

    public Task<string> GetDomain(string domainKey) => Record(nameof(GetDomain));

    public Task<string> Identify(string domainKey, string payload) => Record(nameof(Identify));

    public Task<string> IdentifyMultiple(string domainKey, string payload) => Record(nameof(IdentifyMultiple));

    public Task<string> Pseudonymize(string domainKey, string payload) => Record(nameof(Pseudonymize));

    public Task<string> PseudonymizeMultiple(string domainKey, string payload) => Record(nameof(PseudonymizeMultiple));

    public Task<string> ConvertTo(string fromDomainKey, string toDomainKey, string payload) => Record(nameof(ConvertTo));

    public Task<string> ConvertMultipleTo(string fromDomainKey, string toDomainKey, string payload) => Record(nameof(ConvertMultipleTo));

    private Task<string> Record(string name)
    {
        Calls.Add(name);
        return _response != null
            ? Task.FromResult(_response)
            : Task.FromException<string>(new NotSupportedException(name));
    }
}
