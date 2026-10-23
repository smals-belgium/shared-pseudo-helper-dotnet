// <copyright file="PseudonymiseMultipleSSIN.cs" company="Riziv-Inami">
// Copyright (c) Riziv-Inami. All rights reserved.
// </copyright>

using Nihdi.Common.Pseudonymisation.Exceptions;
using Nihdi.Common.Pseudonymisation.Internal;
using static Nihdi.Common.Pseudonymisation.CodeSamples.PseudonymisationHelper_Initialisation;

namespace Nihdi.Common.Pseudonymisation.CodeSamples;

public class PseudonymiseMultipleSSIN
{
    PseudonymisationHelper _pseudonymisationHelper;

    public PseudonymiseMultipleSSIN()
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

    public async Task Demo()
    {
        // tag::async[]
        string[] values = new[] { "00000000097", "00000000196", "00000000295" };

        IDomain domain = await _pseudonymisationHelper.GetDomain("uhmep_v1");
        IValueFactory valueFactory = domain.ValueFactory;

        IMultiplePseudonymInTransit multiplePseudonymInTransit =
            await valueFactory
                .Multiple(values.Select(value => valueFactory.From(value)).ToList())
                .Pseudonymize();

        for (int i = 0; i < multiplePseudonymInTransit.Size(); i++)
        {
            try
            {
                IPseudonymInTransit pseudonymInTransit = multiplePseudonymInTransit[i];
                // Add your implementation here
            }
            catch (EHealthProblemException e)
            {
                IEHealthProblem problem = e.Problem;
                // Add your implementation here
            }
        }

        // end::async[]
    }
}
