// <copyright file="TransitInfoCustomizerBase.cs" company="Riziv-Inami">
// Copyright (c) Riziv-Inami. All rights reserved.
// </copyright>

namespace Nihdi.Common.Pseudonymisation;

/// <summary>
/// Base implementation of <see cref="ITransitInfoCustomizer"/> that provides
/// customization capabilities for pseudonym transit information.
/// </summary>
public abstract class TransitInfoCustomizerBase : ITransitInfoCustomizer
{
    /// <inheritdoc />
    public virtual Dictionary<string, object> Header => new Dictionary<string, object>();

    /// <inheritdoc />
    public virtual Dictionary<string, object> Payload => new Dictionary<string, object>();
}