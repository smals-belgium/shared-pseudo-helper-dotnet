// <copyright file="ResolvePseudoInTransitFromPseudoAtRest.cs" company="Riziv-Inami">
// Copyright (c) Riziv-Inami. All rights reserved.
// </copyright>

using static Nihdi.Common.Pseudonymisation.CodeSamples.PseudonymisationHelper_Initialisation;

namespace Nihdi.Common.Pseudonymisation.CodeSamples;

public class GeneratePseudoInTransitFromPseudoAtRest
{
    private PseudonymisationHelper _pseudonymisationHelper;

    public GeneratePseudoInTransitFromPseudoAtRest()
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

    public void Synchronous()
    {
        // tag::sync[]
        var domain = _pseudonymisationHelper.GetDomain("uhmep_v1");

        if (domain == null)
        {
            throw new InvalidOperationException("domain cannot be null");
        }

        var pseudonymInTransit =
            domain.Result?
            .PseudonymFactory
            .FromX("...")
            .InTransit()
            .AsString();

        // end::sync[]
    }
}
