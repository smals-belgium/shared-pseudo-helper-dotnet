// <copyright file="PseudonymizationHelperTests.cs" company="Riziv-Inami">
// Copyright (c) Riziv-Inami. All rights reserved.
// </copyright>

using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using Nihdi.Common.Pseudonymisation.Extensions;
using Nihdi.Common.Pseudonymisation.Internal;
using Nihdi.Common.Pseudonymisation.Utils;

namespace Nihdi.Common.Pseudonymisation.Tests;

[TestClass]
public class PseudonymisationHelperTest
{
    private static readonly IDateTimeService _dateTimeService = new DateTimeService();
    private static readonly Dictionary<string, RSA> _generatedRsaKeys = new Dictionary<string, RSA>();
    private static readonly Dictionary<string, string> _generatedCertificates = new Dictionary<string, string>();
    private static readonly Dictionary<string, string> _privateJwksByHash = new Dictionary<string, string>();
    private static PseudonymisationHelper? _pseudonymisationHelper;
    private static string _dynamicDomainJson = string.Empty;
    private static (string Kid, RSA Rsa) _uhmepKey;
    private static (string Kid, RSA Rsa) _pseudoKey;
    private static string _uhmepJwk = string.Empty;
    private static string _pseudoJwk = string.Empty;

    public TestContext? TestContext
    {
        get; set;
    }

    [ClassInitialize]
    public static void ClassInitialize(TestContext context)
    {
        // Generate RSA keys and certificates
        _uhmepKey = GenerateRsaKeyAndCertificate("UHMEP", "0406798006");
        _pseudoKey = GenerateRsaKeyAndCertificate("PSEUDONYMISATION", "0809394427");

        // Generate symmetric keys for domain secrets
        var secretKey1 = GenerateSymmetricKey();
        var secretKey2 = GenerateSymmetricKey();

        // Create and store the private JWKs with their x5t#S256 hash as the key
        _uhmepJwk = CreatePrivateJwk(_generatedRsaKeys[_uhmepKey.Kid], _uhmepKey.Kid);
        var uhmepHash = ComputeX5tS256(_generatedCertificates[_uhmepKey.Kid]);
        _privateJwksByHash[uhmepHash] = _uhmepJwk;

        _pseudoJwk = CreatePrivateJwk(_generatedRsaKeys[_pseudoKey.Kid], _pseudoKey.Kid);
        var pseudoHash = ComputeX5tS256(_generatedCertificates[_pseudoKey.Kid]);
        _privateJwksByHash[pseudoHash] = _pseudoJwk;

        // Create domain JSON structure
        _dynamicDomainJson = GenerateDomainJson(
            _uhmepKey,
            _pseudoKey,
            secretKey1,
            secretKey2);

        // Create private key supplier that simply looks up the JWK by hash
        Func<string, string> privateKeySupplier = hash =>
        {
            if (string.IsNullOrEmpty(hash))
            {
                throw new ArgumentException("Hash cannot be null or empty");
            }

            if (_privateJwksByHash.TryGetValue(hash, out var jwk))
            {
                return jwk;
            }

            var availableHashes = string.Join(", ", _privateJwksByHash.Keys);
            throw new ArgumentException($"Could not find a private key corresponding to the hash: {hash}. Available hashes: {availableHashes}");
        };

        // Create JWKS supplier - should return the public keys in JWKS format
        Func<Task<string>> jwksSupplier = () => Task.FromResult(CreateJwks(_uhmepKey, _pseudoKey));

        var internalPseudonymisationHelper =
            PseudonymisationHelper.Builder()
                                  .JwksUrl(new Uri("https://api-acpt.ehealth.fgov.be/etee/v1/pubKeys/cacerts/jwks?identifier=0406798006&type=CBE&applicationIdentifier=UHMEP&use=enc"))
                                  .JwkSupplier(jwksSupplier)
                                  .PrivateKeySupplier(privateKeySupplier)
                                  .PseudonymisationClient(new PseudonymisationClient(_dateTimeService, _dynamicDomainJson))
                                  .Build();

        _pseudonymisationHelper =
            PseudonymisationHelper.Builder()
                                  .JwksUrl(new Uri("https://api-acpt.ehealth.fgov.be/etee/v1/pubKeys/cacerts/jwks?identifier=0406798006&type=CBE&applicationIdentifier=UHMEP&use=enc"))
                                  .JwkSupplier(jwksSupplier)
                                  .PrivateKeySupplier(privateKeySupplier)
                                  .PseudonymisationClient(new InternalPseudonymisationClient(internalPseudonymisationHelper, _dateTimeService, _dynamicDomainJson))
                                  .Build();
    }

