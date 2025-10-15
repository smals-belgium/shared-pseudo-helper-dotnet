// <copyright file="IValue.cs" company="Riziv-Inami">
// Copyright (c) Riziv-Inami. All rights reserved.
// </copyright>

namespace Nihdi.Common.Pseudonymisation;

using System.Text;

// tag:interface[]

/// <summary>
/// Wrapper around an elliptic curve point representing a value, that provides useful methods to manipulate it.
/// </summary>
public interface IValue : IPoint
{
    /// <summary>
    /// Returns the value as bytes array.
    /// Use it for non-text values.
    /// </summary>
    /// <returns>The value as a bytes array.</returns>
    byte[] AsBytes();

    /// <summary>
    /// Returns the value as String.
    /// Convenient method that converts the bytes array to a String.
    /// Use it only for text values for which you called <see cref="IValueFactory.From(string, Encoding)"/>.
    /// </summary>
    /// <param name="encoding">The encoding to use for the conversion.</param>
    /// <returns>The value as a string.</returns>
    string AsString(Encoding encoding);

    /// <summary>
    /// Returns the value as a String.
    /// Convenient method that converts the bytes array (representing UTF-8) to a string.
    /// Use it for text values.
    /// </summary>
    /// <returns>The value as string.</returns>
    string AsString();

    /// <summary>
    /// Pseudonymize this <see cref="IValue"/>.
    /// </summary>
    /// <returns>
    /// A random <see cref="IPseudonymInTransit"/> for this <see cref="IValue"/>.
    /// </returns>.
    Task<IPseudonymInTransit> Pseudonymize();
}

// end::interface[]