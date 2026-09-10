// <copyright file="TransitInfoScalarRoundTripTests.cs" company="Riziv-Inami">
// Copyright (c) Riziv-Inami. All rights reserved.
// </copyright>

using Microsoft.IdentityModel.Tokens;
using Nihdi.Common.Pseudonymisation.Jose;

namespace Nihdi.Common.Pseudonymisation.Tests.PortSpecific;

/// <summary>
/// Checks that the scalar written into a transit info is read back correctly by
/// PseudonymInTransit.AtRest and applied to the point.
/// </summary>
[TestClass]
public class TransitInfoScalarRoundTripTests
{
    private const string DomainKey = "test";
    private const string Audience = "https://api-acpt.ehealth.fgov.be/pseudo/v1/domains/test";
    private const string ActiveKid = "2026-09";

    // A valid P-521 point, taken from the existing tests.
    private const string X =
        "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAMDEy" +
        "MzQ1Njc4OTEwDAAAAAAAAAAB";

    private const string Y =
        "ABxCeSYmGgumIwOuFyKmK1l8bilTjL+NK" +
        "Elpegi8mcsn3QB6grG5PHpgD8XCTPTGhDrT6HsGuEkEOVwZ23maSliV";

    [TestMethod]
    public void AtRest_RecoversAScalarWhoseBase64UsesTheStandardAlphabet()
    {
        var scalar = ScalarWithStandardOnlyCharacters();

        // Check that the scalar contains '+' or '/', the characters that base64 and base64url
        // write differently. Note that this test passes with either decoder.
        var encoded = Convert.ToBase64String(scalar.ToByteArray());
        Assert.IsTrue(
            encoded.Contains('+') || encoded.Contains('/'),
            $"the scalar must encode to characters that base64url spells differently, got {encoded}");

        var domain = CreateDomain();
        var transitInfo = new TransitInfo(domain, scalar, new NoCustomisation());

        var scalarFromTransitInfo = new BigInteger(
            Convert.FromBase64String(transitInfo.Payload()!["scalar"].ToString()!));
        Assert.AreEqual(scalar, scalarFromTransitInfo);

        var pseudonymInTransit = domain.PseudonymInTransitFactory
            .FromXYAndTransitInfo(X, Y, transitInfo.AsString());

        // AtRest(false) skips the iat/exp date checks, which this test does not need.
        var atRest = pseudonymInTransit.AtRest(false);

        Assert.IsNotNull(atRest);
        Assert.AreEqual(ExpectedXAfterMultiplyingBy(scalar), atRest!.X());
    }

    /// <summary>
    /// Builds a scalar from alternating <c>0xFB</c> and <c>0xFF</c> bytes. When the scalar is
    /// later encoded as standard Base64, those bytes produce '+' and '/', the two characters that
    /// Base64URL spells differently.
    /// </summary>
    /// <returns>The scalar.</returns>
    private static BigInteger ScalarWithStandardOnlyCharacters()
    {
        var bytes = new byte[32];
        for (var i = 0; i < bytes.Length; i++)
        {
            bytes[i] = (byte)(i % 2 == 0 ? 0xFB : 0xFF);
        }

        // Positive sign, so the encoded value never gets an extra leading zero byte.
        return new BigInteger(1, bytes);
    }

    private static string ExpectedXAfterMultiplyingBy(BigInteger scalar)
    {
        var curve = EcCurveNames.GetCurveFromString("P-521");
        var point = curve.CreatePoint(
            new BigInteger(1, Convert.FromBase64String(X)),
            new BigInteger(1, Convert.FromBase64String(Y)));

        return Convert.ToBase64String(point.Multiply(scalar).Normalize().XCoord.GetEncoded());
    }

    private static Domain CreateDomain()
    {
        var secretKey = new byte[32];
        for (var i = 0; i < secretKey.Length; i++)
        {
            secretKey[i] = (byte)i;
        }

        var secretKeys = new Dictionary<string, SymmetricSecurityKey>
        {
            [ActiveKid] = new SymmetricSecurityKey(secretKey),
        };

        return new Domain(
            DomainKey,
            "P-521",
            EcCurveNames.GetCurveFromString("P-521"),
            Audience,
            8,
            secretKeys,
            ActiveKid,
            JweContentEncryptionAlgorithm.A256GCM,
            TimeSpan.FromMinutes(10),
            null,
            null);
    }

    /// <summary>
    /// A customizer that adds no extra header or payload fields.
    /// </summary>
    private sealed class NoCustomisation : ITransitInfoCustomizer
    {
        public Dictionary<string, object> Header { get; } = new Dictionary<string, object>();

        public Dictionary<string, object> Payload { get; } = new Dictionary<string, object>();
    }
}