    [ClassCleanup]
    public static void ClassCleanup()
    {
        // Dispose of RSA keys
        foreach (var rsa in _generatedRsaKeys.Values)
        {
            rsa?.Dispose();
        }

        _generatedRsaKeys.Clear();
        _generatedCertificates.Clear();
        _privateJwksByHash.Clear();
    }

    [TestMethod]
    [Description("Shows a sample of the generated domain.json and keys used in tests. Run this test to inspect the test data structure.")]
    // Comment to actually run the test.
    // [Ignore]
    public void ShowGeneratedTestData()
    {
        try
        {
            TestContext?.WriteLine("========================================");
            TestContext?.WriteLine("GENERATED TEST DATA SAMPLE");
            TestContext?.WriteLine("========================================");
            TestContext?.WriteLine(string.Empty);

            TestContext?.WriteLine("This test shows the structure of the generated test data.");
            TestContext?.WriteLine("The domain.json simulates the eHealth service response without requiring actual service calls.");
            TestContext?.WriteLine(string.Empty);

            TestContext?.WriteLine("--- DOMAIN.JSON ---");
            TestContext?.WriteLine(_dynamicDomainJson);
            TestContext?.WriteLine(string.Empty);

            TestContext?.WriteLine("--- UHMEP PRIVATE JWK ---");
            TestContext?.WriteLine($"Kid: {_uhmepKey.Kid}");
            TestContext?.WriteLine($"x5t#S256: {ComputeX5tS256(_generatedCertificates[_uhmepKey.Kid])}");
            TestContext?.WriteLine(_uhmepJwk);
            TestContext?.WriteLine(string.Empty);

            TestContext?.WriteLine("--- PSEUDONYMISATION PRIVATE JWK ---");
            TestContext?.WriteLine($"Kid: {_pseudoKey.Kid}");
            TestContext?.WriteLine($"x5t#S256: {ComputeX5tS256(_generatedCertificates[_pseudoKey.Kid])}");
            TestContext?.WriteLine(_pseudoJwk);
            TestContext?.WriteLine(string.Empty);

            TestContext?.WriteLine("--- PUBLIC KEYS JWKS ---");
            TestContext?.WriteLine(CreateJwks(_uhmepKey, _pseudoKey));
            TestContext?.WriteLine(string.Empty);

            TestContext?.WriteLine("--- TEST INFORMATION ---");
            TestContext?.WriteLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            TestContext?.WriteLine("Test SSINs used: 012345678910, 987654321012, 123456789012");
            TestContext?.WriteLine(string.Empty);
            TestContext?.WriteLine("NOTE: This is TEST DATA for DEMONSTRATION PURPOSES ONLY, using self-signed certificates.");
            TestContext?.WriteLine("DO NOT use these keys in production or with actual eHealth services.");

            // Also write to console for immediate visibility
            Console.WriteLine("========================================");
            Console.WriteLine("GENERATED TEST DATA SAMPLE");
            Console.WriteLine("========================================");
            Console.WriteLine(string.Empty);
            Console.WriteLine("--- DOMAIN.JSON ---");
            Console.WriteLine(_dynamicDomainJson);
            Console.WriteLine(string.Empty);
            Console.WriteLine($"--- UHMEP KEY: {_uhmepKey.Kid}, x5t#S256: {ComputeX5tS256(_generatedCertificates[_uhmepKey.Kid])} ---");
            Console.WriteLine($"--- PSEUDO KEY: {_pseudoKey.Kid}, x5t#S256: {ComputeX5tS256(_generatedCertificates[_pseudoKey.Kid])} ---");
        }
        catch (Exception ex)
        {
            Assert.Fail("An error occurred while generating or displaying test data: " + ex.Message);
        }
    }

