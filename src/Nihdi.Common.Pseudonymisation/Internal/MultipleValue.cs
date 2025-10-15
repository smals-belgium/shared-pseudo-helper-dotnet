// <copyright file="MultipleValue.cs" company="Riziv-Inami">
// Copyright (c) Riziv-Inami. All rights reserved.
// </copyright>

namespace Nihdi.Common.Pseudonymisation.Internal;

using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using Nihdi.Common.Pseudonymisation.Exceptions;
using Org.BouncyCastle.Math;

/// <inheritdoc/>
public class MultipleValue : MultiplePoint<IValue>, IMultipleValue
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MultipleValue"/> class.
    /// </summary>
    /// <param name="domain">The domain to which this collection belongs.</param>
    public MultipleValue(IDomain domain)
        : base((Domain)domain)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MultipleValue"/> class.
    /// </summary>
    /// <param name="domain">The domain to which this collection belongs.</param>
    /// <param name="values">The collection of values.</param>
    public MultipleValue(IDomain domain, ICollection<object> values)
        : base((Domain)domain, values)
    {
    }

    /// <inheritdoc/>
    public Task<IMultiplePseudonymInTransit> Pseudonymize()
    {
        return Task.FromResult((IMultiplePseudonymInTransit)this.PseudonymizeImpl().Result);
    }

    /// <inheritdoc/>
    public override void CheckCollectionSize(int size)
    {
        if (size > 10)
        {
            throw new ArgumentOutOfRangeException("The number of values" +
                "in this collection must be less or equal to 10");
        }
    }

    /// <inheritdoc/>
    public override IValue Validate(IValue value)
    {
        // Ensures that the value is from the expected domain
        if (value.Domain.Key != this.Domain.Key)
        {
            throw new ArgumentException(
                $"All given values are not from the domain `{this.Domain}`.");
        }

        return value;
    }

    private Task<MultiplePseudonymInTransit> PseudonymizeImpl()
    {
        if (this.Points.IsNullOrEmpty())
        {
            return Task.FromResult(new MultiplePseudonymInTransit(this.Domain));
        }

        var nbValues = this.Points.Count;
        if (nbValues == 1)
        {
            var value = this.Points[0] as IValue;

            if (value == null)
            {
                throw new InvalidOperationException("points[0] cannot be null");
            }

            return value.Pseudonymize()
                    .ContinueWith(pseudonymsInTransit =>
                    new MultiplePseudonymInTransit(this.Domain, new List<object> { pseudonymsInTransit }));
        }

        var randoms = new List<BigInteger>(nbValues);
        var payload = new JObject();
        var inputs = new JArray();
        payload.Add("inputs", inputs);
        for (int i = 0; i < nbValues; i++)
        {
            var random = this.Domain.CreateRandom();
            inputs.Add(this.Domain.CreatePayload(((Value)this.Points![i]).Multiply(random)));
            randoms.Add(random);
        }

        if (this.Domain.PseudonymisationClient == null)
        {
            throw new InvalidOperationException("domain.PseudonymisationClient cannot be null");
        }

        return this.Domain.PseudonymisationClient
            .PseudonymizeMultiple(this.Domain.Key!, payload.ToString())
            .ContinueWith(rawResponse =>
            {
                var response = JObject.Parse(rawResponse.Result);
                var outputs = response["outputs"] as JObject
                    ?? throw new InvalidOperationException("outputs fields is null");
                var pseudonymsInTransit = new MultiplePseudonymInTransit(this.Domain);
                var pseudonymInTransitFactory = this.Domain.PseudonymInTransitFactory;
                for (int i = 0; i < nbValues; i++)
                {
                    try
                    {
                        pseudonymsInTransit.Add(
                            pseudonymInTransitFactory.FromResponse(
                                outputs?[i.ToString()] as JObject ?? throw new ArgumentNullException(string.Empty),
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
}