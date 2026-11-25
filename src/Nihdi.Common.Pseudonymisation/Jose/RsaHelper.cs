// <copyright file="RsaHelper.cs" company="Riziv-Inami">
// Copyright (c) Riziv-Inami. All rights reserved.
// </copyright>

namespace Nihdi.Common.Pseudonymisation.Jose;

using System.Security.Cryptography;

/// <summary>
/// Helper class for RSA operations.
/// </summary>
public static class RsaHelper
{
    /// <summary>
    /// Creates an RSA instance that supports modern cryptographic operations.
    /// </summary>
    /// <returns>An RSA instance that supports OaepSHA256.</returns>
    public static RSA Create()
    {
        return RSA.Create();
    }

    /// <summary>
    /// Creates an RSA instance with the specified key size that supports modern
    /// cryptographic operations.
    /// </summary>
    /// <param name="keySizeInBits">The key size in bits.</param>
    /// <returns>An RSA instance that supports OaepSHA256.</returns>
    public static RSA Create(int keySizeInBits)
    {
        return RSA.Create(keySizeInBits);
    }
}