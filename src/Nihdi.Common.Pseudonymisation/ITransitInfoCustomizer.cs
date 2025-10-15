// <copyright file="ITransitInfoCustomizer.cs" company="Riziv-Inami">
// Copyright (c) Riziv-Inami. All rights reserved.
// </copyright>

namespace Nihdi.Common.Pseudonymisation;

/// <summary>
/// Interface to customize the <see cref="ITransitInfo"/> of a <see cref="IPseudonymInTransit"/>.
/// </summary>
public interface ITransitInfoCustomizer
{
    /// <summary>
    /// Gets the protected <see cref="ITransitInfo"/> header parameters to add to the <see cref="IPseudonymInTransit"/>.
    /// Use this to add any information that recipients who are not able to decrypt the <see cref="ITransitInfo"/> need to know about this <see cref="ITransitInfo"/>.
    /// Please note that managed params as aud, and exp will be overriden if you provide them.
    /// Warning: the returned dictionary cannot be null!.
    /// </summary>
    /// <value>The protected header parameters.</value>
    Dictionary<string, object> Header
    {
        get;
    }

    /// <summary>
    /// Gets return the private <see cref="ITransitInfo"/> payload properties to add in the <see cref="IPseudonymInTransit"/>.
    /// Use this to add any information that recipients who are able to decrypt the <see cref="ITransitInfo"/> need to know about this <see cref="ITransitInfo"/>.
    /// Please note that managed params as aud, iat and exp will be overriden if you provide them.
    /// </summary>
    /// <value>The private payload properties.</value>
    Dictionary<string, object> Payload
    {
        get;
    }
}