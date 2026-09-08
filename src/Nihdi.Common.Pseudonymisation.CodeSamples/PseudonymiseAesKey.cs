// <copyright file="PseudonymiseAesKey.cs" company="Riziv-Inami">
// Copyright (c) Riziv-Inami. All rights reserved.
// </copyright>

using Microsoft.IdentityModel.Tokens;
using static Nihdi.Common.Pseudonymisation.CodeSamples.PseudonymisationHelper_Initialisation;

namespace Nihdi.Common.Pseudonymisation.CodeSamples;

public class PseudonymisAesString
{
    PseudonymisationHelper _pseudonymisationHelper;

    public PseudonymisAesString()
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
        byte[] secretKey = GeneraSecretKey(32);
        var base64UrlEncodedKey = Base64UrlEncoder.Encode(secretKey);

        var pseudonym =
            _pseudonymisationHelper
                ?.GetDomain("uhmep_v1")
                ?.Result
                ?.ValueFactory
                .From(base64UrlEncodedKey)
                .Pseudonymize().Result;
        // end::sync[]
    }

    public async Task Asynchronous()
    {
        // tag::async[]
        byte[] secretKey = GeneraSecretKey(32);
        var base64UrlEncodedKey = Base64UrlEncoder.Encode(secretKey);

        var domain = await _pseudonymisationHelper.GetDomain("uhmep_v1");

        if (domain != null)
        {
            var pseudonym = await domain
                .ValueFactory
                .From(base64UrlEncodedKey)
                .Pseudonymize();
        }

        // end::async[]
    }

    private byte[] GeneraSecretKey(int v)
    {
        throw new NotImplementedException();
    }
}
