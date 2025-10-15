// <copyright file="InvalidTransitInfoException.cs" company="Riziv-Inami">
// Copyright (c) Riziv-Inami. All rights reserved.
// </copyright>

namespace Nihdi.Common.Pseudonymisation.Exceptions;

/// <summary>
/// Exception thrown when transit information in the pseudonymisation process is invalid.
/// </summary>
/// <remarks>
/// This exception is thrown when the provided transit information does not meet the required format or contains invalid data
/// that prevents proper pseudonymisation processing.
/// </remarks>
/// <seealso cref="System.Exception" />
[Serializable]
public class InvalidTransitInfoException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="InvalidTransitInfoException"/> class.
    /// </summary>
    public InvalidTransitInfoException()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="InvalidTransitInfoException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public InvalidTransitInfoException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="InvalidTransitInfoException"/> class with a specified error message
    /// and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="inner">The exception that is the cause of the current exception, or a null reference if no inner exception is specified.</param>
    public InvalidTransitInfoException(string message, Exception inner)
        : base(message, inner)
    {
    }
}