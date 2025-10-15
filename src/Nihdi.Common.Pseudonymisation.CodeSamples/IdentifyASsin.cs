// <copyright file="IdentifyASsin.cs" company="Riziv-Inami">
// Copyright (c) Riziv-Inami. All rights reserved.
// </copyright>

using static Nihdi.Common.Pseudonymisation.CodeSamples.PseudonymisationHelper_Initialisation;

namespace Nihdi.Common.Pseudonymisation.CodeSamples;

public class IdentifyASsin
{
    PseudonymisationHelper _pseudonymisationHelper;

    public IdentifyASsin()
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
        var ssin =
            _pseudonymisationHelper
            .GetDomain("uhmep_v1")
            ?.Result
            ?.PseudonymInTransitFactory
            .FromSec1AndTransitInfo("...")
            .Identify().Result
            .AsString();
        // end::sync[]
    }

    public async Task Asynchronous()
    {
        // tag::async[]
        var domain =
            await _pseudonymisationHelper.GetDomain("uhmep_v1");

        if (domain == null)
        {
            throw new InvalidOperationException("domain cannot be null");
        }

        var value = await
            domain.PseudonymInTransitFactory
            .FromSec1AndTransitInfo("...")
            .Identify();

        var ssin = value.AsString();
        // end::async[]
    }
}
