// <copyright file="JsonWebKeyFactoryTests.cs" company="Riziv-Inami">
// Copyright (c) Riziv-Inami. All rights reserved.
// </copyright>

namespace Nihdi.Common.Pseudonymisation.Tests.Jose;

using System;
using Microsoft.IdentityModel.Tokens;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nihdi.Common.Pseudonymisation.Jose;

[TestClass]
public class JsonWebKeyFactoryTests
{
    [TestMethod]
    public void Create_WithNullJson_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.ThrowsException<ArgumentNullException>(() =>
            JsonWebKeyFactory.Create(null!));

        Assert.AreEqual("jwkJson", exception.ParamName);
    }

    [TestMethod]
    public void Create_WithEmptyJson_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.ThrowsException<ArgumentNullException>(() =>
            JsonWebKeyFactory.Create(string.Empty));

        Assert.AreEqual("jwkJson", exception.ParamName);
    }

    [TestMethod]
    public void Create_WithInvalidJson_ThrowsException()
    {
        // Arrange
        var invalidJson = "not a valid json";

        // Act & Assert
        Assert.ThrowsException<ArgumentException>(() =>
            JsonWebKeyFactory.Create(invalidJson));
    }

    [TestMethod]
    public void Create_WithExistingAlgorithm_PreservesOriginalAlgorithm()
    {
        // Arrange
        var jwkJson = @"{
            ""kty"": ""RSA"",
            ""alg"": ""RS512"",
            ""use"": ""sig"",
            ""n"": ""xGOr-H7A-PWG3z0hanS8T7J3HiSckL0n_sCPYuAX-Ug"",
            ""e"": ""AQAB""
        }";

        // Act
        var jwk = JsonWebKeyFactory.Create(jwkJson);

        // Assert
        Assert.AreEqual("RS512", jwk.Alg);
    }

    [TestMethod]
    public void Create_RsaKey_WithEncUse_InfersRsaOaep256()
    {
        // Arrange
        var jwkJson = @"{
            ""kty"": ""RSA"",
            ""use"": ""enc"",
            ""n"": ""xGOr-H7A-PWG3z0hanS8T7J3HiSckL0n_sCPYuAX-Ug"",
            ""e"": ""AQAB""
        }";

        // Act
        var jwk = JsonWebKeyFactory.Create(jwkJson);

        // Assert
        Assert.AreEqual("RSA-OAEP-256", jwk.Alg);
    }

    [TestMethod]
    public void Create_RsaKey_WithSigUse_InfersRs256()
    {
        // Arrange
        var jwkJson = @"{
            ""kty"": ""RSA"",
            ""use"": ""sig"",
            ""n"": ""xGOr-H7A-PWG3z0hanS8T7J3HiSckL0n_sCPYuAX-Ug"",
            ""e"": ""AQAB""
        }";

        // Act
        var jwk = JsonWebKeyFactory.Create(jwkJson);

        // Assert
        Assert.AreEqual("RS256", jwk.Alg);
    }

    [TestMethod]
    public void Create_RsaKey_WithoutUse_DefaultsToRsaOaep256()
    {
        // Arrange
        var jwkJson = @"{
            ""kty"": ""RSA"",
            ""n"": ""xGOr-H7A-PWG3z0hanS8T7J3HiSckL0n_sCPYuAX-Ug"",
            ""e"": ""AQAB""
        }";

        // Act
        var jwk = JsonWebKeyFactory.Create(jwkJson);

        // Assert
        Assert.AreEqual("RSA-OAEP-256", jwk.Alg);
    }

    [TestMethod]
    public void Create_RsaKey_WithUnknownUse_DefaultsToRsaOaep256()
    {
        // Arrange
        var jwkJson = @"{
            ""kty"": ""RSA"",
            ""use"": ""unknown"",
            ""n"": ""xGOr-H7A-PWG3z0hanS8T7J3HiSckL0n_sCPYuAX-Ug"",
            ""e"": ""AQAB""
        }";

        // Act
        var jwk = JsonWebKeyFactory.Create(jwkJson);

        // Assert
        Assert.AreEqual("RSA-OAEP-256", jwk.Alg);
    }

    [TestMethod]
    public void Create_EcKey_WithoutAlgorithm_InfersEcdhEs()
    {
        // Arrange
        var jwkJson = @"{
            ""kty"": ""EC"",
            ""crv"": ""P-256"",
            ""x"": ""WKn-ZIGevcwGIyyrzFoZNBdaq9_TsqzGl96oc0CWuis"",
            ""y"": ""y77t-RvAHRKTsSGdIYUfweuOvwrvDD-Q3Hv5J0fSKbE""
        }";

        // Act
        var jwk = JsonWebKeyFactory.Create(jwkJson);

        // Assert
        Assert.AreEqual("ECDH-ES", jwk.Alg);
    }

    [TestMethod]
    public void Create_EcKey_WithExistingAlgorithm_PreservesAlgorithm()
    {
        // Arrange
        var jwkJson = @"{
            ""kty"": ""EC"",
            ""alg"": ""ES256"",
            ""crv"": ""P-256"",
            ""x"": ""WKn-ZIGevcwGIyyrzFoZNBdaq9_TsqzGl96oc0CWuis"",
            ""y"": ""y77t-RvAHRKTsSGdIYUfweuOvwrvDD-Q3Hv5J0fSKbE""
        }";

        // Act
        var jwk = JsonWebKeyFactory.Create(jwkJson);

        // Assert
        Assert.AreEqual("ES256", jwk.Alg);
    }

    [TestMethod]
    public void Create_OctKey_WithEncUse_InfersA256Gcm()
    {
        // Arrange
        var jwkJson = @"{
            ""kty"": ""oct"",
            ""use"": ""enc"",
            ""k"": ""GawgguFyGrWKav7AX4VKUg""
        }";

        // Act
        var jwk = JsonWebKeyFactory.Create(jwkJson);

        // Assert
        Assert.AreEqual("A256GCM", jwk.Alg);
    }

    [TestMethod]
    public void Create_OctKey_WithSigUse_InfersHs256()
    {
        // Arrange
        var jwkJson = @"{
            ""kty"": ""oct"",
            ""use"": ""sig"",
            ""k"": ""AyM1SysPpbyDfgZld3umj1qzKObwVMkoqQ-EstJQLr_T-1qS0gZH75aKtMN3Yj0iPS4hcgUuTwjAzZr1Z9CAow""
        }";

        // Act
        var jwk = JsonWebKeyFactory.Create(jwkJson);

        // Assert
        Assert.AreEqual("HS256", jwk.Alg);
    }

    [TestMethod]
    public void Create_OctKey_WithoutUse_DefaultsToA256Cm()
    {
        // Arrange
        var jwkJson = @"{
            ""kty"": ""oct"",
            ""k"": ""GawgguFyGrWKav7AX4VKUg""
        }";

        // Act
        var jwk = JsonWebKeyFactory.Create(jwkJson);

        // Assert
        Assert.AreEqual("A256CM", jwk.Alg);
    }

    [TestMethod]
    public void Create_OctKey_WithUnknownUse_DefaultsToA256Cm()
    {
        // Arrange
        var jwkJson = @"{
            ""kty"": ""oct"",
            ""use"": ""unknown"",
            ""k"": ""GawgguFyGrWKav7AX4VKUg""
        }";

        // Act
        var jwk = JsonWebKeyFactory.Create(jwkJson);

        // Assert
        Assert.AreEqual("A256GCM", jwk.Alg);
    }

    [TestMethod]
    public void Create_OkpKey_WithoutAlgorithm_InfersX25519()
    {
        // Arrange
        var jwkJson = @"{
            ""kty"": ""OKP"",
            ""crv"": ""X25519"",
            ""x"": ""hSDwCYkwp1R0i33ctD73Wg2_Og0mOBr066SpjqqbTmo""
        }";

        // Act
        var jwk = JsonWebKeyFactory.Create(jwkJson);

        // Assert
        Assert.AreEqual("X25519", jwk.Alg);
    }

    [TestMethod]
    public void Create_OkpKey_WithExistingAlgorithm_PreservesAlgorithm()
    {
        // Arrange
        var jwkJson = @"{
            ""kty"": ""OKP"",
            ""alg"": ""EdDSA"",
            ""crv"": ""Ed25519"",
            ""x"": ""11qYAYKxCrfVS_7TyWQHOg7hcvPapiMlrwIaaPcHURo""
        }";

        // Act
        var jwk = JsonWebKeyFactory.Create(jwkJson);

        // Assert
        Assert.AreEqual("EdDSA", jwk.Alg);
    }

    [TestMethod]
    public void Create_UnknownKeyType_ThrowsNotSupportedException()
    {
        // Arrange
        var jwkJson = @"{
            ""kty"": ""UNKNOWN"",
            ""k"": ""some-key-data""
        }";

        // Act & Assert
        var exception = Assert.ThrowsException<NotSupportedException>(() =>
            JsonWebKeyFactory.Create(jwkJson));

        Assert.AreEqual("Unsupported key type: UNKNOWN.", exception.Message);
    }

    [TestMethod]
    public void Create_MissingKeyType_ThrowsNotSupportedException()
    {
        // Arrange
        var jwkJson = @"{
            ""k"": ""some-key-data""
        }";

        // Act & Assert
        var exception = Assert.ThrowsException<NotSupportedException>(() =>
            JsonWebKeyFactory.Create(jwkJson));

        Assert.IsTrue(exception.Message.Contains("Unsupported key type"));
    }

    [TestMethod]
    public void Create_ComplexRsaKey_WithPrivateParameters_InfersCorrectAlgorithm()
    {
        // Arrange
        var jwkJson = @"{
            ""kty"": ""RSA"",
            ""use"": ""sig"",
            ""kid"": ""2011-04-29"",
            ""n"": ""0vx7agoebGcQSuuPiLJXZptN9nndrQmbXEps2aiAFbWhM78LhWx4cbbfAAtVT86zwu1RK7aPFFxuhDR1L6tSoc_BJECPebWKRXjBZCiFV4n3oknjhMstn64tZ_2W-5JsGY4Hc5n9yBXArwl93lqt7_RN5w6Cf0h4QyQ5v-65YGjQR0_FDW2QvzqY368QQMicAtaSqzs8KJZgnYb9c7d0zgdAZHzu6qMQvRL5hajrn1n91CbOpbISD08qNLyrdkt-bFTWhAI4vMQFh6WeZu0fM4lFd2NcRwr3XPksINHaQ-G_xBniIqbw0Ls1jF44-csFCur-kEgU8awapJzKnqDKgw"",
            ""e"": ""AQAB"",
            ""d"": ""X4cTteJY_gn4FYPsXB8rdXix5vwsg1FLN5E3EaG6RJoVH-HLLKD9M7dx5oo7GURknchnrRweUkC7hT5fJLM0WbFAKNLWY2vv7B6NqXSzUvxT0_YSfqijwp3RTzlBaCxWp4doFk5N2o8Gy_nHNKroADIkJ46pRUohsXywbReAdYaMwFs9tv8d_cPVY3i07a3t8MN6TNwm0dSawm9v47UiCl3Sk5ZiG7xojPLu4sbg1U2jx4IBTNBznbJSzFHK66jT8bgkuqsk0GjskDJk19Z4qwjwbsnn4j2WBii3RL-Us2lGVkY8fkFzme1z0HbIkfz0Y6mqnOYtqc0X4jfcKoAC8Q"",
            ""p"": ""83i-7IvMGXoMXCskv73TKr8637FiO7Z27zv8oj6pbWUQyLPQBQxtPVnwD20R-60eTDmD2ujnMt5PoqMrm8RfmNhVWDtjjMmCMjOpSXicFHj7XOuVIYQyqVWlWEh6dN36GVZYk93N8Bc9vY41xy8B9RzzOGVQzXvNEvn7O0nVbfs"",
            ""q"": ""3dfOR9cuYq-0S-mkFLzgItgMEfFzB2q3hWehMuG0oCuqnb3vobLyumqjVZQO1dIrdwgTnCdpYzBcOfW5r370AFXjiWft_NGEiovonizhKpo9VVS78TzFgxkIdrecRezsZ-1kYd_s1qDbxtkDEgfAITAG9LUnADun4vIcb6yelxk"",
            ""dp"": ""G4sPXkc6Ya9y8oJW9_ILj4xuppu0lzi_H7VTkS8xj5SdX3coE0oimYwxIi2emTAue0UOa5dpgFGyBJ4c8tQ2VF402XRugKDTP8akYhFo5tAA77Qe_NmtuYZc3C3m3I24G2GvR5sSDxUyAN2zq8Lfn9EUms6rY3Ob8YeiKkTiBj0"",
            ""dq"": ""s9lAH9fggBsoFR8Oac2R_E2gw282rT2kGOAhvIllETE1efrA6huUUvMfBcMpn8lqeW6vzznYY5SSQF7pMdC_agI3nG8Ibp1BUb0JUiraRNqUfLhcQb_d9GF4Dh7e74WbRsobRonujTYN1xCaP6TO61jvWrX-L18txXw494Q_cgk"",
            ""qi"": ""GyM_p6JrXySiz1toFgKbWV-JdI3jQ4ypu9rbMWx3rQJBfmt0FoYzgUIZEVFEcOqwemRN81zoDAaa-Bk0KWNGDjJHZDdDmFhW3AN7lI-puxk_mHZGJ11rxyR8O55XLSe3SPmRfKwZI6yU24ZxvQKFYItdldUKGzO6Ia6zTKhAVRU""
        }";

        // Act
        var jwk = JsonWebKeyFactory.Create(jwkJson);

        // Assert
        Assert.AreEqual("RS256", jwk.Alg);
    }

    [TestMethod]
    public void Create_SymmetricKey_WithBase64UrlEncodedKey_InfersCorrectAlgorithm()
    {
        // Arrange - 256-bit symmetric key
        var jwkJson = @"{
            ""kty"": ""oct"",
            ""use"": ""enc"",
            ""kid"": ""1"",
            ""k"": ""GawgguFyGrWKav7AX4VKUg_N8OMpYHwXBEb5CkJMkLE"",
            ""alg"": """"
        }";

        // Act
        var jwk = JsonWebKeyFactory.Create(jwkJson);

        // Assert
        Assert.AreEqual("A256GCM", jwk.Alg);
    }

    [TestMethod]
    public void Create_MultipleCallsWithSameJson_ProducesConsistentResults()
    {
        // Arrange
        var jwkJson = @"{
            ""kty"": ""RSA"",
            ""use"": ""enc"",
            ""n"": ""xGOr-H7A-PWG3z0hanS8T7J3HiSckL0n_sCPYuAX-Ug"",
            ""e"": ""AQAB""
        }";

        // Act
        var jwk1 = JsonWebKeyFactory.Create(jwkJson);
        var jwk2 = JsonWebKeyFactory.Create(jwkJson);

        // Assert
        Assert.AreEqual(jwk1.Alg, jwk2.Alg);
        Assert.AreEqual(jwk1.Kty, jwk2.Kty);
        Assert.AreEqual(jwk1.Use, jwk2.Use);
    }
}