// <copyright file="AddCustomInformationIntoTransitInfoPayload.cs" company="Riziv-Inami">
// Copyright (c) Riziv-Inami. All rights reserved.
// </copyright>

using static Nihdi.Common.Pseudonymisation.CodeSamples.PseudonymisationHelper_Initialisation;

namespace Nihdi.Common.Pseudonymisation.CodeSamples;

public class AddCustomInformationIntoTransitInfoPayload
{
    private readonly PseudonymisationHelper _pseudonymisationHelper;

    public AddCustomInformationIntoTransitInfoPayload()
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

    public async Task Demo()
    {
        // tag::async[]
        var transitInfoCustomizer = new PayloadTransitInfoCustomizer();

        var domain = await _pseudonymisationHelper.GetDomain("uhmep_v1");

        var pseudonymInTransit =
            domain
                .PseudonymFactory
                .FromX("...")
                .InTransit(transitInfoCustomizer);
        // end::async[]
    }

    // tag::customizer[]
    internal class PayloadTransitInfoCustomizer : TransitInfoCustomizerBase
    {
        public override Dictionary<string, object> Payload
            => new Dictionary<string, object>() { { "sub", "test" } };
    }

    // end::customizer[]
}