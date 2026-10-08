// <copyright file="IdentifyMultipleSsins.cs" company="Riziv-Inami">
// Copyright (c) Riziv-Inami. All rights reserved.
// </copyright>

using System.Collections.ObjectModel;
using Nihdi.Common.Pseudonymisation.Exceptions;
using static Nihdi.Common.Pseudonymisation.CodeSamples.PseudonymisationHelper_Initialisation;

namespace Nihdi.Common.Pseudonymisation.CodeSamples;

public class IdentifyMultipleSsins
{
    private PseudonymisationHelper _pseudonymisationHelper;

    public IdentifyMultipleSsins()
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

    public async Task Asynchronous()
    {
        // tag::sync[]
        var domain = await _pseudonymisationHelper.GetDomain("uhmep_v1");

        var multiplePseudonymInTransit =
            domain.ValueFactory
            .Multiple(
                new[] { "00000000097", "00000000196", "00000000295" }
                .Select(ssin => domain.ValueFactory.From(ssin))
            .ToArray())
            .Pseudonymize().Result;

        for (int i = 0; i < multiplePseudonymInTransit.Size(); i++)
        {
            try
            {
                var pseudonymInTransit = multiplePseudonymInTransit[i];
                // Add your implementation here
            }
            catch (EHealthProblemException e)
            {
                var problem = e.Problem;
                // Add your implementation here
            }
        }

        // end::sync[]
    }
}
