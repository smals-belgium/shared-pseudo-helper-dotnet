// <copyright file="ResolvePseudoAtRestFromPseudoInTransit.cs" company="Riziv-Inami">
// Copyright (c) Riziv-Inami. All rights reserved.
// </copyright>

using static Nihdi.Common.Pseudonymisation.CodeSamples.PseudonymisationHelper_Initialisation;

namespace Nihdi.Common.Pseudonymisation.CodeSamples;

public class ResolvePseudoAtRestFromPseudoInTransit
{
    private readonly IPatientInfoService _patientInfoService;
    private PseudonymisationHelper _pseudonymisationHelper;

    public ResolvePseudoAtRestFromPseudoInTransit()
    {
        _patientInfoService = new PatientInfoService();

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
        var domain = _pseudonymisationHelper.GetDomain("uhmep_v1").Result;

        if (domain == null)
        {
            throw new InvalidOperationException("domain cannot be null");
        }

        var pseudonymAtRest =
            domain
            .PseudonymInTransitFactory
            .FromSec1AndTransitInfo("...")
            .AtRest()?
            .X();

        // We assume patientInfoService is the service that allows you to
        // retrieve information about a patient from your database.
        // Please note that you should only save the `x` coordinate in the
        // database.

        var patientInfo = _patientInfoService.GetByPseudonym(pseudonymAtRest);

        // end::sync[]
    }
}

internal class PatientInfoService : IPatientInfoService
{
    public object GetByPseudonym(string? pseudonymAtRest)
    {
        throw new NotImplementedException();
    }
}

internal interface IPatientInfoService
{
    object GetByPseudonym(string? pseudonymAtRest);
}
