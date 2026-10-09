// <copyright file="RefreshOwnedDomains.cs" company="Riziv-Inami">
// Copyright (c) Riziv-Inami. All rights reserved.
// </copyright>

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using static Nihdi.Common.Pseudonymisation.CodeSamples.PseudonymisationHelper_Initialisation;

namespace Nihdi.Common.Pseudonymisation.CodeSamples;

public class RefreshOwnedDomains
{
    private readonly ILogger _logger = NullLogger.Instance;

    private PseudonymisationHelper _pseudonymisationHelper;

    public RefreshOwnedDomains()
    {
        _pseudonymisationHelper =
            PseudonymisationHelper
            .Builder()
            .JwksUrl(new Uri("https://api.ehealth.fgov.be/etee/v1/pubKeys/cacerts/jwks?identifier=0406798006&type=CBE&applicationIdentifier=UHMEP&use=enc"))
            .JwkSupplier(() => Task.FromResult("..."))
            .PseudonymisationClient(new PseudonymisationClient())
            .PrivateKeySupplier(domainKey => "...")
            .Build()!;
    }

    public async Task Demo()
    {
        // tag::async[]
        foreach (var domainKey in _pseudonymisationHelper.RefreshableDomains)
        {
            try
            {
                await _pseudonymisationHelper.RefreshDomain(domainKey);
            }
            catch (Exception e)
            {
                _logger.LogWarning(e, "Refresh of the domain {DomainKey} failed, will retry periodically", domainKey);

                // Schedule asynchronous periodic retry using RefreshDomain() here
            }
        }

        // end::async[]
    }
}
