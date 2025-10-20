// <copyright file="IdentifyMultipleSsins.cs" company="Riziv-Inami">
// Copyright (c) Riziv-Inami. All rights reserved.
// </copyright>

using System.Collections.ObjectModel;
using Nihdi.Common.Pseudonymisation.Exceptions;
using static Nihdi.Common.Pseudonymisation.CodeSamples.PseudonymisationHelper_Initialisation;

namespace Nihdi.Common.Pseudonymisation.CodeSamples;

public class IdentifyMultipleSsinsReturnedByRestAPI
{
    private PseudonymisationHelper _pseudonymisationHelper;

    public IdentifyMultipleSsinsReturnedByRestAPI()
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
        // tag::async[]
        var domain = await _pseudonymisationHelper.GetDomain("uhmep_v1");

        if (domain == null)
        {
            throw new InvalidOperationException("domain cannot be null");
        }

        var factory = domain.PseudonymInTransitFactory;
        var pseudonymsInTransit = new Collection<IPseudonymInTransit>
        {
            factory.FromSec1AndTransitInfo("..."),
            factory.FromSec1AndTransitInfo("..."),
            factory.FromSec1AndTransitInfo("...")
        };

        var multiplePseudonymInTransit = factory.Multiple(pseudonymsInTransit);
        var multipleValue = await multiplePseudonymInTransit.Identify();

        for (int i = 0; i < multipleValue.Size(); i++)
        {
            try
            {
                var pseudonymInTransit = multipleValue[i];
                // Add your implementation here
            }
            catch (EHealthProblemException e)
            {
                var problem = e.Problem;
                // Add your implementation here
            }
        }

        // end::async[]
    }
}
