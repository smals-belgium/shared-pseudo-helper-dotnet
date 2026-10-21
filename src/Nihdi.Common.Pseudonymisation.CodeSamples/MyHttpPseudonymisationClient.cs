// <copyright file="MyHttpPseudonymisationClient.cs" company="Riziv-Inami">
// Copyright (c) Riziv-Inami. All rights reserved.
// </copyright>

namespace Nihdi.Common.Pseudonymisation.CodeSamples;

public class MyHttpPseudonymisationClient : IPseudonymisationClient
{
    private readonly HttpClient _httpClient;

    public MyHttpPseudonymisationClient(HttpClient httpClient)
    {
        // Configured by your application: base address, authentication, ...
        _httpClient = httpClient;
    }

    // tag::gethttp[]
    public async Task<string> GetDomain(string domainKey, CancellationToken cancellationToken)
    {
        // Check the eHealth Pseudonymisation documentation for the current path.
        using var response = await _httpClient.GetAsync($"pseudo/v1/domains/{domainKey}", cancellationToken);

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }

    // end::gethttp[]

    public Task<string> ConvertTo(string fromDomainKey, string toDomainKey, string payload, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<string> ConvertMultipleTo(string fromDomainKey, string toDomainKey, string payload, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<string> Identify(string domainKey, string payload, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<string> IdentifyMultiple(string domainKey, string payload, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<string> Pseudonymize(string domainKey, string payload, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<string> PseudonymizeMultiple(string domainKey, string payload, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