    // Make helper methods static
    private static (string Kid, RSA Rsa) GenerateRsaKeyAndCertificate(string appId, string cbeNumber)
    {
        var rsa = RSA.Create(2048);
        var kid = $"E{GenerateNumericKid()}"; // Generate a KID like the format in domain.json

        _generatedRsaKeys[kid] = rsa;

        // Generate a self-signed certificate with proper subject name
        var cert = GenerateSelfSignedCertificate($"{appId}_{cbeNumber}", rsa);
        _generatedCertificates[kid] = Convert.ToBase64String(cert.RawData);

        return (kid, rsa);
    }

    private static X509Certificate2 GenerateSelfSignedCertificate(string subjectName, RSA rsa)
    {
        // Use standard X500 attributes only
        var distinguishedName = new X500DistinguishedName($"CN={subjectName}, O=Federal Government, C=BE");

        var request = new CertificateRequest(
            distinguishedName,
            rsa,
            HashAlgorithmName.SHA256,
            RSASignaturePadding.Pkcs1);

        request.CertificateExtensions.Add(
            new X509KeyUsageExtension(X509KeyUsageFlags.DataEncipherment, false));

        var certificate = request.CreateSelfSigned(
            DateTimeOffset.Now,
            DateTimeOffset.Now.AddYears(3));

        return certificate;
    }

    private static string GenerateNumericKid()
    {
        // Generate a large numeric ID similar to the ones in domain.json
        var random = new Random();
        var bytes = new byte[10];
        random.NextBytes(bytes);
        return string.Join(string.Empty, bytes.Select(b => b.ToString()));
    }

    private static byte[] GenerateSymmetricKey()
    {
        using var aes = Aes.Create();
        aes.KeySize = 256;
        aes.GenerateKey();
        return aes.Key;
    }

    // Keep other static helper methods...
    private static string CreatePrivateJwk(RSA rsa, string kid)
    {
        var parameters = rsa.ExportParameters(true);
        var certBase64 = _generatedCertificates[kid];
        var x5tS256Hash = ComputeX5tS256(certBase64);

        // Create the JWK as a dictionary to have full control over property names
        var jwkDict = new Dictionary<string, object>
        {
            ["kty"] = "RSA",
            ["use"] = "enc",
            ["kid"] = kid,
            ["e"] = Base64UrlEncoder.Encode(parameters.Exponent),
            ["n"] = Base64UrlEncoder.Encode(parameters.Modulus),
            ["d"] = Base64UrlEncoder.Encode(parameters.D),
            ["p"] = Base64UrlEncoder.Encode(parameters.P),
            ["q"] = Base64UrlEncoder.Encode(parameters.Q),
            ["dp"] = Base64UrlEncoder.Encode(parameters.DP),
            ["dq"] = Base64UrlEncoder.Encode(parameters.DQ),
            ["qi"] = Base64UrlEncoder.Encode(parameters.InverseQ),
            ["x5c"] = new[] { certBase64 },
            ["x5t#S256"] = x5tS256Hash, // This will be properly serialized with the # character
            ["nbf"] = DateTimeOffset.Now.ToUnixTimeSeconds(),
            ["exp"] = DateTimeOffset.Now.AddYears(2).ToUnixTimeSeconds()
        };

        var options = new JsonSerializerOptions { WriteIndented = true };
        return JsonSerializer.Serialize(jwkDict, options);
    }

