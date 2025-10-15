// <copyright file="DefaultTransitInfoCustomizer.cs" company="Riziv-Inami">
// Copyright (c) Riziv-Inami. All rights reserved.
// </copyright>

namespace Nihdi.Common.Pseudonymisation.Internal;

using Nihdi.Common.Pseudonymisation;

/// <summary>
/// Default implementation of <see cref="ITransitInfoCustomizer"/> that provides empty header and payload dictionaries.
/// </summary>
internal class DefaultTransitInfoCustomizer : ITransitInfoCustomizer
{
    /// <inheritdoc/>
    public Dictionary<string, object> Header => new Dictionary<string, object>();

    /// <inheritdoc/>
    public Dictionary<string, object> Payload => new Dictionary<string, object>();
}