// <copyright file="PseudonymiseASsin.cs" company="Riziv-Inami">
// Copyright (c) Riziv-Inami. All rights reserved.
// </copyright>

using static Nihdi.Common.Pseudonymisation.CodeSamples.PseudonymisationHelper_Initialisation;

namespace Nihdi.Common.Pseudonymisation.CodeSamples;

public class PseudonymiseASsin
{
    private PseudonymisationHelper _pseudonymisationHelper;

    public PseudonymiseASsin()
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

    public async Task<IPseudonymInTransit?> Demo()
    {
        // tag::async[]
        var domain = await _pseudonymisationHelper.GetDomain("uhmep_v1");

        var pseudonym = await domain.ValueFactory
                .From("00000000097")
                .Pseudonymize();
        // end::async[]
        return pseudonym;
    }
}
