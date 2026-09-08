// <copyright file="JweDecryptionException.cs" company="Riziv-Inami">
// Copyright (c) Riziv-Inami. All rights reserved.
// </copyright>

namespace Nihdi.Common.Pseudonymisation.Jose;

using System;

/// <summary>
/// Exception thrown when JWE decryption fails.
/// </summary>
public class JweDecryptionException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="JweDecryptionException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    public JweDecryptionException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="JweDecryptionException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The inner exception.</param>
    public JweDecryptionException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}