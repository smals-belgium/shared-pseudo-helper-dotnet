// <copyright file="PreloadKnownDomains.cs" company="Riziv-Inami">
// Copyright (c) Riziv-Inami. All rights reserved.
// </copyright>

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using static Nihdi.Common.Pseudonymisation.CodeSamples.PseudonymisationHelper_Initialisation;

namespace Nihdi.Common.Pseudonymisation.CodeSamples;

public class PreloadKnownDomains
{
    private readonly ILogger _logger = NullLogger.Instance;

    private PseudonymisationHelper _pseudonymisationHelper;

    public PreloadKnownDomains()
    {
        _pseudonymisationHelper =
            PseudonymisationHelper
            .Builder()
            .JwksUrl(new Uri("https://api.ehealth.fgov.be/etee/v1/pubKeys/cacerts/jwks?identifier=0406798006&type=CBE&applicationIdentifier=UHMEP&use=enc"))
            .JwkSupplier(cancellationToken => Task.FromResult("..."))
            .PseudonymisationClient(new PseudonymisationClient())
            .PrivateKeySupplier(domainKey => "...")
            .Build()!;
    }

    public async Task Demo()
    {
        // tag::async[]
        try
        {
            await _pseudonymisationHelper.GetDomain("yourdomain_v1");
        }
        catch (Exception e)
        {
            _logger.LogWarning(e, "Domain preloading failed, will retry periodically");

            // Schedule asynchronous periodic retry using RefreshDomain() here
        }

        // end::async[]
    }
}
