// <copyright file="TransitInfoValidationTests.cs" company="Riziv-Inami">
// Copyright (c) Riziv-Inami. All rights reserved.
// </copyright>

using System.Text;
using System.Text.Json;
using Nihdi.Common.Pseudonymisation.Exceptions;

namespace Nihdi.Common.Pseudonymisation.Tests.PortSpecific;

/// <summary>
/// Tests for the transit info header validation.
/// <para>
/// Tests in this folder exist only in the C# port.
/// </para>
/// </summary>
[TestClass]
public class TransitInfoValidationTests
{
    private const string ExpectedAudience = "https://api-acpt.ehealth.fgov.be/pseudo/v1/domains/ehealth_v1";
    private const string OtherAudience = "https://api-acpt.ehealth.fgov.be/pseudo/v1/domains/other_v1";
    private const string Kid = "b54ce3e4-e7c5-455f-808e-69a03a37a85f";

    // IV, ciphertext and tag of an existing test transit info. They are never decrypted here,
    // because the header is checked first, so they only need a valid format.
    private const string Iv = "zO-S0LyrwtQLb-x9";

    private const string Ciphertext =
        "oB87loxuJfNmQbif4hHLh2Mvot17jxeqpBfsjayqyXVKMXB8-QMZYBY1OgwmWU7" +
        "ZJKvbBU62f0I6FRZIoKMQjlPMoNNJmnc2FkaIpyi6TLAciZgdolJZwZgIN5_gdK" +
        "dURIJBFOH_MEyZCCAcK6TuYRM98aGPV2SMU06RUnqrWZa1eie93w4u";

    private const string Tag = "SNkaDxhvpQaQL0aBumRLmQ";

    [TestMethod]
    public void ValidateHeader_WhenAudienceDiffersFromTheDomain_Throws()
    {
        var domain = CreateTestDomain("test", 8, ExpectedAudience);
        var transitInfo = new TransitInfo(domain, BuildRawTransitInfo(OtherAudience));

        Assert.ThrowsException<InvalidTransitInfoException>(() => transitInfo.ValidateHeader());
    }

    [TestMethod]
    public void ValidateHeader_WhenAudienceMatchesTheDomain_DoesNotThrow()
    {
        var domain = CreateTestDomain("test", 8, ExpectedAudience);
        var transitInfo = new TransitInfo(domain, BuildRawTransitInfo(ExpectedAudience));

        transitInfo.ValidateHeader();
    }

    [TestMethod]
    public void ValidateHeader_WhenTheDomainHasNoAudience_Throws()
    {
        var domain = CreateTestDomain("test", 8, null);
        var transitInfo = new TransitInfo(domain, BuildRawTransitInfo(ExpectedAudience));

        Assert.ThrowsException<InvalidTransitInfoException>(() => transitInfo.ValidateHeader());
    }

    [TestMethod]
    public void Audience_OnAValidTransitInfo_ReturnsTheAudienceOfTheHeader()
    {
        var domain = CreateTestDomain("test", 8, ExpectedAudience);
        var transitInfo = new TransitInfo(domain, BuildRawTransitInfo(ExpectedAudience));

        Assert.AreEqual(ExpectedAudience, transitInfo.Audience());
    }

    [TestMethod]
    public void Audience_WhenTheHeaderHasNoAud_Throws()
    {
        var domain = CreateTestDomain("test", 8, ExpectedAudience);
        var transitInfo = new TransitInfo(domain, BuildRawTransitInfo(null));

        Assert.ThrowsException<InvalidTransitInfoException>(() => transitInfo.Audience());
    }

    [TestMethod]
    public void Audience_WhenTheAudOfTheHeaderIsBlank_Throws()
    {
        var domain = CreateTestDomain("test", 8, ExpectedAudience);
        var transitInfo = new TransitInfo(domain, BuildRawTransitInfo("   "));

        Assert.ThrowsException<InvalidTransitInfoException>(() => transitInfo.Audience());
    }

    /// <summary>
    /// Builds a transit info (JWE compact form) with the given audience in its header.
    /// When the audience is null, the header has no `aud` field.
    /// </summary>
    /// <param name="audience">The audience to put in the header, or null to omit it.</param>
    /// <returns>The transit info, in JWE compact form.</returns>
    private static string BuildRawTransitInfo(string? audience)
    {
        var header = new Dictionary<string, object>();
        if (audience != null)
        {
            header["aud"] = audience;
        }

        header["enc"] = "A256GCM";
        header["exp"] = 1718203288L;
        header["iat"] = 1718202688L;
        header["alg"] = "dir";
        header["kid"] = Kid;

        // The encrypted key part is empty: the `dir` algorithm has no encrypted key.
        return Base64UrlEncode(JsonSerializer.Serialize(header))
            + ".." + Iv + "." + Ciphertext + "." + Tag;
    }

    /// <summary>
    /// Encodes a string in base64url. Written by hand on purpose, so the test does not rely on
    /// <c>Base64UrlEncoder</c>, which the code under test uses to decode.
    /// </summary>
    /// <param name="value">The string to encode.</param>
    /// <returns>The base64url representation of <paramref name="value"/>.</returns>
    private static string Base64UrlEncode(string value)
    {
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(value))
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }
}
