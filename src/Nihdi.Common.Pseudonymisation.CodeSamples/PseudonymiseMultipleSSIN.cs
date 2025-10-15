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
            .JwkSupplier(() => Task.FromResult("..."))
            .PseudonymisationClient(new PseudonymisationClient())
            .PrivateKeySupplier(domainKey => "...")
            .Build()!;
    }

    public void Synchronous()
    {
        // tag::sync[]
        string[] values = new[] { "00000000097", "00000000196", "00000000295" };

        IValueFactory valueFactory = _pseudonymisationHelper.GetDomain("uhmep_v1")?.Result?.ValueFactory
            ?? throw new InvalidOperationException($"{nameof(valueFactory)} cannot be null");
        IMultiplePseudonymInTransit multiplePseudonymInTransit =
            valueFactory
            .Multiple(values.Select(value => valueFactory.From(value)).ToList())
            .Pseudonymize()
            .Result;

        for (int i = 0; i < multiplePseudonymInTransit.Size(); i++)
        {
            try
            {
                IPseudonymInTransit pseudonymInTransit = multiplePseudonymInTransit[i];
                // Add your implementation here
            }
            catch (EHealthProblemException e)
            {
                EHealthProblem problem = e.Problem;
                // Add your implementation here
            }
        }

        // end::sync[]
    }
}
