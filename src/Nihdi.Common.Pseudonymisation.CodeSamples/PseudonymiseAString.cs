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
            .JwkSupplier(() => Task.FromResult("..."))
            .PseudonymisationClient(new PseudonymisationClient())
            .PrivateKeySupplier(domainKey => "...")
            .Build()!;
    }

    public void DefaultEncoding()
    {
        // tag::PseudonymizeAString[]
        var pseudonym =
            _pseudonymisationHelper
                ?.GetDomain("uhmep_v1")
                ?.Result
                ?.ValueFactory
                .From("Cédric Dupont")
                .Pseudonymize().Result;

        // end::PseudonymizeAString[]
    }

    public void WithEncoding()
    {
        // tag::PseudonymizeAStringWithEncoding[]
        var pseudonym =
            _pseudonymisationHelper
                ?.GetDomain("uhmep_v1")
                ?.Result
                ?.ValueFactory
                .From("Cédric Dupont", Encoding.GetEncoding("ISO-8859-1"))
                .Pseudonymize()
                .Result;
        // end::PseudonymizeAStringWithEncoding[]
    }
}
