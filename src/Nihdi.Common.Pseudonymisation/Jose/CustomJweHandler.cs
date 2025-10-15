// <copyright file="CustomJweHandler.cs" company="Riziv-Inami">
// Copyright (c) Riziv-Inami. All rights reserved.
// </copyright>

namespace Nihdi.Common.Pseudonymisation.Jose;

using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.IdentityModel.Tokens;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Parameters;

/// <summary>
/// A custom JWE handler that supports creating and decrypting JWE tokens with custom headers.
/// </summary>
public class CustomJweHandler
{
    /// <summary>
    /// Creates a JWE token using EncryptingCredentials and custom header parameters.
    /// </summary>
    /// <param name="payload">The payload to encrypt.</param>
    /// <param name="encryptingCredentials">The EncryptingCredentials object.</param>
    /// <param name="customParams">Optional custom header parameters.</param>
    /// <returns>The JWE token in compact serialization.</returns>
    public string CreateToken(string payload, EncryptingCredentials encryptingCredentials, Dictionary<string, object>? customParams = null)
    {
        if (encryptingCredentials == null)
        {
            throw new ArgumentNullException(nameof(encryptingCredentials));
        }

        // Validate algorithm
        if (!string.Equals(encryptingCredentials.Alg, "dir", StringComparison.OrdinalIgnoreCase))
        {
            throw new NotSupportedException($"Algorithm '{encryptingCredentials.Alg}' is not supported. Only 'dir' is supported in this implementation.");
        }

        // Extract symmetric key
        if (encryptingCredentials.Key is not SymmetricSecurityKey symmetricKey)
        {
            throw new ArgumentException("EncryptingCredentials must use a SymmetricSecurityKey for 'dir' encryption.");
        }

        if (symmetricKey.Key.Length != 32)
        {
            throw new ArgumentException("Symmetric key must be 256 bits (32 bytes) for A256GCM.");
        }

        // JWE Header
        var jweHeader = new Dictionary<string, object>
    {
        { "alg", encryptingCredentials.Alg },
        { "enc", encryptingCredentials.Enc },
        { "typ", "JWT" },
    };

        // Merge custom parameters into the header
        if (customParams != null)
        {
            foreach (var kvp in customParams)
            {
                if (!jweHeader.ContainsKey(kvp.Key))
                {
                    jweHeader[kvp.Key] = kvp.Value;
                }
            }
        }

        var headerJson = JsonSerializer.Serialize(jweHeader);
        var headerBase64 = Base64UrlEncoder.Encode(Encoding.UTF8.GetBytes(headerJson));

        // Initialization Vector (12 bytes for GCM)
        byte[] iv = new byte[12];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(iv);
        }

        // Encrypt the payload using BouncyCastle GCM
        byte[] plaintext = Encoding.UTF8.GetBytes(payload);
        GcmBlockCipher gcm = new GcmBlockCipher(new Org.BouncyCastle.Crypto.Engines.AesEngine());
        AeadParameters parameters = new AeadParameters(new KeyParameter(symmetricKey.Key), 128, iv);

        gcm.Init(true, parameters);

        byte[] ciphertextWithTag = new byte[gcm.GetOutputSize(plaintext.Length)];
        int len = gcm.ProcessBytes(plaintext, 0, plaintext.Length, ciphertextWithTag, 0);
        gcm.DoFinal(ciphertextWithTag, len);

        // Separate ciphertext and tag
        byte[] ciphertext = new byte[plaintext.Length];
        byte[] tag = new byte[16]; // 128-bit tag

        Array.Copy(ciphertextWithTag, 0, ciphertext, 0, plaintext.Length);
        Array.Copy(ciphertextWithTag, plaintext.Length, tag, 0, tag.Length);

        // Build the JWE Compact Serialization
        string ivBase64 = Base64UrlEncoder.Encode(iv);
        string ciphertextBase64 = Base64UrlEncoder.Encode(ciphertext);
        string tagBase64 = Base64UrlEncoder.Encode(tag);

        // JWE format: <header>.<encryptedKey>.<iv>.<ciphertext>.<tag>
        string jweCompact = $"{headerBase64}..{ivBase64}.{ciphertextBase64}.{tagBase64}";

        return jweCompact;
    }

    /// <summary>
    /// Decrypts a JWE token using EncryptingCredentials (supports "dir").
    /// </summary>
    /// <param name="jweToken">The JWE token.</param>
    /// <param name="encryptingCredentials">The EncryptingCredentials object.</param>
    /// <returns>The decrypted payload.</returns>
    public string DecryptJwe(string jweToken, EncryptingCredentials encryptingCredentials)
    {
        if (encryptingCredentials == null)
        {
            throw new ArgumentNullException(nameof(encryptingCredentials));
        }

        // Validate algorithm
        if (!string.Equals(encryptingCredentials.Alg, "dir", StringComparison.OrdinalIgnoreCase))
        {
            throw new NotSupportedException($"Algorithm '{encryptingCredentials.Alg}' is not supported. Only 'dir' is supported in this implementation.");
        }

        // Extract symmetric key
        if (encryptingCredentials.Key is not SymmetricSecurityKey symmetricKey)
        {
            throw new ArgumentException("EncryptingCredentials must use a SymmetricSecurityKey for 'dir' encryption.");
        }

        // Split the JWE Compact Serialization
        var parts = jweToken.Split('.');
        if (parts.Length != 5)
        {
            throw new ArgumentException("Invalid JWE token format.");
        }

        byte[] iv = Base64UrlEncoder.DecodeBytes(parts[2]);
        byte[] ciphertext = Base64UrlEncoder.DecodeBytes(parts[3]);
        byte[] tag = Base64UrlEncoder.DecodeBytes(parts[4]);

        // Combine ciphertext and tag
        byte[] combinedCiphertext = new byte[ciphertext.Length + tag.Length];
        Array.Copy(ciphertext, 0, combinedCiphertext, 0, ciphertext.Length);
        Array.Copy(tag, 0, combinedCiphertext, ciphertext.Length, tag.Length);

        // Decrypt using BouncyCastle
        GcmBlockCipher gcm = new GcmBlockCipher(new Org.BouncyCastle.Crypto.Engines.AesEngine());
        AeadParameters parameters = new AeadParameters(new KeyParameter(symmetricKey.Key), 128, iv);

        gcm.Init(false, parameters);

        byte[] decryptedData = new byte[gcm.GetOutputSize(combinedCiphertext.Length)];
        int len = gcm.ProcessBytes(combinedCiphertext, 0, combinedCiphertext.Length, decryptedData, 0);
        gcm.DoFinal(decryptedData, len);

        return Encoding.UTF8.GetString(decryptedData).TrimEnd('\0');
    }
}
