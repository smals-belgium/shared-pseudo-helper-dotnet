// <copyright file="JweDecryption.cs" company="Riziv-Inami">
// Copyright (c) Riziv-Inami. All rights reserved.
// </copyright>

namespace Nihdi.Common.Pseudonymisation.Jose;

using System;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Parameters;

/// <summary>
/// Provides methods for JWE decryption operations.
/// </summary>
public class JweDecryption
{
    /// <summary>
    /// Unwraps the Content Encryption Key (CEK) using RSA.
    /// </summary>
    /// <param name="encryptedCek">The encrypted Content Encryption Key.</param>
    /// <param name="key">The RSA security key used for unwrapping.</param>
    /// <returns>The unwrapped Content Encryption Key.</returns>
    public static byte[] UnwrapKey(byte[] encryptedCek, SecurityKey key)
    {
        if (key is RsaSecurityKey rsaKey)
        {
            // Always create a new RSA instance using the helper to ensure compatibility
            using (var rsa = RsaHelper.Create())
            {
                // Import parameters from either the RSA instance or the parameters
                if (rsaKey.Rsa != null)
                {
                    rsa.ImportParameters(rsaKey.Rsa.ExportParameters(true));
                }
                else
                {
                    rsa.ImportParameters(rsaKey.Parameters);
                }

                return rsa.Decrypt(encryptedCek, RSAEncryptionPadding.OaepSHA256);
            }
        }

        throw new NotSupportedException("Only RSA key unwrapping is supported.");
    }

    /// <summary>
    /// Decrypts the ciphertext using AES-GCM.
    /// </summary>
    /// <param name="aad">The additional authenticated data (AAD).</param>
    /// <param name="cek">The Content Encryption Key (CEK).</param>
    /// <param name="iv">The initialization vector (IV).</param>
    /// <param name="ciphertext">The ciphertext to decrypt.</param>
    /// <param name="authTag">The authentication tag.</param>
    /// <returns>The decrypted plaintext.</returns>
    public static byte[] DecryptAesGcm(byte[] aad, byte[] cek, byte[] iv, byte[] ciphertext, byte[] authTag)
    {
        if (aad == null)
        {
            throw new ArgumentNullException(nameof(aad));
        }

        if (cek == null)
        {
            throw new ArgumentNullException(nameof(cek));
        }

        if (iv == null)
        {
            throw new ArgumentNullException(nameof(iv));
        }

        if (ciphertext == null)
        {
            throw new ArgumentNullException(nameof(ciphertext));
        }

        if (authTag == null)
        {
            throw new ArgumentNullException(nameof(authTag));
        }

        byte[] cipherTextWithTag = new byte[ciphertext.Length + authTag.Length];
        Buffer.BlockCopy(ciphertext, 0, cipherTextWithTag, 0, ciphertext.Length);
        Buffer.BlockCopy(authTag, 0, cipherTextWithTag, ciphertext.Length, authTag.Length);

        try
        {
            var gcm = new GcmBlockCipher(new AesEngine());
            int tagSizeInBits = authTag.Length * 8;
            var parameters = new AeadParameters(new KeyParameter(cek), tagSizeInBits, iv, aad);
            gcm.Init(false, parameters);

            byte[] output = new byte[gcm.GetOutputSize(cipherTextWithTag.Length)];
            int len = gcm.ProcessBytes(cipherTextWithTag, 0, cipherTextWithTag.Length, output, 0);
            int finalLen = gcm.DoFinal(output, len);
            int totalLen = len + finalLen;

            if (totalLen == output.Length)
            {
                return output;
            }
            else
            {
                byte[] result = new byte[totalLen];
                Array.Copy(output, 0, result, 0, totalLen);
                return result;
            }
        }
        catch (InvalidCipherTextException)
        {
            throw new Exception("AuthenticationException failed: Invalid ciphertext or tag.");
        }
    }
}
