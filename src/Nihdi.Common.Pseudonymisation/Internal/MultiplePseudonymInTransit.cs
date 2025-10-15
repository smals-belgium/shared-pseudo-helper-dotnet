// <copyright file="MultiplePseudonymInTransit.cs" company="Riziv-Inami">
// Copyright (c) Riziv-Inami. All rights reserved.
// </copyright>

namespace Nihdi.Common.Pseudonymisation.Internal;

using Newtonsoft.Json.Linq;
using Nihdi.Common.Pseudonymisation.Exceptions;
using Org.BouncyCastle.Math;

/// <summary>
/// Collection of <see cref="IPseudonymInTransit"/>s, all belonging to the same.
/// </summary>
public class MultiplePseudonymInTransit
    : MultiplePoint<IPseudonymInTransit>, IMultiplePseudonymInTransit
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MultiplePseudonymInTransit"/> class.
    /// </summary>
    /// <param name="domain">The domain to which the pseudonyms belong.</param>
    public MultiplePseudonymInTransit(IDomain domain)
        : base(domain)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MultiplePseudonymInTransit"/> class.
    /// </summary>
    /// <param name="domain">The domain to which the pseudonyms belong.</param>
    /// <param name="pseudonymsInTransit">The collection of pseudonyms in transit.</param>
    public MultiplePseudonymInTransit(
        IDomain domain,
        ICollection<object> pseudonymsInTransit)
        : base(domain, pseudonymsInTransit)
    {
    }

    /// <inheritdoc/>
    Task<IMultiplePseudonymInTransit>? IMultiplePseudonymInTransit.ConvertTo(IDomain toDomain)
    {
        if (toDomain == null)
        {
            throw new ArgumentNullException(nameof(toDomain));
        }

        var nbPseudonymsInTransit = Points?.Count ?? 0;

        if (nbPseudonymsInTransit == 0)
        {
            return Task.FromResult((IMultiplePseudonymInTransit)new MultiplePseudonymInTransit(toDomain));
        }

        if (nbPseudonymsInTransit == 1)
        {
            var pseudonymInTransitToConvert = Points?[0] as PseudonymInTransit
                ?? throw new InvalidOperationException($"The pseudonym to convert is not of type `{nameof(IPseudonymInTransit)}`");

            var result = pseudonymInTransitToConvert
                .ConvertTo(toDomain)
                .ContinueWith(pseudonymInTransit =>
                {
                    return new MultiplePseudonymInTransit(toDomain, new List<object> { pseudonymInTransit });
                });
        }

        var randoms = new List<BigInteger>(nbPseudonymsInTransit);
        var payload = new JObject();
        var inputs = new JArray();
        payload.Add("inputs", inputs);

        for (int i = 0; i < nbPseudonymsInTransit; i++)
        {
            var random = Domain.CreateRandom();

            var pseudonymInTransit = Points?[i] as PseudonymInTransit
                ?? throw new InvalidOperationException($"The pseudonym to convert is not of type `{nameof(IPseudonymInTransit)}`");

            if (pseudonymInTransit.GetTransitInfo() == null)
            {
                throw new InvalidOperationException("The pseudonym in transit must have a valid TransitInfo property.");
            }

            inputs.Add(Domain.CreatePayload(((Pseudonym)pseudonymInTransit.Pseudonym()).Multiply(random), pseudonymInTransit.GetTransitInfo().AsString()));
            randoms.Add(random);
        }

        if (Domain.PseudonymisationClient == null)
        {
            throw new InvalidOperationException("The pseudonymisation client attached to the domain cannot be null");
        }

        if (Domain.Key == null)
        {
            throw new InvalidOperationException("The domain's key cannot be null");
        }

        if (toDomain.Key == null)
        {
            throw new InvalidOperationException("The target domain's key cannot be null");
        }

        return Domain
            .PseudonymisationClient
            .ConvertMultipleTo(
                Domain.Key,
                toDomain.Key,
                payload.ToString())
            .ContinueWith(rawResponse =>
            {
                var response = JObject.Parse(rawResponse.Result);
                var outputsToken = response["outputs"]
                    ?? throw new InvalidOperationException("The response does not contain an 'outputs' field.");
                var outputs = (JArray)outputsToken;
                var pseudonymsInTransit = new MultiplePseudonymInTransit(toDomain);
                var pseudonymInTransitFactory = (PseudonymInTransitFactory)toDomain.PseudonymInTransitFactory;

                for (int i = 0; i < nbPseudonymsInTransit; i++)
                {
                    try
                    {
                        pseudonymsInTransit.Add(pseudonymInTransitFactory.FromResponse((JObject)outputs[i], randoms[i]));
                    }
                    catch (EHealthProblemException e)
                    {
                        pseudonymsInTransit.Add(e.Problem);
                    }
                }

                return (IMultiplePseudonymInTransit)pseudonymsInTransit;
            });
    }

    /// <inheritdoc/>
    public override void CheckCollectionSize(int size)
    {
        if (size > 10)
        {
            throw new ArgumentOutOfRangeException(
                "The number of pseudonyms in transit in this collection " +
                "must be less or equal to 10");
        }
    }

    /// <inheritdoc/>
    public Task<IMultipleValue> Identify()
    {
        var nbPseudonymsInTransit = Points?.Count ?? 0;

        if (nbPseudonymsInTransit == 0)
        {
            return Task.FromResult((IMultipleValue)new MultipleValue(Domain));
        }

        if (nbPseudonymsInTransit == 1)
        {
            var pseudonymInTransitToIdentify = Points?[0] as IPseudonymInTransit;

            if (pseudonymInTransitToIdentify == null)
            {
                throw new InvalidOperationException("The pseudonym to identify" +
                    $"is not of type `{nameof(IPseudonymInTransit)}`");
            }

            var value = pseudonymInTransitToIdentify
            .Identify()
            .ContinueWith(task =>
            {
                if (task.Status == TaskStatus.RanToCompletion)
                {
                    return new MultipleValue((IDomain)base.Domain, new List<object> { task.Result });
                }
                else if (task.Exception?.InnerException is EHealthProblemException eHealthProblemException)
                {
                    return new MultipleValue((IDomain)base.Domain, new List<object> { eHealthProblemException.Problem });
                }
                else
                {
                    task.GetAwaiter().GetResult(); // This will rethrow the original exception
                    return null; // Never reached
                }
            });
        }

        var randoms = new List<BigInteger>(nbPseudonymsInTransit);
        var payload = new JObject();
        var inputs = new JArray();

        payload.Add("inputs", inputs);
        for (int i = 0; i < nbPseudonymsInTransit; i++)
        {
            var random = Domain.CreateRandom();
            var pseudonymInTransit = (PseudonymInTransit?)Points?[i] ?? throw new InvalidCastException("Cannot cast points[i] to PseundonymInTransit.");
            var payloadObj = JObject.Parse(Domain.CreatePayload(((Pseudonym)((PseudonymInTransit)Points![i]).Pseudonym()).Multiply(random), pseudonymInTransit.GetTransitInfo().AsString()));
            payloadObj["transitInfo"] = pseudonymInTransit.GetTransitInfo().AsString();

            inputs.Add(payloadObj);
            randoms.Add(random);
        }

        if (Domain.PseudonymisationClient == null)
        {
            throw new InvalidOperationException("The pseudonymisation client " +
                "attached to the domain cannot be null");
        }

        if (Domain.Key == null)
        {
            throw new InvalidOperationException("The domain's key cannot be null");
        }

        return Domain
.PseudonymisationClient
            .IdentifyMultiple(
                Domain.Key,
                payload.ToString())
            .ContinueWith(rawResponse =>
            {
                var response = JObject.Parse(rawResponse.Result);
                var jsonString = response?["outputs"]?.ToString();
                if (jsonString == null)
                {
                    throw new InvalidOperationException("The raw response does" +
                        "contain an `outputs` field");
                }

                var outputs = JArray.Parse(jsonString);

                var values = new MultipleValue(Domain);
                var pseudonymFactory = Domain.PseudonymFactory;

                for (int i = 0; i < nbPseudonymsInTransit; i++)
                {
                    try
                    {
                        values.Add(pseudonymFactory.FromResponse((JObject)outputs[i], randoms[i]).AsValue());
                    }
                    catch (EHealthProblemException ex)
                    {
                        values.Add(ex.Problem);
                    }
                }

                return (IMultipleValue)values;
            });
    }

    /// <inheritdoc/>
    public override IPseudonymInTransit Validate(IPseudonymInTransit pseudonymInTransit)
    {
        // Ensures that the pseudonym in transit is from the expected domain
        if (pseudonymInTransit.Domain.Key != Domain.Key)
        {
            throw new ArgumentException("All given pseudonyms are not " +
                $"from the domain `{Domain}`.");
        }

        return pseudonymInTransit;
    }
}