    private static string ComputeX5tS256(string certBase64)
    {
        var certBytes = Convert.FromBase64String(certBase64);
        using var sha256 = SHA256.Create();
        var hash = sha256.ComputeHash(certBytes);
        return Base64UrlEncoder.Encode(hash);
    }

    private static string CreateJwks((string Kid, RSA Rsa) uhmepKey, (string Kid, RSA Rsa) pseudoKey)
    {
        var uhmepParameters = uhmepKey.Rsa.ExportParameters(false);
        var pseudoParameters = pseudoKey.Rsa.ExportParameters(false);

        var jwk = new
        {
            keys = new[]
            {
                new
                {
                    kty = "RSA",
                    use = "enc",
                    kid = uhmepKey.Kid,  // Explicitly name the property
                    e = Base64UrlEncoder.Encode(uhmepParameters.Exponent),
                    n = Base64UrlEncoder.Encode(uhmepParameters.Modulus),
                    x5c = new[] { _generatedCertificates[uhmepKey.Kid] },
                    @x5tS256 = ComputeX5tS256(_generatedCertificates[uhmepKey.Kid]) // Use @ prefix for property name with #
                },
                new
                {
                    kty = "RSA",
                    use = "enc",
                    kid = pseudoKey.Kid,  // Explicitly name the property
                    e = Base64UrlEncoder.Encode(pseudoParameters.Exponent),
                    n = Base64UrlEncoder.Encode(pseudoParameters.Modulus),
                    x5c = new[] { _generatedCertificates[pseudoKey.Kid] },
                    @x5tS256 = ComputeX5tS256(_generatedCertificates[pseudoKey.Kid]) // Use @ prefix for property name with #
                }
            }
        };

        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = null // Ensure property names are preserved as-is
        };

        var json = JsonSerializer.Serialize(jwk, options);

        // Verify the JSON contains x5t#S256 (the # gets encoded properly in JSON)
        json = json.Replace("x5tS256", "x5t#S256");

