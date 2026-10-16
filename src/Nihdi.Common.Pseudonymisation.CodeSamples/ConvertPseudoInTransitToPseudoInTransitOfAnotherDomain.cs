// <copyright file="ConvertPseudoInTransitToPseudoInTransitOfAnotherDomain.cs" company="Riziv-Inami">
// Copyright (c) Riziv-Inami. All rights reserved.
// </copyright>

using static Nihdi.Common.Pseudonymisation.CodeSamples.PseudonymisationHelper_Initialisation;

namespace Nihdi.Common.Pseudonymisation.CodeSamples;

public class ConvertPseudoInTransitToPseudoInTransitOfAnotherDomain
{
    PseudonymisationHelper _pseudonymisationHelper;

    public ConvertPseudoInTransitToPseudoInTransitOfAnotherDomain()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
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
        var domain = await _pseudonymisationHelper.GetDomain("uhmep_v1");

        var toDomain = await _pseudonymisationHelper.GetDomain("uhmep_v1");

        var pseudonymInTransit =
            await domain
                .PseudonymInTransitFactory
                .FromSec1AndTransitInfo("...")
                .ConvertTo(toDomain);

        // end::async[]
    }
}
