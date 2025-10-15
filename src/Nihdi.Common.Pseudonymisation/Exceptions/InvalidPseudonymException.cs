// <copyright file="InvalidPseudonymException.cs" company="Riziv-Inami">
// Copyright (c) Riziv-Inami. All rights reserved.
// </copyright>

namespace Nihdi.Common.Pseudonymisation.Exceptions;

/// <summary>
/// Exception thrown when a pseudonym is invalid.
/// </summary>
[Serializable]
public class InvalidPseudonymException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="InvalidPseudonymException"/> class.
    /// </summary>
    public InvalidPseudonymException()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="InvalidPseudonymException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public InvalidPseudonymException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="InvalidPseudonymException"/> class with a specified error message and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="inner">The exception that is the cause of the current exception.</param>
    public InvalidPseudonymException(string message, Exception inner)
        : base(message, inner)
    {
    }
}