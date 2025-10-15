// <copyright file="JweToken.cs" company="Riziv-Inami">
// Copyright (c) Riziv-Inami. All rights reserved.
// </copyright>

namespace Nihdi.Common.Pseudonymisation.Jose;

using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.IdentityModel.Tokens;
using Nihdi.Common.Pseudonymisation.Extensions;
using Org.BouncyCastle.Crypto.Agreement;
using Org.BouncyCastle.Crypto.Parameters;

/// <summary>
/// Represents a JSON Web Encryption (JWE) token.
/// </summary>
public class JweToken
{
    /// <summary>
    /// Initializes a new instance of the <see cref="JweToken"/> class.
    /// </summary>
    /// <param name="protectedHeader">The protected header in Base64Url-encoded format.</param>
    /// <param name="iv">The Initialization Vector (IV) used in the encryption process
    /// in Base64Url-encoded format.
    /// </param>
    /// <param name="cipherText">The ciphertext of the encrypted payload in Base64Url-encoded format.</param>
    /// <param name="authenticationTag">The authentication tag used for integrity verification
    /// in Base64Url-encoded format.
    /// </param>
    /// <param name="recipients">The list of recipients for the JWE token.</param>
    [JsonConstructor]
    public JweToken(string protectedHeader, byte[] iv, byte[] cipherText, byte[] authenticationTag, List<JweRecipient> recipients)
    {
        ProtectedHeader = protectedHeader
            ?? throw new ArgumentNullException(nameof(ProtectedHeader), "Protected header is required.");

        IV = iv
            ?? throw new ArgumentNullException(nameof(IV), "IV is required.");

        Ciphertext = cipherText
            ?? throw new ArgumentNullException(nameof(Ciphertext), "Ciphertext is required.");

        AuthenticationTag = authenticationTag
             ?? throw new ArgumentNullException(nameof(AuthenticationTag), "AuthenticationTag is required.");

        Recipients = recipients
            ?? throw new ArgumentException("Recipients list must not be empty.");
    }

    /// <summary>
    /// Gets or sets the protected header in Base64Url-encoded format.
    /// </summary>
    /// <value>A base64url-encoded JSON string containing the protected header information.</value>
    [JsonPropertyName("protected")]
    public string ProtectedHeader
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the Initialization Vector (IV) used in the encryption process.
    /// </summary>
    /// <value>A base64url-encoded string representing the IV.</value>
    [JsonPropertyName("iv")]
    [JsonConverter(typeof(ByteArrayBase64Converter))]
    public byte[] IV
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the ciphertext of the encrypted payload.
    /// </summary>
    /// <value>
    /// A base64url-encoded string representing the ciphertext.
    /// </value>
    [JsonPropertyName("ciphertext")]
    [JsonConverter(typeof(ByteArrayBase64Converter))]
    public byte[] Ciphertext
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the authentication tag used for integrity verification.
    /// </summary>
    /// <value>A base64url-encoded string representing the authentication tag.</value>
    [JsonPropertyName("tag")]
    [JsonConverter(typeof(ByteArrayBase64Converter))]
    public byte[] AuthenticationTag
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the additional authenticated data (AAD) used in the encryption process.
    /// </summary>
    /// <value>A base64url-encoded string representing the AAD.</value>
    [JsonPropertyName("aad")]
    [JsonConverter(typeof(ByteArrayBase64Converter))]
    public byte[]? AAD
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the list of recipients for the JWE token.
    /// </summary>
    /// <value>A list of <see cref="JweRecipient"/> objects representing the recipients.</value>
    [JsonPropertyName("recipients")]
    public List<JweRecipient> Recipients { get; set; } = new List<JweRecipient>();

