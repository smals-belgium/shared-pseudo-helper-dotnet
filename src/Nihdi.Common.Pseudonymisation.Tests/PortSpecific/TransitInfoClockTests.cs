// <copyright file="TransitInfoClockTests.cs" company="Riziv-Inami">
// Copyright (c) Riziv-Inami. All rights reserved.
// </copyright>

using Microsoft.IdentityModel.Tokens;
using Nihdi.Common.Pseudonymisation.Exceptions;
using Nihdi.Common.Pseudonymisation.Jose;

namespace Nihdi.Common.Pseudonymisation.Tests.PortSpecific;

/// <summary>
/// Tests for the issue and expiry times of a transit info, which depend on the clock.
/// <para>
/// Tests in this folder exist only in the C# port.
/// </para>
/// </summary>
[TestClass]
public class TransitInfoClockTests
{
    private const string _domainKey = "test";
    private const string _audience = "https://api-acpt.ehealth.fgov.be/pseudo/v1/domains/test";
    private const string _activeKid = "2026-09";

    // The domain keeps a transit info for ten minutes, and a transit info tolerates one minute of clock skew.
    private static readonly TimeSpan _timeToLive = TimeSpan.FromMinutes(10);
    private static readonly DateTimeOffset _issuedAt = new DateTimeOffset(2026, 10, 23, 8, 0, 0, TimeSpan.Zero);

    [TestMethod]
    public void ValidatePayload_RightAfterTheTransitInfoIsIssued_DoesNotThrow()
    {
        var transitInfo = ReadOn(IssuedAt(_issuedAt), _issuedAt);

        transitInfo.ValidatePayload();
    }

    [TestMethod]
    public void ValidatePayload_JustBeforeTheExpiryOfTheTransitInfo_DoesNotThrow()
    {
        var transitInfo = ReadOn(IssuedAt(_issuedAt), _issuedAt + _timeToLive);

        transitInfo.ValidatePayload();
    }

    [TestMethod]
    public void ValidatePayload_WithinTheClockSkewAfterTheExpiry_DoesNotThrow()
    {
        var transitInfo = ReadOn(IssuedAt(_issuedAt), _issuedAt + _timeToLive + TimeSpan.FromSeconds(59));

        transitInfo.ValidatePayload();
    }

    [TestMethod]
    public void ValidatePayload_BeyondTheClockSkewAfterTheExpiry_Throws()
    {
        var transitInfo = ReadOn(IssuedAt(_issuedAt), _issuedAt + _timeToLive + TimeSpan.FromSeconds(61));

        Assert.ThrowsException<InvalidTransitInfoException>(() => transitInfo.ValidatePayload());
    }

    [TestMethod]
    public void ValidatePayload_WhenTheTransitInfoWasIssuedWithinTheClockSkewInTheFuture_DoesNotThrow()
    {
        var transitInfo = ReadOn(IssuedAt(_issuedAt + TimeSpan.FromSeconds(59)), _issuedAt);

        transitInfo.ValidatePayload();
    }

    [TestMethod]
    public void ValidatePayload_WhenTheTransitInfoWasIssuedBeyondTheClockSkewInTheFuture_Throws()
    {
        var transitInfo = ReadOn(IssuedAt(_issuedAt + TimeSpan.FromSeconds(61)), _issuedAt);

        Assert.ThrowsException<InvalidTransitInfoException>(() => transitInfo.ValidatePayload());
    }

    [TestMethod]
    public void IssuedTransitInfo_CarriesTheTimesOfTheClockItWasIssuedWith()
    {
        var payload = IssuedAt(_issuedAt).Payload()!;

        Assert.AreEqual(_issuedAt.ToUnixTimeSeconds(), Convert.ToInt64(payload["iat"]));
        Assert.AreEqual((_issuedAt + _timeToLive).ToUnixTimeSeconds(), Convert.ToInt64(payload["exp"]));
    }

    private static TransitInfo IssuedAt(DateTimeOffset moment)
    {
        return new TransitInfo(
            CreateDomain(),
            new BigInteger(1, new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 }),
            new NoCustomisation(),
            new FixedTimeProvider(moment));
    }

    private static TransitInfo ReadOn(TransitInfo issued, DateTimeOffset moment)
    {
        return new TransitInfo(CreateDomain(), issued.AsString(), new FixedTimeProvider(moment));
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
            [_activeKid] = new SymmetricSecurityKey(secretKey),
        };

        return new Domain(
            _domainKey,
            "P-521",
            EcCurveNames.GetCurveFromString("P-521"),
            _audience,
            8,
            secretKeys,
            _activeKid,
            JweContentEncryptionAlgorithm.A256GCM,
            _timeToLive,
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
