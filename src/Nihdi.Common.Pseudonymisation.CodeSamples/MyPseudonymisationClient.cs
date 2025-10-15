// <copyright file="MyPseudonymisationClient.cs" company="Riziv-Inami">
// Copyright (c) Riziv-Inami. All rights reserved.
// </copyright>

namespace Nihdi.Common.Pseudonymisation.CodeSamples;

// tag::getdomain[]
public class MyPseudonymisationClient : IPseudonymisationClient
{
    public Task<string> GetDomain(string domainKey)
    {
        if (domainKey == "ehealth_v1")
        {
            return Task.FromResult(
                "{\n" +
                "  \"audience\": \"https://api.ehealth.fgov.be/pseudo/v1/domains/ehealth_v1\",\n" +
                "  \"bufferSize\": 8,\n" +
                "  \"timeToLiveInTransit\": \"PT10M\",\n" +
                "  \"domain\": \"ehealth_v1\"\n" +
                "}");
        }

        // Add here your implementation that calls eHealth Pseudonymisation service

        throw new ArgumentException($"Could not find domain {domainKey}");
    }

    // end::getdomain[]

    public async Task RetrieveDomainDemo()
    {
        var pseudonymisationHelper = PseudonymisationHelper.Builder()
                             .JwksUrl(new Uri("https://api-acpt.ehealth.fgov.be/etee/v1/pubKeys/cacerts/jwks?identifier=0406798006&type=CBE&applicationIdentifier=UHMEP&use=enc"))
                             .JwkSupplier(null)
                             .PrivateKeySupplier(null)
                             .PseudonymisationClient(null)
                             .Build() ?? throw new InvalidOperationException();
        // tag::retrievedomain[]
        IDomain? domain = await pseudonymisationHelper.GetDomain("uhmep_v1");
        // end::retrievedomain[]
    }

    public Task<string> ConvertTo(string fromDomainKey, string toDomainKey, string payload)
    {
        throw new NotImplementedException();
    }

    public Task<string> ConvertMultipleTo(string fromDomainKey, string toDomainKey, string payload)
    {
        throw new NotImplementedException();
    }

    public Task<string> Identify(string domainKey, string payload)
    {
        throw new NotImplementedException();
    }

    public Task<string> IdentifyMultiple(string domainKey, string payload)
    {
        throw new NotImplementedException();
    }

    public Task<string> Pseudonymize(string domainKey, string payload)
    {
        throw new NotImplementedException();
    }

    public Task<string> PseudonymizeMultiple(string domainKey, string payload)
    {
        throw new NotImplementedException();
    }
}
