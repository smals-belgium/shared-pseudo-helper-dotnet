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
        // The tests below rely on this: a recipient can come with a null `"header"`, so Decrypt
        // really can meet a recipient whose Header is null.
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

    [TestMethod]
    public void Deserialize_WhenARecipientHasNoHeaderField_LeavesItsHeaderNull()
    {
        var token = TokenWithRecipients("{}");

        Assert.IsNull(token.Recipients[0].Header);
    }

    [TestMethod]
    public void Deserialize_WhenARecipientHeaderHasNoKid_LeavesTheKidNull()
    {
        var token = TokenWithRecipients(_recipientWithoutKid);

        Assert.IsNull(token.Recipients[0].Header!.Kid);
    }

    // An absent field must stay absent on the wire, as Nimbus does, not turn into "".
    [TestMethod]
    public void ToString_DoesNotWriteTheRecipientFieldsThatAreAbsent()
    {
        var token = TokenWithRecipients(_ourRecipient, "{}");

        var written = token.ToString();

        Assert.IsFalse(written.Contains("\"alg\""), written);
        Assert.IsFalse(written.Contains("\"jku\""), written);
        Assert.IsFalse(written.Contains("\"header\": null"), written);
        Assert.IsFalse(written.Contains("\"\""), written);
    }

    // Nimbus rejects a null recipient (a ParseException, or a NullPointerException when it sits
    // next to a real one). Both cases are refused here the same way, with a message that says so.
    [TestMethod]
    public void Deserialize_WhenARecipientIsNull_RefusesTheToken()
    {
        var thrown = Assert.ThrowsException<ArgumentException>(() => TokenWithRecipients("null"));

        StringAssert.Contains(thrown.Message, "null");
    }

    [TestMethod]
    public void Deserialize_WhenANullRecipientSitsNextToARealOne_RefusesTheToken()
    {
        Assert.ThrowsException<ArgumentException>(() => TokenWithRecipients("null", _ourRecipient));
    }

    [TestMethod]
    public void Deserialize_WhenRecipientsIsEmpty_RefusesTheToken()
    {
        var thrown = Assert.ThrowsException<ArgumentException>(() => TokenWithRecipients());

        StringAssert.Contains(thrown.Message, "recipients");
    }

    [TestMethod]
    public void Deserialize_WhenRecipientsIsAbsent_RefusesTheToken()
    {
        var thrown = Assert.ThrowsException<ArgumentException>(() => TokenFromJson(JsonWith(string.Empty)));

        StringAssert.Contains(thrown.Message, "recipients");
    }

    private static JweToken TokenWithRecipients(params string[] recipientsJson)
    {
        return TokenFromJson(JsonWith("\"recipients\":[" + string.Join(",", recipientsJson) + "]"));
    }

    // The members every token needs, plus whatever else the test adds. "e30" is an empty
    // protected header ({}); the other fields only need to be present.
    private static string JsonWith(string extraMembers)
    {
        var members = "{\"protected\":\"e30\",\"iv\":\"AAAA\",\"ciphertext\":\"AAAA\",\"tag\":\"AAAA\"";

        return extraMembers.Length == 0 ? members + "}" : members + "," + extraMembers + "}";
    }

    private static JweToken TokenFromJson(string json)
    {
        return JsonSerializer.Deserialize<JweToken>(json)!;
    }
}
