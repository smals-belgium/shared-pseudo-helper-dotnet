// <copyright file="ITransitInfo.cs" company="Riziv-Inami">
// Copyright (c) Riziv-Inami. All rights reserved.
// </copyright>

namespace Nihdi.Common.Pseudonymisation;

/// <summary>
/// Transit info containing encrypted inofrmation about the <see cref="IPseudonymInTransit"/>.
/// It contains the encrypted headers <c>iat</c>, <c>exp</c> and <c>scalar</c>
/// which is the scalar to use to "decrypt" the <see cref="IPseudonymInTransit"/>.
/// It also contains public headers like <c>iat</c> and <c>exp</c>.
/// </summary>
public interface ITransitInfo
{
    /// <summary>
    /// Returns the JWE compact representation of this <see cref="ITransitInfo"/>.
    /// </summary>
    /// <returns>The JWE compact representation of this <see cref="ITransitInfo"/>.</returns>
    string AsString();

    /// <summary>
    /// Returns the audience of this <see cref="ITransitInfo"/>.
    /// </summary>
    /// <returns>The audience of this <see cref="ITransitInfo"/>.</returns>
    string Audience();

    /// <summary>
    /// Validate the header of this <see cref="ITransitInfo"/>.
    /// </summary>
    void ValidateHeader();

    /// <summary>
    /// Returns a dictionary containing the parameters of the header of this <see cref="ITransitInfo"/>.
    /// Changes done on the returned dictionary are not reflected to this <see cref="ITransitInfo"/>.
    /// </summary>
    /// <returns>Dictionary containing the parameters of the header of this <see cref="ITransitInfo"/>.</returns>
    Dictionary<string, object> Header();

    /// <summary>
    /// Returns a dictionary containing the payload of this <see cref="ITransitInfo"/>.
    /// Changes done on the returned dictionary are not reflected to this <see cref="ITransitInfo"/>.
    /// </summary>
    /// <returns>A dictionary containing the parameters of the payload of this <see cref="ITransitInfo"/>.</returns>
    Dictionary<string, object>? Payload();
}