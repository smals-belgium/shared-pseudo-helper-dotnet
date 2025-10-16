// <copyright file="RsaHelper.cs" company="Riziv-Inami">
// Copyright (c) Riziv-Inami. All rights reserved.
// </copyright>

namespace Nihdi.Common.Pseudonymisation.Jose;

using System.Security.Cryptography;

/// <summary>
/// Helper class for RSA operations that provides cross-platform compatibility
/// between .NET Framework 4.8 and .NET 8+.
/// </summary>
public static class RsaHelper
{
    /// <summary>
    /// Creates an RSA instance that supports modern cryptographic operations
    /// on both .NET Framework 4.8 and .NET 8+.
    /// </summary>
    /// <returns>An RSA instance that supports OaepSHA256 on all platforms.</returns>
    /// <remarks>
    /// On .NET Framework 4.8, this returns RSACng which supports OaepSHA256.
    /// On .NET 8+, this returns the default RSA implementation.
    /// </remarks>
    public static RSA Create()
    {
#if NETFRAMEWORK
        return new RSACng();
#else
        return RSA.Create();
#endif
    }

    /// <summary>
    /// Creates an RSA instance with the specified key size that supports modern
    /// cryptographic operations on both .NET Framework 4.8 and .NET 8+.
    /// </summary>
    /// <param name="keySizeInBits">The key size in bits.</param>
    /// <returns>An RSA instance that supports OaepSHA256 on all platforms.</returns>
    public static RSA Create(int keySizeInBits)
    {
#if NETFRAMEWORK
        return new RSACng(keySizeInBits);
#else
        return RSA.Create(keySizeInBits);
#endif
    }
}