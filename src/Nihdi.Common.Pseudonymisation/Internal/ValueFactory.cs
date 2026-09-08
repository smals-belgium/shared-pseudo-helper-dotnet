// <copyright file="ValueFactory.cs" company="Riziv-Inami">
// Copyright (c) Riziv-Inami. All rights reserved.
// </copyright>

namespace Nihdi.Common.Pseudonymisation.Internal;

using System.Collections.Generic;
using System.Text;
using Nihdi.Common.Pseudonymisation;
using Nihdi.Common.Pseudonymisation.Exceptions;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Math.EC;

/// <inheritdoc/>
public class ValueFactory : PointFactory, IValueFactory
{
    private readonly int _maxValueSize;

    /// <summary>
    /// Initializes a new instance of the <see cref="ValueFactory"/> class.
    /// </summary>
    /// <param name="domain">The domain to use for the value factory.</param>
    public ValueFactory(IDomain domain)
        : base(domain)
    {
        var curve = this.Domain.Curve;
        _maxValueSize = (curve.FieldSize / 8) - this.Domain.BufferSize - 1;
    }

    /// <inheritdoc/>
    public int GetMaxValueSize()
    {
        return _maxValueSize;
    }

    /// <inheritdoc/>
    public IValue From(byte[] value)
    {
        if (value == null)
        {
            throw new ArgumentNullException(nameof(value));
        }

        if (value.Length > _maxValueSize)
        {
            throw new InvalidValueException("The value is too long: should be max " + _maxValueSize + " bytes");
        }

        var i = Domain.BufferSize;

        // Create a new Byte Array with a length 1 + value.length + 1 + bufferSize
        // The first Byte is set to 0
        var xCoordinateBytes = new byte[1 + value.Length + 1 + i];

        // Copy the value into xBytes starting at position one
        int position = 1;
        Array.Copy(value, 0, xCoordinateBytes, position, value.Length);
        position += value.Length;
        xCoordinateBytes[position] = (byte)value.Length;

        // Compute the X coordinates by converting the xBytes to a BigInteger
        // Then put the X Coordinate on the elliptic curve
        var xCoordinates = new BigInteger(xCoordinateBytes);

        // Compute y on the elliptic curve
        var y = ComputeY(Domain.Curve, xCoordinates);
        while (y == null)
        {
            xCoordinates = xCoordinates.Add(BigInteger.One);
            y = ComputeY(Domain.Curve, xCoordinates);
        }

        return new Value(CreateEcPoint(xCoordinates, y), Domain);
    }

    /// <inheritdoc/>
    public IValue From(string value, Encoding? encoding = null)
    {
        if (value == null)
        {
            throw new ArgumentNullException(nameof(value));
        }

        if (encoding == null)
        {
            encoding = Encoding.UTF8;
        }

        return From(encoding.GetBytes(value));
    }

    /// <inheritdoc/>
    public IMultipleValue Multiple()
    {
        return new MultipleValue(Domain);
    }

    /// <inheritdoc/>
    public IMultipleValue Multiple(ICollection<IValue> values)
    {
        if (values == null)
        {
            throw new ArgumentNullException(nameof(values));
        }

        return new MultipleValue(Domain, values.ToList<object>());
    }

    private static BigInteger? ComputeY(ECCurve curve, BigInteger x)
    {
        ECFieldElement xFieldElement = curve.FromBigInteger(x);
        ECFieldElement rhs = xFieldElement.Square().Add(curve.A).Multiply(xFieldElement).Add(curve.B);
        ECFieldElement y = rhs.Sqrt();

        return y?.ToBigInteger();
    }

    private ECPoint CreateEcPoint(BigInteger x, BigInteger y)
    {
        try
        {
            return Domain.Curve.CreatePoint(x, y);
        }
        catch (Exception e)
        {
            throw new InvalidValueException("Invalid coordinates", e);
        }
    }
}