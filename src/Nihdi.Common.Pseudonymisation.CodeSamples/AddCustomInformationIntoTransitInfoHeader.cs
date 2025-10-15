// <copyright file="AddCustomInformationIntoTransitInfoHeader.cs" company="Riziv-Inami">
// Copyright (c) Riziv-Inami. All rights reserved.
// </copyright>

using static Nihdi.Common.Pseudonymisation.CodeSamples.PseudonymisationHelper_Initialisation;

namespace Nihdi.Common.Pseudonymisation.CodeSamples;

public class AddCustomInformationIntoTransitInfoHeader
{
    private readonly PseudonymisationHelper _pseudonymisationHelper;

    public AddCustomInformationIntoTransitInfoHeader()
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
        var transitInfoCustomizer = new HeaderTransitInfoCustomizer();

        var pseudonymInTransit =
            _pseudonymisationHelper
                .GetDomain("uhmep_v1")
                .Result
                ?.PseudonymFactory
                .FromX("...")
                .InTransit(transitInfoCustomizer);
        // end::sync[]
    }

    // tag::customizer[]
    internal class HeaderTransitInfoCustomizer : ITransitInfoCustomizer
    {
        private object _jwsCompact = "...";

        public Dictionary<string, object> Header => throw new NotImplementedException();

        public Dictionary<string, object> Payload
            => new Dictionary<string, object>() { { "signature", _jwsCompact } };
    }

    // end::customizer[]
}
