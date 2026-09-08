// <copyright file="PseudonymFactory.cs" company="Riziv-Inami">
// Copyright (c) Riziv-Inami. All rights reserved.
// </copyright>

namespace Nihdi.Common.Pseudonymisation.Internal;

using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using Nihdi.Common.Pseudonymisation.Exceptions;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Math.EC;
using static Nihdi.Common.Pseudonymisation.Internal.Point;

/// <inheritdoc/>
public class PseudonymFactory : PointFactory, IPseudonymFactory
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PseudonymFactory"/> class.
    /// </summary>
    /// <param name="domain">The domain to which this factory belongs.</param>
    public PseudonymFactory(IDomain domain)
        : base(domain)
    {
    }

    /// <inheritdoc/>
    IPseudonym IPseudonymFactory.FromX(string xAsBase64String)
    {
        return this.FromX(xAsBase64String);
    }

    /// <inheritdoc/>
    IPseudonym IPseudonymFactory.FromXy(string xAsBase64String, string yAsBase64String)
    {
       return this.FromXy(xAsBase64String, yAsBase64String);
    }

    /// <inheritdoc/>
    IMultiplePseudonym IPseudonymFactory.Multiple()
    {
        return this.Multiple();
    }

    /// <inheritdoc/>
    IMultiplePseudonym IPseudonymFactory.Multiple(IList<IPseudonym> pseudonyms)
    {
        return this.Multiple(pseudonyms);
    }

    /// <summary>
    /// Creates a pseudonym from the given X coordinate in Base64 string format.
    /// The Y coordinate will be computed and one of the two possible values will be randomly chosen.
    /// The Y coordinate can be randomly chosen because only X is important in the context of
    /// eHealth pseudonymisation.
    /// </summary>
    /// <param name="xAsBase64String">Base64 string representation of the X coordinate.</param>
    /// <returns>A <see cref="IPseudonym"/> instance created from the given X coordinate.</returns>
    public Pseudonym FromX(string xAsBase64String)
    {
        byte[] xAsBytes;

        try
        {
            xAsBytes = Base64UrlEncoder.DecodeBytes(xAsBase64String);
        }
        catch (Exception ex)
        {
            throw new InvalidPseudonymException(
                "The X coordinate is not a valid Base64 string",
                ex);
        }

        var x = ToBigInteger(xAsBytes, "The X coordinate cannot be converted in BigInteger");
        var y = ComputeY(Domain.Curve, x);

        if (y == null)
        {
            throw new InvalidPseudonymException("Invalid X coordinate: no Y " +
                "coordinate can be computed for this X coordinate");
        }

        return new Pseudonym(CreateEcPoint(x, y), Domain);
    }

    /// <summary>
    /// Creates a pseudonym from the given X coordinate in Base64 string format.
    /// </summary>
    /// <param name="xAsBase64String">The Base64 encoded X coordinate.</param>
    /// <param name="yAsBase64String">The Base64 encoded Y coordinate.</param>
    /// <returns>The created pseudonym.</returns>
    public Pseudonym FromXy(string xAsBase64String, string yAsBase64String)
    {
        if (string.IsNullOrEmpty(xAsBase64String))
        {
            throw new ArgumentException("The Base64 encoded X coordinate is empty or null.", xAsBase64String);
        }

        if (string.IsNullOrEmpty(yAsBase64String))
        {
            throw new ArgumentException("The Base64 encoded Y coordinate is empty or null.", yAsBase64String);
        }

        var xAsBytes = Convert.FromBase64String(xAsBase64String);
        var yAsBytes = Convert.FromBase64String(yAsBase64String);
        var x = new BigInteger(xAsBytes);
        var y = new BigInteger(yAsBytes);

        return new Pseudonym(CreateEcPoint(x, y), Domain);
    }

    /// <summary>
    /// Creates an empty collection for multiple pseudonyms.
    /// </summary>
    /// <returns>An empty <see cref="IMultiplePseudonym"/> instance.</returns>
    public MultiplePseudonym Multiple()
    {
        return new MultiplePseudonym(Domain);
    }

    /// <summary>
    /// Creates a <see cref="IMultiplePseudonym"/> containing the items of the given
    /// <see cref="IList{IPseudonym}"/> collection.
    /// The items (references) of the given collection are copied to the new instance.
    /// Changes done to the given collection after this call will not be reflected
    /// in the created instance.
    /// </summary>
    /// <param name="pseudonyms"><see cref="IList{IPseudonym}"/> collection of pseudonyms.</param>
    /// <returns>A <see cref="IMultiplePseudonym"/> instance containing the given pseudonyms.</returns>
    public MultiplePseudonym Multiple(IList<IPseudonym> pseudonyms)
    {
        if (pseudonyms == null)
        {
            throw new ArgumentNullException(nameof(pseudonyms));
        }

        return new MultiplePseudonym(Domain, pseudonyms);
    }

    /// <summary>
    /// Creates a pseudonym from the given raw JSON response string received from eHealth.
    /// The pseudonym is unblinded using the provided scalar.
    /// </summary>
    /// <param name="rawResponse">The raw JSON response string from eHealth.</param>
    /// <param name="scalar">The scalar used to unblind the pseudonym.</param>
    /// <returns>The unblinded pseudonym.</returns>
    internal Pseudonym FromRawResponse(string rawResponse, BigInteger scalar)
    {
        return FromResponse(JObject.Parse(rawResponse), scalar);
    }

    /// <summary>
    /// Creates a pseudonym from the given JSON response object received from eHealth.
    /// The pseudonym is unblinded using the provided scalar.
    /// </summary>
    /// <param name="response">The JSON response object from eHealth.</param>
    /// <param name="scalar">The scalar used to unblind the pseudonym.</param>
    /// <returns>The unblinded pseudonym.</returns>
    internal Pseudonym FromResponse(JObject response, BigInteger scalar)
    {
        if (!IsAcceptableResponse(response))
        {
            throw new EHealthProblemException(EHealthProblem.FromResponse(response));
        }

        var domainFromResponse = response?["domain"]?.ToString();
        if (domainFromResponse != Domain.Key)
        {
            throw new InvalidDataException("Pseudonmym sent by eHealth is invalid: "
                + $"`{domainFromResponse}` does not match the expected domain `{Domain.Key}`");
        }

        try
        {
            var x = response?["x"]?.ToString();
            var y = response?["y"]?.ToString();
            var blindedPseudonym = FromXy(x!, y!) as Pseudonym;

            if (blindedPseudonym == null)
            {
                throw new InvalidPseudonymException();
            }

            return blindedPseudonym.MultiplyByModInverse(scalar);
        }
        catch (InvalidPseudonymException e)
        {
            throw new InvalidOperationException("Pseudonym sent by eHealth is invalid", e);
        }
    }

    /// <summary>
    /// Creates a pseudonym in transit from its SEC1 representation and transit info.
    /// </summary>
    /// <param name="sec1">The SEC1 representation of the pseudonym.</param>
    /// <returns>The pseudonym created from the SEC1 representation.</returns>
    internal Pseudonym FromSec1(string sec1)
    {
        if (string.IsNullOrEmpty(sec1))
        {
            throw new ArgumentNullException(nameof(sec1));
        }

        var sec1AsBytes = Base64UrlEncoder.DecodeBytes(sec1);
        var ecPoint = DecodeSec1(sec1AsBytes);
        return new Pseudonym(ecPoint, Domain);
    }

    private bool IsAcceptableResponse(JObject response)
    {
        return response.ContainsKey("x") && response.ContainsKey("y");
    }

    private BigInteger ToBigInteger(byte[] xAsBytes, string exceptionMessage)
    {
        try
        {
            return new BigInteger(xAsBytes);
        }
        catch (Exception ex)
        {
            throw new InvalidPseudonymException(exceptionMessage, ex);
        }
    }

    private ECPoint CreateEcPoint(BigInteger x, BigInteger y)
    {
        try
        {
            return Domain.Curve.CreatePoint(x, y);
        }
        catch (Exception ex)
        {
            throw new InvalidPseudonymException("Invalid coordinates", ex);
        }
    }

    private ECPoint DecodeSec1(byte[] sec1AsBytes)
    {
        try
        {
            return Domain.Curve.DecodePoint(sec1AsBytes);
        }
        catch (Exception ex)
        {
            throw new InvalidPseudonymException("Invalid SEC1 representation of the point", ex);
        }
    }
}