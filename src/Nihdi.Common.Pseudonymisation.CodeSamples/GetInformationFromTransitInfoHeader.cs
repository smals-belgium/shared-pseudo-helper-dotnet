// <copyright file="GetInformationFromTransitInfoHeader.cs" company="Riziv-Inami">
// Copyright (c) Riziv-Inami. All rights reserved.
// </copyright>

using static Nihdi.Common.Pseudonymisation.CodeSamples.PseudonymisationHelper_Initialisation;

namespace Nihdi.Common.Pseudonymisation.CodeSamples;

public class GetInformationFromTransitInfoHeader
{
    PseudonymisationHelper _pseudonymisationHelper;

    public GetInformationFromTransitInfoHeader()
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
        var pseudonymInTransit =
            _pseudonymisationHelper
                .GetDomain("uhmep_v1")
                .Result
                ?.PseudonymInTransitFactory
                .FromSec1AndTransitInfo("...")
                .GetTransitInfo()
                .Header()["exp"];
        // end::sync[]
    }
}
