// <copyright file="Value.cs" company="Riziv-Inami">
// Copyright (c) Riziv-Inami. All rights reserved.
// </copyright>

namespace Nihdi.Common.Pseudonymisation.Internal;

using System.Text;
using System.Threading.Tasks;
using Nihdi.Common.Pseudonymisation;
using Org.BouncyCastle.Math.EC;

/// <inheritdoc/>
internal class Value : Pseudonym, IValue
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Value"/> class.
    /// </summary>
    /// <param name="ecPoint">The elliptic curve point representing the value.</param>
    /// <param name="domain">The domain associated with the value.</param>
    public Value(ECPoint ecPoint, IDomain domain)
        : base(ecPoint, domain)
    {
    }

    /// <inheritdoc/>
    public byte[] AsBytes()
    {
        var x = EcPoint.XCoord.GetEncoded();
        var valueLengthPos = x.Length - Domain.BufferSize - 1;
        var valueLength = x[valueLengthPos];
        var startPosition = valueLengthPos - valueLength;
        // ArraySegment takes a number of bytes, not an end position.
        return new ArraySegment<byte>(x, startPosition, valueLength).ToArray();
    }

    /// <inheritdoc/>
    public string AsString(Encoding encoding)
    {
        var x = EcPoint.XCoord.GetEncoded();
        var valueLenghtPos = x.Length - Domain.BufferSize - 1;
        var valueLength = x[valueLenghtPos];
        var startPosition = valueLenghtPos - valueLength;
        return encoding.GetString(x, startPosition, valueLength);
    }

    /// <inheritdoc/>
    public new string AsString()
    {
        return AsString(Encoding.UTF8);
    }

    /// <inheritdoc/>
    public Task<IPseudonymInTransit> Pseudonymize(CancellationToken cancellationToken = default)
    {
        if (Domain.PseudonymisationClient == null)
        {
            throw new InvalidOperationException("PseudonymisationClient is null");
        }

        var random = Domain.CreateRandom();
        var blindedValue = new Pseudonym(EcPoint.Multiply(random).Normalize(), Domain);
        var payload = Domain.CreatePayloadString(blindedValue);
        var pseudonymInTransitFactory = Domain.PseudonymInTransitFactory;
        if (Domain.Key == null)
        {
            throw new ArgumentNullException(nameof(Domain.Key));
        }

        return TimeLimit.WaitFor(Domain.PseudonymisationClient.Pseudonymize(Domain.Key, payload, cancellationToken), cancellationToken)
                .ContinueWith(rawResponse =>
                    pseudonymInTransitFactory.FromRawResponse(rawResponse.GetAwaiter().GetResult(), random));
    }
}