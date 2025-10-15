// <copyright file="PseudonymisationClient.cs" company="Riziv-Inami">
// Copyright (c) Riziv-Inami. All rights reserved.
// </copyright>

namespace Nihdi.Common.Pseudonymisation.Tests;

internal class PseudonymisationClient : IPseudonymisationClient
{
    private readonly IDateTimeService _dateTimeService;
    private readonly string _domainJson;

    public PseudonymisationClient(IDateTimeService? dateTimeService = null, string? domainJson = null)
    {
        this._dateTimeService = dateTimeService ?? new DateTimeService();
        this._domainJson = domainJson ?? string.Empty;
    }

    public async Task<string> GetDomain(string domainKey)
    {
        if (string.IsNullOrEmpty(_domainJson))
        {
            throw new InvalidOperationException("domainJson cannot be null or empty");
        }

        return await Task.FromResult(_domainJson);
    }

    public Task<string> ConvertTo(string fromDomainKey, string toDomainKey, string payload)
    {
        throw new NotSupportedException();
    }

    public Task<string> ConvertMultipleTo(string fromDomainKey, string toDomainKey, string payload)
    {
        throw new NotSupportedException();
    }

    public Task<string> Identify(string domainKey, string payload)
    {
        throw new NotSupportedException();
    }

    public Task<string> IdentifyMultiple(string domainKey, string payload)
    {
        throw new NotSupportedException();
    }

    public Task<string> Pseudonymize(string domainKey, string payload)
    {
        throw new NotSupportedException();
    }

    public Task<string> PseudonymizeMultiple(string domainKey, string payload)
    {
        throw new NotSupportedException();
    }
}