    /// <summary>
    /// Decrypts the JWE token using the provided JSON Web Key (JWK) and the specified key identifier (kid).
    /// </summary>
    /// <param name="jwk">The JSON Web Key (JWK) used for decryption.</param>
    /// <param name="myKid">The key identifier (kid) of the recipient.</param>
    /// <returns>The decrypted payload.</returns>
    public JweDecryptedPayload Decrypt(JsonWebKey jwk, string myKid)
    {
        // Find the correct recipient by "kid"
        var recipient = Recipients.FirstOrDefault(r => r.Header.Kid == myKid);

        if (recipient == null)
        {
            throw new InvalidOperationException($"Recipient with kid '{myKid}' not found.");
        }

        // Extract encrypted AES key, IV, ciphertext, tag, and protected header.
        byte[] encryptedKey = recipient?.EncryptedKey ?? throw new InvalidDataException("The recipient encrypted key cannot be null.");
        byte[] iv = IV;
        byte[] cipherText = Ciphertext;
        byte[] tag = AuthenticationTag;
        byte[] protectedHeaderBytes = Base64UrlEncoder.DecodeBytes(ProtectedHeader);

        string alg = jwk.Alg;

        // Parse the protected header JSON
        string headerJson = Encoding.UTF8.GetString(protectedHeaderBytes);
        var protectedHeader = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(headerJson)
            ?? throw new InvalidOperationException("Protected header cannot be null.");

        if (!protectedHeader.TryGetValue("enc", out JsonElement encElement) ||
            encElement.ValueKind != JsonValueKind.String ||
            encElement.GetString() != "A256GCM")
        {
            throw new InvalidOperationException("Invalid encryption algorithm. Only A256GCM is supported.");
        }

        // Extract 'alg' safely from protected header
        // If alg is present, takes precendence on the protected header
        if (protectedHeader.TryGetValue("alg", out JsonElement algElement)
            && algElement.ValueKind != JsonValueKind.String
            && algElement.GetString() != null)
        {
            alg = algElement.GetString()!;
        }

        alg = alg ?? "dir"; // If absent from both protected header and jwk, default to 'dir'

        // Example usage of `alg`
        Console.WriteLine($"Encryption Algorithm: A256GCM");
        Console.WriteLine($"Key Management Algorithm: {alg}");

        if (alg == "RSA-OAEP-256")
        {
            Console.WriteLine("Using RSA-OAEP-256 for key management.");
        }

        byte[] aesKey = alg switch
        {
            "RSA-OAEP" => DecryptWithRsa(jwk, encryptedKey, RSAEncryptionPadding.OaepSHA256),
            "RSA-OAEP-256" => DecryptWithRsa(jwk, encryptedKey, RSAEncryptionPadding.OaepSHA256),
            "RSA1_5" => DecryptWithRsa(jwk, encryptedKey, RSAEncryptionPadding.Pkcs1),
            "dir" => Base64UrlEncoder.DecodeBytes(jwk.K),
            "ECDH-ES" => DeriveKeyWithEcdhEs(jwk, protectedHeader, recipient.Header.ToDictionary()),
            "ECDH-ES+A256KW" => DeriveKeyWithEcdhEs(jwk, protectedHeader),
            _ => throw new CryptographicException($"Unsupported key encryption algorithm: {alg}. "),
        };

        var decryptionAAD = ConstructDecryptionAAd(protectedHeaderBytes, AAD);

        byte[] plaintextBytes = JweDecryption.DecryptAesGcm(decryptionAAD, aesKey, iv, cipherText, tag);
        Debug.WriteLine("Plaing text (HEX): " + BitConverter.ToString(plaintextBytes));

        var payload = Encoding.UTF8.GetString(plaintextBytes);

        // Ensure AES Key is 32 bytes (256-bit)
        if (aesKey.Length != 32)
        {
            throw new CryptographicException($"Invalid AES key size : expected 32 bytes, got {aesKey.Length} bytes.");
        }

        // Return the decrypted token
        return new JweDecryptedPayload
        {
            ProtectedHeader = ProtectedHeader,
            Payload = payload,
        };
    }

    /// <summary>
    /// Derives the AES key using ECDH-ES key agreement.
    /// </summary>
    /// <returns>A byte array representing the derived AES key.</returns>
    public override string ToString()
    {
        return JsonSerializer.Serialize(this, new JsonSerializerOptions
        {
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        });
    }

    private byte[] DecryptWithRsa(JsonWebKey jwk, byte[] encryptedKey, RSAEncryptionPadding padding)
    {
        using (RSA rsa = CreateRsa())
        {
            rsa.ImportParameters(jwk.ExtractRsaParameters());

            return rsa.Decrypt(encryptedKey, padding);
        }
    }

    private byte[] DeriveKeyWithEcdhEs(
        JsonWebKey jwk,
        Dictionary<string, JsonElement> protectedHeader,
        Dictionary<string, object>? header = null)
    {
        if (!protectedHeader.TryGetValue("epk", out JsonElement epkElement))
        {
            throw new CryptographicException("Missing 'epk' field in protected header.");
        }

        // Deserialize 'epk' (ephemerical public key)
        string epkJson = epkElement.GetRawText();
        JsonWebKey epk = JsonSerializer.Deserialize<JsonWebKey>(epkJson)
            ?? throw new CryptographicException("Invalid 'epk' format.");

        // Convert keys to BouncyCastle format
        ECPrivateKeyParameters privateKey = JweCryptoHelper.ConvertToPrivateKey(jwk);
        ECPublicKeyParameters publicKey = JweCryptoHelper.ConvertToPublicKey(epk);

        // Perform EXDH key agreement
        var agreement = new ECDHBasicAgreement();
        agreement.Init(privateKey);
        Org.BouncyCastle.Math.BigInteger sharedSecret = agreement.CalculateAgreement(publicKey);

        return JweCryptoHelper.DeriveAesKeyFromEcdh(sharedSecret, 256);
    }

    private byte[] ConstructDecryptionAAd(byte[] protectedHeaderBytes, byte[]? aad)
    {
        if (aad == null)
        {
            return Encoding.UTF8.GetBytes(Base64UrlEncoder.Encode(protectedHeaderBytes));
        }

        return Encoding.UTF8.GetBytes(
            string.Concat(
                Base64UrlEncoder.Encode(protectedHeaderBytes),
                ".",
                Base64UrlEncoder.Encode(aad)));
    }

    private RSA CreateRsa()
    {
#if NETFRAMEWORK
        return new RSACng();
#else
        return RSA.Create();
#endif
    }
}