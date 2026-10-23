// <copyright file="PseudonymisationClient.cs" company="Riziv-Inami">
// Copyright (c) Riziv-Inami. All rights reserved.
// </copyright>

namespace Nihdi.Common.Pseudonymisation.Tests;

internal class PseudonymisationClient : IPseudonymisationClient
{
    private readonly TimeProvider _timeProvider;
    private readonly string _domainJson;

    public PseudonymisationClient(TimeProvider? timeProvider = null, string? domainJson = null)
    {
        this._timeProvider = timeProvider ?? TimeProvider.System;
        this._domainJson = domainJson ?? string.Empty;
    }

    public async Task<string> GetDomain(string domainKey, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(_domainJson))
        {
            throw new InvalidOperationException("domainJson cannot be null or empty");
        }

        return await Task.FromResult(_domainJson);
    }

    public Task<string> ConvertTo(string fromDomainKey, string toDomainKey, string payload, CancellationToken cancellationToken)
    {
        throw new NotSupportedException();
    }

    public Task<string> ConvertMultipleTo(string fromDomainKey, string toDomainKey, string payload, CancellationToken cancellationToken)
    {
        throw new NotSupportedException();
    }

    public Task<string> Identify(string domainKey, string payload, CancellationToken cancellationToken)
    {
        throw new NotSupportedException();
    }

    public Task<string> IdentifyMultiple(string domainKey, string payload, CancellationToken cancellationToken)
    {
        throw new NotSupportedException();
    }

    public Task<string> Pseudonymize(string domainKey, string payload, CancellationToken cancellationToken)
    {
        throw new NotSupportedException();
    }

    public Task<string> PseudonymizeMultiple(string domainKey, string payload, CancellationToken cancellationToken)
    {
        throw new NotSupportedException();
    }
}