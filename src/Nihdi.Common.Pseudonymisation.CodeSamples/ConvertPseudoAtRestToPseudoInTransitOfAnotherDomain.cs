// <copyright file="ConvertPseudoAtRestToPseudoInTransitOfAnotherDomain.cs" company="Riziv-Inami">
// Copyright (c) Riziv-Inami. All rights reserved.
// </copyright>

using static Nihdi.Common.Pseudonymisation.CodeSamples.PseudonymisationHelper_Initialisation;

namespace Nihdi.Common.Pseudonymisation.CodeSamples;

public class ConvertPseudoAtRestToPseudoInTransitOfAnotherDomain
{
    PseudonymisationHelper _pseudonymisationHelper;

    public ConvertPseudoAtRestToPseudoInTransitOfAnotherDomain()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
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
        var domain = _pseudonymisationHelper.GetDomain("uhmep_v1")
            ?? throw new InvalidOperationException("domain cannot be null");


        var toDomain = _pseudonymisationHelper.GetDomain("ehealth_v1").Result
            ?? throw new InvalidOperationException("toDomain cannot be null");

        var pseudonymInTransit =
            domain
                .Result?
                .PseudonymFactory.FromX("...")
                .ConvertTo(toDomain).Result;

        // end::sync[]
    }
}
