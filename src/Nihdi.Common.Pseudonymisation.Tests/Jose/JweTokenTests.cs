// <copyright file="JweTokenTests.cs" company="Riziv-Inami">
// Copyright (c) Riziv-Inami. All rights reserved.
// </copyright>

namespace Nihdi.Common.Pseudonymisation.Tests.Jose;

using System;
using System.IO;
using System.Text.Json;
using Microsoft.IdentityModel.Tokens;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nihdi.Common.Pseudonymisation.Jose;

[TestClass]
public class JweTokenTests
{
    private const string _headerlessRecipient = "{\"header\":null}";

    private const string _recipientWithoutKid = "{\"header\":{\"alg\":\"RSA-OAEP-256\"}}";

    // No encrypted key on purpose: once Decrypt has picked this recipient, it stops right away
    // with an InvalidDataException. That exception is how the tests see which one was picked.
    private const string _ourRecipient = "{\"header\":{\"kid\":\"our-kid\"}}";

    [TestMethod]
    public void Deserialize_WhenARecipientHasANullHeader_LeavesItsHeaderNull()
    {
        // The tests below rely on this: `"header": null` overwrites the empty header a recipient
        // starts with, so Decrypt really can meet a recipient whose Header is null.
        var token = TokenWithRecipients(_headerlessRecipient);

        Assert.IsNull(token.Recipients[0].Header);
    }

    [TestMethod]
    public void Decrypt_WhenARecipientHasNoHeader_LooksPastItAndPicksOurs()
    {
        var token = TokenWithRecipients(_headerlessRecipient, _ourRecipient);

        Assert.ThrowsException<InvalidDataException>(() => token.Decrypt(new JsonWebKey(), "our-kid"));
    }

    [TestMethod]
    public void Decrypt_WhenNoRecipientHasOurKid_SaysWhichKidIsMissing()
    {
        var token = TokenWithRecipients(_headerlessRecipient, _recipientWithoutKid);

        var thrown = Assert.ThrowsException<InvalidOperationException>(
            () => token.Decrypt(new JsonWebKey(), "our-kid"));

        StringAssert.Contains(thrown.Message, "our-kid");
    }

    // Without the check on the kid, null would match the recipient with no header, and the empty
    // string would match the recipient whose header has no kid.
    [DataTestMethod]
    [DataRow(null)]
    [DataRow("")]
    public void Decrypt_WithoutAKid_RefusesInsteadOfPickingARecipientThatHasNone(string? kid)
    {
        var token = TokenWithRecipients(_headerlessRecipient, _recipientWithoutKid);

        var thrown = Assert.ThrowsException<ArgumentException>(() => token.Decrypt(new JsonWebKey(), kid!));

        Assert.AreEqual("myKid", thrown.ParamName);
    }

    private static JweToken TokenWithRecipients(params string[] recipientsJson)
    {
        // "e30" is an empty protected header ({}); the other fields only need to be present.
        var json = "{\"protected\":\"e30\",\"iv\":\"AAAA\",\"ciphertext\":\"AAAA\",\"tag\":\"AAAA\","
            + "\"recipients\":[" + string.Join(",", recipientsJson) + "]}";

        return JsonSerializer.Deserialize<JweToken>(json)!;
    }
}
