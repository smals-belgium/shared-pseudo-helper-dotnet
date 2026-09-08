// <copyright file="IValueFactory.cs" company="Riziv-Inami">
// Copyright (c) Riziv-Inami. All rights reserved.
// </copyright>

namespace Nihdi.Common.Pseudonymisation;

using System.Text;
using Nihdi.Common.Pseudonymisation.Exceptions;

/// <summary>
/// Defines a factory for creating <see cref="IValue"/> objects from various data sources.
/// This interface provides methods to convert raw data (such as byte arrays and strings)
/// into typed value objects and to create collections of multiple values.
/// </summary>
public interface IValueFactory
{
    /// <summary>
    /// Returns the maximum size of the value (as bytes) that can be converted in a Point.
    /// Please note that this is the maximum theoretical size. eHealth asks us not
    /// to pseudonymise data with a size exceeding 32 bytes.
    /// </summary>
    /// <returns>The maximum size in bytes of the value.</returns>
    int GetMaxValueSize();

    /// <summary>
    /// Creates an <see cref="IValue"/> from the given byte array.
    /// </summary>
    /// <param name="value">Raw value to convert to <see cref="IValue"/>.</param>
    /// <returns>The <see cref="IValue"/> for the given byte array.</returns>
    IValue From(byte[] value);

    /// <summary>
    /// Creates an <see cref="IValue"/> from the given string, using the specified encoding.
    /// If no encoding is provided, UTF-8 is used by default.
    /// </summary>
    /// <param name="value">String to convert to <see cref="IValue"/>.</param>
    /// <param name="encoding">The encoding to use when converting the string to bytes.</param>
    /// <returns>The <see cref="IValue"/> for the given string.</returns>
    IValue From(string value, Encoding? encoding = null);

    /// <summary>
    /// Creates an empty <see cref="IMultipleValue"/>.
    /// </summary>
    /// <returns>An empty <see cref="IMultipleValue"/>.</returns>
    IMultipleValue Multiple();

    /// <summary>
    /// Creates an <see cref="IMultipleValue"/> from a collection of <see cref="IValue"/> objects.
    /// The items (references) in the collection are copied to the new multiple value.
    /// Changes to the collection after calling this method will not affect the created multiple value.
    /// </summary>
    /// <param name="values">
    /// A collection of <see cref="IValue"/> objects to be included in the multiple value.
    /// </param>
    /// <returns>An empty <see cref="IMultipleValue"/>.</returns>
    IMultipleValue Multiple(ICollection<IValue> values);
}