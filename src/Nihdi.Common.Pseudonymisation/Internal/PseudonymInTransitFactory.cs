// <copyright file="PseudonymInTransitFactory.cs" company="Riziv-Inami">
// Copyright (c) Riziv-Inami. All rights reserved.
// </copyright>

namespace Nihdi.Common.Pseudonymisation.Internal;

using System.Collections.ObjectModel;
using Newtonsoft.Json.Linq;
using Nihdi.Common.Pseudonymisation.Exceptions;
using Org.BouncyCastle.Math;

/// <summary>
/// Factory class responsible for creating pseudonym in transit objects.
/// This class inherits from <see cref="PointFactory"/> and implements
/// <see cref="IPseudonymInTransitFactory"/>.
/// </summary>
/// <remarks>
/// A pseudonym in transit represents a pseudonym with associated transit information,
/// typically used during the transmission of pseudonymized data between systems.
/// </remarks>
public class PseudonymInTransitFactory : PointFactory, IPseudonymInTransitFactory
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PseudonymInTransitFactory"/> class.
    /// </summary>
    /// <param name="domain">The domain to be used for pseudonym creation.</param>
    public PseudonymInTransitFactory(IDomain domain)
        : base(domain)
    {
    }

    /// <inheritdoc/>
    public IPseudonymInTransit FromSec1AndTransitInfo(string sec1AndTransitInfo)
    {
        if (string.IsNullOrEmpty(sec1AndTransitInfo))
        {
            throw new ArgumentNullException(nameof(sec1AndTransitInfo));
        }

        var colonPos = sec1AndTransitInfo.IndexOf(':');
        if (colonPos == -1)
        {
            throw new InvalidPseudonymException(
                "Missing `:` in the pseudonym in transit string." +
                "Format must be {seci1InBase64Url}:{transitInfoInBase64Url}");
        }

        var pseudonym = Domain.PseudonymFactory.FromSec1(sec1AndTransitInfo.Substring(0, colonPos));
        var transitInfo = new TransitInfo(Domain, sec1AndTransitInfo.Substring(colonPos + 1));
        return new PseudonymInTransit(pseudonym, transitInfo);
    }

    /// <inheritdoc/>
    public IPseudonymInTransit FromXYAndTransitInfo(string x, string y, string transitInfo)
    {
        return new PseudonymInTransit(Domain.PseudonymFactory.FromXy(x, y), new TransitInfo(Domain, transitInfo));
    }

    /// <inheritdoc/>
    public IMultiplePseudonymInTransit Multiple()
    {
        return new MultiplePseudonymInTransit(Domain);
    }

    /// <inheritdoc/>
    public IMultiplePseudonymInTransit Multiple(Collection<IPseudonymInTransit> pseudonymsInTransit)
    {
        return new MultiplePseudonymInTransit(Domain, pseudonymsInTransit.ToArray<object>());
    }

    /// <summary>
    /// Creates a <see cref="IPseudonymInTransit"/> from the given raw response and scalar.
    /// </summary>
    /// <param name="rawResponse">A raw JSON string representing the response.</param>
    /// <param name="scalar">A <see cref="BigInteger"/> instance representing the scalar value.</param>
    /// <returns>A new instance of <see cref="IPseudonymInTransit"/>.</returns>
    public IPseudonymInTransit FromRawResponse(string rawResponse, BigInteger scalar)
    {
        return FromResponse(JObject.Parse(rawResponse), scalar);
    }

    /// <summary>
    /// Creates a <see cref="IPseudonymInTransit"/> from the given response and scalar.
    /// </summary>
    /// <param name="response">A <see cref="JObject"/> representing the response.</param>
    /// <param name="scalar">A <see cref="BigInteger"/> instance representing the scalar value.</param>
    /// <returns>A new instance of <see cref="IPseudonymInTransit"/>.</returns>
    internal IPseudonymInTransit FromResponse(JObject response, BigInteger scalar)
    {
        if (!IsAcceptableResponse(response))
        {
            throw new EHealthProblemException(EHealthProblem.FromResponse(response));
        }

        return new PseudonymInTransit(
            Domain.PseudonymFactory.FromResponse(response, scalar),
            new TransitInfo(Domain, response["transitInfo"]!.ToString()),
            null);
    }

    private bool IsAcceptableResponse(JObject? response)
    {
        return response?["transitInfo"] != null;
    }
}
