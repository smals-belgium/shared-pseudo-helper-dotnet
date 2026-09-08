// <copyright file="IDomain.cs" company="Riziv-Inami">
// Copyright (c) Riziv-Inami. All rights reserved.
// </copyright>

namespace Nihdi.Common.Pseudonymisation;

// tag::interface[]

/// <summary>
/// Represents a domain for pseudonymisation operations, providing factories and configuration
/// for pseudonym generation and management.
/// </summary>
public interface IDomain
{
    /// <summary>
    /// Gets the key for this domain.
    /// </summary>
    /// <value>A string that represents the key of this pseudonymisation domain.</value>
    public string? Key
    {
        get;
    }

    /// <summary>
    /// Gets the factory responsible for creating values within this domain.
    /// </summary>
    /// <value>The value factory implementation for this domain.</value>
    public IValueFactory ValueFactory
    {
        get;
    }

    /// <summary>
    /// Gets the factory responsible for creating pseudonyms within this domain.
    /// </summary>
    /// <value>The pseudonym factory implementation for this domain.</value>
    public IPseudonymFactory PseudonymFactory
    {
        get;
    }

    /// <summary>
    /// Gets the factory responsible for creating pseudonyms that are in transit between systems.
    /// </summary>
    /// <value>The pseudonym-in-transit factory implementation for this domain.</value>
    /// public IPseudonymInTransitFactory PseudonymInTransitFactory
    public IPseudonymInTransitFactory PseudonymInTransitFactory
    {
        get;
    }

    /// <summary>
    /// Gets the buffer size used for pseudonymisation operations within this domain.
    /// </summary>
    /// <value>An integer representing the size of the buffer.</value>
    int BufferSize
    {
        get;
    }
}

// end::interface[]