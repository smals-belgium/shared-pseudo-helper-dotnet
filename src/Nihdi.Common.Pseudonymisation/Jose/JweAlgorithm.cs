// <copyright file="JweAlgorithm.cs" company="Riziv-Inami">
// Copyright (c) Riziv-Inami. All rights reserved.
// </copyright>

namespace Nihdi.Common.Pseudonymisation.Jose;

/// <summary>
/// Represents JWE (JSON Web Encryption) algorithms.
/// </summary>
public readonly struct JweAlgorithm : IEquatable<JweAlgorithm>
{
    /// <summary>
    /// Direct use of a shared symmetric key as the CEK (Content Encryption Key).
    /// </summary>
    /// <returns>The JWE algorithm string.</returns>
    public static readonly JweAlgorithm DIR = new("dir");

    /// <summary>
    /// RSAES-PKCS1-v1_5 encryption.
    /// </summary>
    /// <returns>The JWE algorithm string.</returns>
    public static readonly JweAlgorithm RSA1_5 = new("RSA1_5");

    /// <summary>
    /// RSAES OAEP using default parameters.
    /// </summary>
    /// <returns>The JWE algorithm string.</returns>
    public static readonly JweAlgorithm RSA_OAEP = new("RSA-OAEP");

    /// <summary>
    /// RSAES OAEP using SHA-256 and MGF1 with SHA-256.
    /// </summary>
    /// <returns>The JWE algorithm string.</returns>
    public static readonly JweAlgorithm RSA_OAEP_256 = new("RSA-OAEP-256");

    /// <summary>
    /// AES Key Wrap with 128-bit key.
    /// </summary>
    /// <returns>The JWE algorithm string.</returns>
    public static readonly JweAlgorithm A128KW = new("A128KW");

    /// <summary>
    /// AES Key Wrap with 192-bit key.
    /// </summary>
    /// <returns>The JWE algorithm string.</returns>
    public static readonly JweAlgorithm A192KW = new("A192KW");

    /// <summary>
    /// AES Key Wrap with 256-bit key.
    /// </summary>
    /// <returns>The JWE algorithm string.</returns>
    public static readonly JweAlgorithm A256KW = new("A256KW");

    /// <summary>
    /// Elliptic Curve Diffie-Hellman Ephemeral Static key agreement using Concat KDF.
    /// </summary>
    /// <returns>The JWE algorithm string.</returns>
    public static readonly JweAlgorithm ECDH_ES = new("ECDH-ES");

    /// <summary>
    /// ECDH-ES using Concat KDF and CEK wrapped with A128KW.
    /// </summary>
    /// <returns>The JWE algorithm string.</returns>
    public static readonly JweAlgorithm ECDH_ES_A128KW = new("ECDH-ES+A128KW");

    /// <summary>
    /// ECDH-ES using Concat KDF and CEK wrapped with A192KW.
    /// </summary>
    /// <returns>The JWE algorithm string.</returns>
    public static readonly JweAlgorithm ECDH_ES_A192KW = new("ECDH-ES+A192KW");

    /// <summary>
    /// ECDH-ES using Concat KDF and CEK wrapped with A256KW.
    /// </summary>
    /// <returns>The JWE algorithm string.</returns>
    public static readonly JweAlgorithm ECDH_ES_A256KW = new("ECDH-ES+A256KW");

    /// <summary>
    /// PBES2 with HMAC SHA-256 and A128KW wrapping.
    /// </summary>
    /// <returns>The JWE algorithm string.</returns>
    public static readonly JweAlgorithm PBES2_HS256_A128KW = new("PBES2-HS256+A128KW");

    /// <summary>
    /// PBES2 with HMAC SHA-384 and A192KW wrapping.
    /// </summary>
    /// <returns>The JWE algorithm string.</returns>
    public static readonly JweAlgorithm PBES2_HS384_A192KW = new("PBES2-HS384+A192KW");

    /// <summary>
    /// PBES2 with HMAC SHA-512 and A256KW wrapping.
    /// </summary>
    /// <returns>The JWE algorithm string.</returns>
    public static readonly JweAlgorithm PBES2_HS512_A256KW = new("PBES2-HS512+A256KW");

    /// <summary>
    /// Exposes all predefined algorithms for iteration.
    /// </summary>
    /// <value>A read-only list of all JWE algorithms.</value>
    public static readonly IReadOnlyList<JweAlgorithm> AllAlgorithms = new List<JweAlgorithm>
    {
        DIR, RSA1_5, RSA_OAEP, RSA_OAEP_256, A128KW, A192KW, A256KW,
        ECDH_ES, ECDH_ES_A128KW, ECDH_ES_A192KW, ECDH_ES_A256KW,
        PBES2_HS256_A128KW, PBES2_HS384_A192KW, PBES2_HS512_A256KW,
    }.AsReadOnly();

    private readonly string _value;

    /// <summary>
    /// Initializes a new instance of the <see cref="JweAlgorithm"/> struct.
    /// </summary>
    /// <param name="value">The string representation of the algorithm.</param>
    /// <returns>A JweAlgorithm instance.</returns>
    private JweAlgorithm(string value)
    {
        _value = value;
    }

    /// <summary>
    /// Implicit conversion from JweAlgorithm to string.
    /// </summary>
    /// <param name="algorithm">The JweAlgorithm instance.</param>
    public static implicit operator string(JweAlgorithm algorithm)
    {
        return algorithm._value;
    }

    /// <summary>
    /// Equality operator for JweAlgorithm.
    /// </summary>
    /// <param name="left">The left operand.</param>
    /// <param name="right">The right operand.</param>
    /// <returns>True if both operands are equal; otherwise, false.</returns>
    public static bool operator ==(JweAlgorithm left, JweAlgorithm right)
    {
        return left.Equals(right);
    }

    /// <summary>
    /// Inequality operator for JweAlgorithm.
    /// </summary>
    /// <param name="left">The left operand.</param>
    /// <param name="right">The right operand.</param>
    /// <returns>True if both operands are not equal; otherwise, false.</returns>
    public static bool operator !=(JweAlgorithm left, JweAlgorithm right)
    {
        return !(left == right);
    }

    /// <summary>
    /// Creates a JweAlgorithm instance from a string value.
    /// </summary>
    /// <param name="value">The string representation of the algorithm.</param>
    /// <returns>A JweAlgorithm instance.</returns>
    public static JweAlgorithm FromString(string value)
    {
        var algorithm = AllAlgorithms.FirstOrDefault(alg => alg._value.Equals(value, StringComparison.OrdinalIgnoreCase));
        return algorithm._value != null ? algorithm : new JweAlgorithm(value);
    }

    /// <summary>
    /// Returns the string representation of the JWE algorithm.
    /// </summary>
    /// <returns>The JWE algorithm string.</returns>
    public override string ToString() => _value;

    /// <summary>
    /// Determines whether the specified <see cref="JweAlgorithm"/> is equal to the current <see cref="JweAlgorithm"/>.
    /// </summary>
    /// <param name="other">The other <see cref="JweAlgorithm"/> to compare.</param>
    /// <returns><c>true</c> if the specified <see cref="JweAlgorithm"/> is equal to the current <see cref="JweAlgorithm"/>; otherwise, <c>false</c>.</returns>
    public bool Equals(JweAlgorithm other) => _value == other._value;

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is JweAlgorithm other && Equals(other);

    /// <inheritdoc/>
    public override int GetHashCode() => _value.GetHashCode();
}