// <copyright file="PseudonymiseAString.cs" company="Riziv-Inami">
// Copyright (c) Riziv-Inami. All rights reserved.
// </copyright>

using System.Text;
using static Nihdi.Common.Pseudonymisation.CodeSamples.PseudonymisationHelper_Initialisation;

namespace Nihdi.Common.Pseudonymisation.CodeSamples;

public class PseudonymiseAString
{
    PseudonymisationHelper _pseudonymisationHelper;

    public PseudonymiseAString()
    {
        _pseudonymisationHelper =
            PseudonymisationHelper
            .Builder()
            .JwksUrl(new Uri("https://api.ehealth.fgov.be/etee/v1/pubKeys/cacerts/jwks?identifier=0406798006&type=CBE&applicationIdentifier=UHMEP&use=enc"))
            .JwkSupplier(_ => Task.FromResult("..."))
            .PseudonymisationClient(new PseudonymisationClient())
            .PrivateKeySupplier(domainKey => "...")
            .Build()!;
    }

    public async Task DefaultEncoding()
    {
        // tag::PseudonymizeAString[]
        var domain = await _pseudonymisationHelper.GetDomain("uhmep_v1");

        var pseudonym =
            await domain
                .ValueFactory
                .From("Cédric Dupont")
                .Pseudonymize();

        // end::PseudonymizeAString[]
    }

    public async Task WithEncoding()
    {
        // tag::PseudonymizeAStringWithEncoding[]
        var domain = await _pseudonymisationHelper.GetDomain("uhmep_v1");

        var pseudonym =
            await domain
                .ValueFactory
                .From("Cédric Dupont", Encoding.GetEncoding("ISO-8859-1"))
                .Pseudonymize();
        // end::PseudonymizeAStringWithEncoding[]
    }
}
