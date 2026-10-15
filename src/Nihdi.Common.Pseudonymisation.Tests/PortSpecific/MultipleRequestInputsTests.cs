// <copyright file="MultipleRequestInputsTests.cs" company="Riziv-Inami">
// Copyright (c) Riziv-Inami. All rights reserved.
// </copyright>

using Newtonsoft.Json.Linq;

namespace Nihdi.Common.Pseudonymisation.Tests.PortSpecific;

/// <summary>
/// Tests on the request body sent to the `multiple` endpoints: `inputs` is a JSON array with one
/// JSON object per element, and the whole body is compact.
/// <para>
/// Tests in this folder exist only in the C# port.
/// </para>
/// </summary>
[TestClass]
public class MultipleRequestInputsTests
{
    private const string _domainKey = "test";
    private const string _targetDomainKey = "target";

    private const string _multipleValuePseudonymize = "MultipleValue.Pseudonymize";
    private const string _multiplePseudonymConvertTo = "MultiplePseudonym.ConvertTo";
    private const string _multiplePseudonymInTransitConvertTo = "MultiplePseudonymInTransit.ConvertTo";
    private const string _multiplePseudonymInTransitIdentify = "MultiplePseudonymInTransit.Identify";

    // Coordinates of a valid P-521 point, taken from the existing tests.
    private const string _x =
        "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAMDEy" +
        "MzQ1Njc4OTEwDAAAAAAAAAAB";

    private const string _y =
        "ABxCeSYmGgumIwOuFyKmK1l8bilTjL+NK" +
        "Elpegi8mcsn3QB6grG5PHpgD8XCTPTGhDrT6HsGuEkEOVwZ23maSliV";

    private const string _transitInfoRaw =
        "eyJhbGciOiJkaXIiLCJlbmMiOiJBMjU2R0NNIiwia2lkIjoiMjAyMi0xMiIsImF1" +
        "ZCI6Imh0dHBzOi8vYXBpLWludC5laGVhbHRoLmZnb3YuYmUvcHNldWRvL3YxL2Rv" +
        "bWFpbnMvdWhtZXBfdjEifQ..osrl3KS4nkheJvcJ.pXN4Asfg8RGtsoV529YoFRW" +
        "P_XSXUViR-wxuvwYTvN9fMSDksq7qZMmmqDstyGyOidHKHrVvtqB0PFrek71P4K8" +
        "Rp0rDuvAc6RC2cbdwV08Ksw6t3Wf72H8c8QDKGKmYb84z_oH8TMnY26cAm0nC2Hb" +
        "18H-SXTh8xFXe3DK8y06wx4rAAXFZGsXayloJ6oweux_tvKQ4NSWi3gzhjVi0g-q" +
        "WR9TYZNj9NNyU9eeSDk9UsXJ8cugpvStu6oFOCbW4520fl6h5oaJ7Rye3.IEd7uL" +
        "w-ICHAwqIzfrjOFw";

    [DataTestMethod]
    [DataRow(_multipleValuePseudonymize)]
    [DataRow(_multiplePseudonymConvertTo)]
    [DataRow(_multiplePseudonymInTransitConvertTo)]
    [DataRow(_multiplePseudonymInTransitIdentify)]
    public void MultipleRequest_SendsEachInputAsAJsonObject(string operation)
    {
        var payload = SendTwoElements(operation);

        var inputs = JObject.Parse(payload)["inputs"] as JArray;
        Assert.IsNotNull(inputs, $"`inputs` should be a JSON array: {payload}");
        Assert.AreEqual(2, inputs.Count);

        // An input sent as a string holds the JSON of the element as escaped text, and not the
        // element itself.
        foreach (var input in inputs)
        {
            Assert.AreEqual(
                JTokenType.Object,
                input.Type,
                $"Each input should be a JSON object: {payload}");
        }
    }

    [DataTestMethod]
    [DataRow(_multipleValuePseudonymize)]
    [DataRow(_multiplePseudonymConvertTo)]
    [DataRow(_multiplePseudonymInTransitConvertTo)]
    [DataRow(_multiplePseudonymInTransitIdentify)]
    public void MultipleRequest_ProducesCompactJson(string operation)
    {
        var payload = SendTwoElements(operation);

        // JObject indents its output by default, which would make every request to eHealth larger.
        Assert.IsFalse(payload.Contains('\n'), $"The payload should be compact but contains a newline: {payload}");
    }

    /// <summary>
    /// Runs the given operation on a collection of two elements, and returns the request body
    /// received by the fake client.
    /// </summary>
    private static string SendTwoElements(string operation)
    {
        var client = new RecordingPseudonymisationClient();
        var domain = CreateDomain(_domainKey, client);
        var toDomain = CreateDomain(_targetDomainKey, client);

        Task? call;
        switch (operation)
        {
            case _multipleValuePseudonymize:
                var value = domain.ValueFactory.From("012345678910");
                call = new MultipleValue(domain, new List<object> { value, value }).Pseudonymize();
                break;

            case _multiplePseudonymConvertTo:
                var pseudonym = domain.PseudonymFactory.FromXy(_x, _y);
                IMultiplePseudonym pseudonyms =
                    new MultiplePseudonym(domain, new List<IPseudonym> { pseudonym, pseudonym });
                call = pseudonyms.ConvertTo(toDomain);
                break;

            case _multiplePseudonymInTransitConvertTo:
                call = CreatePseudonymsInTransit(domain).ConvertTo(toDomain);
                break;

            case _multiplePseudonymInTransitIdentify:
                call = CreatePseudonymsInTransit(domain).Identify();
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(operation), operation, null);
        }

        // The call fails because the fake client always fails, but the payload is recorded first,
        // and the payload is what these tests check.
        try
        {
            call!.Wait();
        }
        catch (AggregateException)
        {
            // Expected: the fake client always fails.
        }

        Assert.AreEqual(1, client.Payloads.Count);
        return client.Payloads[0];
    }

    private static IMultiplePseudonymInTransit CreatePseudonymsInTransit(Domain domain)
    {
        var pseudonymInTransit = domain.PseudonymInTransitFactory.FromXYAndTransitInfo(_x, _y, _transitInfoRaw);
        return new MultiplePseudonymInTransit(domain, new List<object> { pseudonymInTransit, pseudonymInTransit });
    }

    private static Domain CreateDomain(string key, IPseudonymisationClient client)
    {
        // `crv` must be set, because building the request payload needs it.
        return new Domain(
            key,
            "P-521",
            EcCurveNames.GetCurveFromString("P-521"),
            null,
            8,
            null,
            null,
            null,
            null,
            client,
            null);
    }
}
