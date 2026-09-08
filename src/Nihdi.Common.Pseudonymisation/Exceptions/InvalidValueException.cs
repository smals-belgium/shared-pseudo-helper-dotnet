// <copyright file="InvalidValueException.cs" company="Riziv-Inami">
// Copyright (c) Riziv-Inami. All rights reserved.
// </copyright>

namespace Nihdi.Common.Pseudonymisation.Exceptions;

using System.Runtime.Serialization;

/// <summary>
/// Exception thrown when a value is invalid for pseudonymisation operations.
/// </summary>
/// <remarks>
/// This exception is typically thrown when processing a value that does not meet
/// the requirements or format needed for pseudonymisation.
/// </remarks>
[Serializable]
public class InvalidValueException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="InvalidValueException"/> class.
    /// </summary>
    public InvalidValueException()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="InvalidValueException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public InvalidValueException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="InvalidValueException"/> class with a specified error message
    /// and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="inner">The exception that is the cause of the current exception, or a null reference if no inner exception is specified.</param>
    public InvalidValueException(string message, Exception inner)
        : base(message, inner)
    {
    }
}