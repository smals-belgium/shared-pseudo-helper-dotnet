// <copyright file="IPseudonymisationClient.cs" company="Riziv-Inami">
// Copyright (c) Riziv-Inami. All rights reserved.
// </copyright>

namespace Nihdi.Common.Pseudonymisation;

/// <summary>
/// Implement this interface to allow the <see cref="PseudonymisationHelper"/>
/// to call the eHealth Pseudonymisation service.
/// Do not forget to add the following headers in each request:.
///  <ul>
///    <li>Content-Type: 'application/json'</li>
///    <li>From: see eHealth Pseudonymisation cookbook</li>
///    <li>User-Agent: see eHealth Pseudonymisation cookbook</li>
///  </ul>
/// </summary>
public interface IPseudonymisationClient
{
    // tag::methods[]

    /// <summary>
    /// Call to /pseudo/v1/domains/{domainKey} and return the response as a
    /// String.
    /// Each call to this method <strong>must</strong> make a call to eHealth
    /// pseudonymisation service: please do not return a cached response.
    /// </summary>
    /// <param name="domainKey">The domain key.</param>
    /// <returns>The response as a string.</returns>
    Task<string> GetDomain(string domainKey);

    /// <summary>
    /// Call to /pseudo/v1/domains/{domainKey}/identify with the given
    /// payload and return the response as a String.
    /// </summary>
    /// <param name="domainKey">The domain key.</param>
    /// <param name="payload">The request body.</param>
    /// <returns>the response as a string.</returns>
    Task<string> Identify(string domainKey, string payload);

    /// <summary>
    /// Call to /pseudo/v1/domains/{domainKey}/identify with the given
    /// payload and return the response as a String.
    /// </summary>
    /// <param name="domainKey">The domain key.</param>
    /// <param name="payload">The request body.</param>
    /// <returns>the response as a string.</returns>
    Task<string> IdentifyMultiple(string domainKey, string payload);

    /// <summary>
    /// Call to /pseudo/v1/domains/{domainKey}/pseudonymize with the given
    /// payload and return the response as a String.
    /// </summary>
    /// <param name="domainKey">The domain key.</param>
    /// <param name="payload">The request body.</param>
    /// <returns>the response as a string.</returns>
    Task<string> Pseudonymize(string domainKey, string payload);

    /// <summary>
    /// Call to /pseudo/v1/domains/{domainKey}/pseudonymizeMultiple with the
    /// given payload and return the response as a String.
    /// </summary>
    /// <param name="domainKey">The domain key.</param>
    /// <param name="payload">The request body.</param>
    /// <returns>the response as a string.</returns>
    Task<string> PseudonymizeMultiple(string domainKey, string payload);

    /// <summary>
    /// Call to /pseudo/v1/domains/{fromDomainKey}/convertTo/{toDomainKey}
    /// with the given payload and return the response as a String.
    /// </summary>
    /// <param name="fromDomainKey">The domain of the pseudonym to convert.</param>
    /// <param name="toDomainKey">The target domain.</param>
    /// <param name="payload">The request body.</param>
    /// <returns>The response as a string.</returns>
    Task<string> ConvertTo(string fromDomainKey, string toDomainKey, string payload);

    /// <summary>
    /// Call to /pseudo/v1/domains/{fromDomainKey}/convertToMultiple/{toDomainKey}
    /// with the given payload and return the response as a String.
    /// </summary>
    /// <param name="fromDomainKey">The domain of the pseudonym to convert.</param>
    /// <param name="toDomainKey">The target domain.</param>
    /// <param name="payload">The request body.</param>
    /// <returns>The response as a string.</returns>
    Task<string> ConvertMultipleTo(string fromDomainKey, string toDomainKey, string payload);

    // end::methods[]
}