        return json;
    }

    private static string GenerateDomainJson(
        (string Kid, RSA Rsa) uhmepKey,
        (string Kid, RSA Rsa) pseudoKey,
        byte[] secretKey1,
        byte[] secretKey2)
    {
        var secretKeys = new List<object>();

        // Create first secret key (inactive)
        var secretKeyKid1 = Guid.NewGuid().ToString();
        var jwe1 = EncryptSecretKey(secretKey1, uhmepKey, pseudoKey);
        secretKeys.Add(new
        {
            kid = secretKeyKid1,
            active = false,
            encoded = jwe1
        });

        // Create second secret key (active)
        var secretKeyKid2 = Guid.NewGuid().ToString();
        var jwe2 = EncryptSecretKey(secretKey2, uhmepKey, pseudoKey);
        secretKeys.Add(new
        {
            kid = secretKeyKid2,
            active = true,
            encoded = jwe2
        });

        var domain = new
        {
            audience = "https://api-acpt.ehealth.fgov.be/pseudo/v1/domains/uhmep_v1",
            bufferSize = 8,
            jku = new[]
            {
                "https://api-acpt.ehealth.fgov.be/etee/v1/pubKeys/cacerts/jwks?identifier=0406798006&type=CBE&applicationIdentifier=UHMEP&use=enc",
                "https://api-acpt.ehealth.fgov.be/etee/v1/pubKeys/cacerts/jwks?identifier=0809394427&type=CBE&applicationIdentifier=PSEUDONYMISATION&use=enc"
            },
            timeToLiveInTransit = "PT10M",
            secretKeys,
            accessRule = GenerateAccessRule(),
            domain = "uhmep_v1",
            desc = "Referral prescriptions",
            crv = "P-521"
        };

        return JsonSerializer.Serialize(domain, new JsonSerializerOptions { WriteIndented = true });
    }

    private static object EncryptSecretKey(byte[] secretKey, (string Kid, RSA Rsa) uhmepKey, (string Kid, RSA Rsa) pseudoKey)
    {
        // Create the symmetric key JWK
        var symmetricJwk = new
        {
            kty = "oct",
            k = Base64UrlEncoder.Encode(secretKey),
            alg = "A256GCM"
        };

        var payload = JsonSerializer.Serialize(symmetricJwk);
        var payloadBytes = Encoding.UTF8.GetBytes(payload);

        // Generate IV and encrypt
        var iv = new byte[12];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(iv);
        }

        // Create AAD (Additional Authenticated Data) from protected header
        var protectedHeader = new { enc = "A256GCM" };
        var protectedHeaderJson = JsonSerializer.Serialize(protectedHeader);
        var protectedHeaderBase64 = Base64UrlEncoder.Encode(Encoding.UTF8.GetBytes(protectedHeaderJson));
        var aad = Encoding.ASCII.GetBytes(protectedHeaderBase64);

        // Use BouncyCastle's GcmBlockCipher for AES-GCM encryption (works on all platforms)
        var cipher = new Org.BouncyCastle.Crypto.Modes.GcmBlockCipher(new Org.BouncyCastle.Crypto.Engines.AesEngine());
        var parameters = new Org.BouncyCastle.Crypto.Parameters.AeadParameters(
            new Org.BouncyCastle.Crypto.Parameters.KeyParameter(secretKey),
            128, // tag size in bits
            iv,
            aad);

        cipher.Init(true, parameters); // true = encryption mode

        var ciphertextWithTag = new byte[cipher.GetOutputSize(payloadBytes.Length)];
        int len = cipher.ProcessBytes(payloadBytes, 0, payloadBytes.Length, ciphertextWithTag, 0);
        cipher.DoFinal(ciphertextWithTag, len);

        // Separate ciphertext and tag
        var ciphertext = new byte[payloadBytes.Length];
        var tag = new byte[16]; // 128-bit tag

        Array.Copy(ciphertextWithTag, 0, ciphertext, 0, payloadBytes.Length);
        Array.Copy(ciphertextWithTag, payloadBytes.Length, tag, 0, tag.Length);

        // Create recipients
        var recipients = new List<object>
        {
            CreateRecipient(
                uhmepKey,
                secretKey,
                "RSA-OAEP-256",
                "https://api-acpt.ehealth.fgov.be/etee/v1/pubKeys/cacerts/jwks?identifier=0406798006&type=CBE&applicationIdentifier=UHMEP&use=enc"),

            CreateRecipient(
                pseudoKey,
                secretKey,
                "RSA-OAEP-256",
                "https://api-acpt.ehealth.fgov.be/etee/v1/pubKeys/cacerts/jwks?identifier=0809394427&type=CBE&applicationIdentifier=PSEUDONYMISATION&use=enc")
        };

        return new
        {
            @protected = protectedHeaderBase64,
            iv = Base64UrlEncoder.Encode(iv),
            ciphertext = Base64UrlEncoder.Encode(ciphertext),
            tag = Base64UrlEncoder.Encode(tag),
            recipients
        };
    }

    private static object CreateRecipient((string Kid, RSA Rsa) keyPair, byte[] cek, string alg, string jku)
    {
        // Encrypt the CEK with RSA-OAEP
        var encryptedKey = keyPair.Rsa.Encrypt(cek, RSAEncryptionPadding.OaepSHA256);

        return new
        {
            encrypted_key = Base64UrlEncoder.Encode(encryptedKey),
            header = new
            {
                jku,
                alg,
                kid = keyPair.Kid
            }
        };
    }

    private static object GenerateAccessRule()
    {
        return new
        {
            domain = "uhmep_v1",
            type = "custom",
            details = new[]
            {
                new
                {
                    operation = "pseudonymize",
                    userGroups = new[]
                    {
                        new
                        {
                            claims = new[]
                            {
                                new
                                {
                                    path = "$.resource_access.nihdi-uhmep-api.roles[*]",
                                    value = "pseudo"
                                }
                            },
                            name = "uhmep_pseudo",
                            description = "UHMEP pseudo"
                        }
                    }
                },
                new
                {
                    operation = "identify",
                    userGroups = new[]
                    {
                        new
                        {
                            claims = new[]
                            {
                                new
                                {
                                    path = "$.resource_access.nihdi-uhmep-api.roles[*]",
                                    value = "pseudo"
                                }
                            },
                            name = "uhmep_pseudo",
                            description = "UHMEP pseudo"
                        }
                    }
                }
            }
        };
    }

    // Test methods remain as instance methods
    [TestMethod]
    public async Task Test()
    {
        var ssin = "012345678910";

        if (_pseudonymisationHelper == null)
        {
            throw new InvalidOperationException("pseudonymisationHelper is null");
        }

        var value = await _pseudonymisationHelper!
            .GetDomain("uhmep_v1")
            .ContinueWith(t => t.Result?.ValueFactory.From(ssin).Pseudonymize())
            .ContinueWith(t => t?.Result?.Result?.Identify());

        var result = value?.Result;
        var actualResultString = result?.AsString();

        Assert.AreEqual(ssin, actualResultString);
    }

    [TestMethod]
    public void Test_Multiple_Creates_Collection_Of_Pseudonyms()
    {
        // Arrange
        var ssins = new[] { "012345678910", "987654321012", "123456789012" };

        if (_pseudonymisationHelper == null)
        {
            throw new InvalidOperationException("pseudonymisationHelper is null");
        }

        var domain = _pseudonymisationHelper.GetDomain("uhmep_v1").Result;
        if (domain == null)
        {
            throw new InvalidOperationException("Domain is null");
        }

        var valueFactory = domain.ValueFactory;

        // Act
        var multipleValues = valueFactory.Multiple(ssins.Select(ssin => valueFactory.From(ssin)).ToList());

        // Assert
        Assert.AreEqual(ssins.Length, multipleValues.Size(), "The size of the collection should match the input count.");

        for (int i = 0; i < ssins.Length; i++)
        {
            var pseudonym = multipleValues[i];
            Assert.IsNotNull(pseudonym, $"Pseudonym at index {i} should not be null.");
            Assert.AreEqual(ssins[i], pseudonym.AsString(), $"Pseudonym at index {i} should match the input value.");
        }
    }

    [TestMethod]
    public async Task Test_MultipleValues_Pseudonymize_And_Identify()
    {
        // Arrange
        var ssins = new[] { "012345678910", "987654321012", "123456789012" };

        if (_pseudonymisationHelper == null)
        {
            throw new InvalidOperationException("pseudonymisationHelper is null");
        }

        var domain = _pseudonymisationHelper.GetDomain("uhmep_v1").Result;
        if (domain == null)
        {
            throw new InvalidOperationException("Domain is null");
        }

        var valueFactory = domain.ValueFactory;

        // Act
        // Create multiple values
        IMultipleValue multipleValues = valueFactory.Multiple(ssins.Select(ssin => valueFactory.From(ssin)).ToList());
        Assert.AreEqual(ssins.Length, multipleValues.Size(), "The size of the collection should match the input count.");

        // Pseudonymize the values to get pseudonyms in transit
        var multiplePseudonymsInTransit = await multipleValues.Pseudonymize();

        Assert.AreEqual(ssins.Length, multiplePseudonymsInTransit.Size(), "The size of the pseudonyms in transit should match the input count.");
        // Identify the pseudonyms back to original values
        var identifiedValues = await multiplePseudonymsInTransit.Identify();
        Assert.AreEqual(ssins.Length, identifiedValues.Size(), "The size of the identified values should match the input count.");

        // Assert - Verify the identified values match the original SSINs
        for (int i = 0; i < ssins.Length; i++)
        {
            var value = identifiedValues[i];
            Assert.IsNotNull(value, $"Identified value at index {i} should not be null.");
            Assert.AreEqual(ssins[i], value.AsString(), $"Identified value at index {i} should match the original SSIN.");
        }
    }
}
