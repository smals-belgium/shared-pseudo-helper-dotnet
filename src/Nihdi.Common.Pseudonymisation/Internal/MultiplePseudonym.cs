// <copyright file="MultiplePseudonym.cs" company="Riziv-Inami">
// Copyright (c) Riziv-Inami. All rights reserved.
// </copyright>

namespace Nihdi.Common.Pseudonymisation.Internal;

using Newtonsoft.Json.Linq;
using Nihdi.Common.Pseudonymisation;
using Nihdi.Common.Pseudonymisation.Exceptions;
using Org.BouncyCastle.Math;

/// <inheritdoc/>
public class MultiplePseudonym : MultiplePoint<IPseudonym>, IMultiplePseudonym
{
    /// <inheritdoc/>
    public MultiplePseudonym(IDomain domain)
        : base((Domain)domain)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MultiplePseudonym"/> class.
    /// </summary>
    /// <param name="domain">The domain to which these pseudonyms belong.</param>
    /// <param name="pseudonyms">A collection of pseudonyms to be managed together.</param>
    /// <remarks>
    /// This constructor creates a multiple pseudonym container by passing the domain and a list
    /// conversion of the pseudonyms to the base constructor.
    /// </remarks>
    public MultiplePseudonym(IDomain domain, ICollection<IPseudonym> pseudonyms)
       : base((Domain)domain, pseudonyms.ToList<object>())
    {
    }

    /// <inheritdoc/>
    public override void CheckCollectionSize(int size)
    {
        if (size > 10)
        {
            throw new ArgumentOutOfRangeException(
                nameof(size),
                "The number of pseudonyms in this collection must be less or equal to 10");
        }
    }

    /// <inheritdoc/>
    Task<MultiplePseudonymInTransit> IMultiplePseudonym.ConvertTo(IDomain toDomain)
    {
        if (Domain.PseudonymisationClient == null)
        {
            throw new InvalidOperationException("domain.PseudonymisationClient cannot be null.");
        }

        var nbPseudonyms = Points?.Count ?? 0;

        if (nbPseudonyms == 0)
        {
            return Task.FromResult(new MultiplePseudonymInTransit(toDomain));
        }

        if (nbPseudonyms == 1)
        {
            var pseudonym = Points?.FirstOrDefault() as IPseudonym;

            if (pseudonym == null)
            {
                throw new ArgumentNullException(nameof(pseudonym));
            }

            pseudonym.ConvertTo(toDomain)
                .ContinueWith(t =>
                {
                    var pseudonymsInTransit = new List<object> { t.Result };
                    return new MultiplePseudonymInTransit(
                        toDomain,
                        pseudonymsInTransit);
                });
        }

        var randoms = new List<BigInteger>(nbPseudonyms);
        var payload = new JObject();
        var inputs = new JArray();
        payload.Add("inputs", inputs);
        for (int i = 0; i < nbPseudonyms; i++)
        {
            var random = Domain.CreateRandom();
            inputs.Add(Domain.CreatePayload(((Pseudonym)Points![i]).Multiply(random)));
            randoms.Add(random);
        }

        if (Domain.Key == null)
        {
            throw new ArgumentNullException(nameof(Domain.Key));
        }

        if (toDomain.Key == null)
        {
            throw new ArgumentNullException(nameof(toDomain.Key));
        }

        return Domain
.PseudonymisationClient
            .ConvertMultipleTo(Domain.Key, toDomain.Key, payload.ToString())
            .ContinueWith(t =>
            {
                var response = JObject.Parse(t.Result);
                var outputs = response["outputs"] as JObject
                    ?? throw new InvalidOperationException("outputs fields is null");
                var pseudonymsInTransit = new MultiplePseudonymInTransit(toDomain);
                var pseudonymInTransitFactory =
                    (PseudonymInTransitFactory)toDomain.PseudonymInTransitFactory;
                for (int i = 0; i < nbPseudonyms; i++)
                {
                    try
                    {
                        pseudonymsInTransit.Add(
                            pseudonymInTransitFactory.FromResponse(
                                outputs?[i] as JObject ?? throw new ArgumentNullException(string.Empty),
                                randoms[i]));
                    }
                    catch (EHealthProblemException ex)
                    {
                        pseudonymsInTransit.Add(ex.Problem);
                    }
                }

                return pseudonymsInTransit;
            });
    }

    /// <inheritdoc/>
    public override IPseudonym Validate(IPseudonym pseudonym)
    {
        // Ensures that the pseudonym is from the expected domain
        if (pseudonym.Domain.Key != Domain.Key)
        {
            throw new ArgumentException("All given pseudonyms are not " +
                $"from the domain `{Domain.Key}`");
        }

        // Ensures that the pseudonym is not an instance PseudonymInTransit
        if (pseudonym is IPseudonymInTransit)
        {
            throw new ArgumentException("None of the provided pseudonyms " +
                $"can be of type `{nameof(IPseudonymInTransit)}`");
        }

        return pseudonym;
    }
}