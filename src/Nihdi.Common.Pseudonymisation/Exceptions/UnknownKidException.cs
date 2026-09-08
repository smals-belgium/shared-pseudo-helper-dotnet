// <copyright file="UnknownKidException.cs" company="Riziv-Inami">
// Copyright (c) Riziv-Inami. All rights reserved.
// </copyright>

namespace Nihdi.Common.Pseudonymisation.Exceptions;

/// <summary>
/// Exception thrown when a Key Identifier (KID) is not recognized or when no corresponding private key is available for decryption.
/// </summary>
/// <remarks>
/// This exception is typically thrown during cryptographic operations when attempting to use a key that cannot be found
/// or when trying to decrypt data using an unrecognized key identifier.
/// </remarks>
internal class UnknownKidException : InvalidTransitInfoException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UnknownKidException"/> class with the specified Key ID.
    /// </summary>
    /// <param name="kid">The Key ID that was not recognized or for which no private key was found.</param>
    public UnknownKidException(string kid)
        : base($"Unknown kid `{kid}` (or no private key found to decrypt it).")
    {
        Kid = kid;
    }

    /// <summary>
    /// Gets the Key ID that caused this exception.
    /// </summary>
    /// <value>The unrecognized Key ID string.</value>
    public string Kid
    {
        get;
    }
}
