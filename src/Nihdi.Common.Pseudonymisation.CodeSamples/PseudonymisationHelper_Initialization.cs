// <copyright file="PseudonymisationHelper_Initialization.cs" company="Riziv-Inami">
// Copyright (c) Riziv-Inami. All rights reserved.
// </copyright>

using Microsoft.Extensions.Logging;

namespace Nihdi.Common.Pseudonymisation.CodeSamples;

public class PseudonymisationHelper_Initialisation
{
    public void Demo(ILoggerFactory loggerFactory)
    {
        // tag::snippet1[]
        var helper =
            PseudonymisationHelper.Builder()
            .JwksUrl(new Uri("https://api.ehealth.fgov.be/etee/v1/pubKeys/cacerts/jwks?identifier=0406798006&type=CBE&applicationIdentifier=UHMEP&use=enc"))
            .JwkSupplier(_ => Task.FromResult("..."))
            .PseudonymisationClient(new PseudonymisationClient())
            .PrivateKeySupplier(domainKey => "...")
            .LoggerFactory(loggerFactory)
            .Timeout(TimeSpan.FromSeconds(10))
            .Build();
        // end::snippet1[]
    }

    // tag::snippet2[]
    public class PseudonymisationClient : IPseudonymisationClient
    {
        public Task<string> ConvertTo(string fromDomainKey, string toDomainKey, string payload, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<string> ConvertMultipleTo(string fromDomainKey, string toDomainKey, string payload, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetDomain(string domainKey, CancellationToken cancellationToken)
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

    // end::snippet2[]
}
