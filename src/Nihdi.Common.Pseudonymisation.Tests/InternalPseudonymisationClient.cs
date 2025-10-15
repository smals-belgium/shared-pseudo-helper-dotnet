// <copyright file="InternalPseudonymisationClient.cs" company="Riziv-Inami">
// Copyright (c) Riziv-Inami. All rights reserved.
// </copyright>

namespace Nihdi.Common.Pseudonymisation.Tests;

using Newtonsoft.Json.Linq;
using Nihdi.Common.Pseudonymisation;

internal class InternalPseudonymisationClient : IPseudonymisationClient
{
    private readonly PseudonymisationHelper? _internalPseudonymisationHelper;
    private readonly IDateTimeService _dateTimeService;
    private readonly string _domainJson;

    public InternalPseudonymisationClient(PseudonymisationHelper? pseudonymisationHelper, IDateTimeService? dateTimeService = null, string? domainJson = null)
    {
        _internalPseudonymisationHelper = pseudonymisationHelper;
        this._dateTimeService = dateTimeService ?? new DateTimeService();
        this._domainJson = domainJson ?? string.Empty;
    }

    public Task<string> ConvertTo(string fromDomainKey, string toDomainKey, string payload)
    {
        return null!;
    }

    public Task<string> GetDomain(string domainKey)
    {
        if (string.IsNullOrEmpty(_domainJson))
        {
            throw new InvalidOperationException("domainJson cannot be null or empty");
        }

        return Task.FromResult(_domainJson);
    }

    public async Task<string> Identify(string domainKey, string payload)
    {
        var now = _dateTimeService.Now;
        var jsonObject = JObject.Parse(payload);

        return await _internalPseudonymisationHelper!
            .GetDomain("uhmep_v1")
            .ContinueWith(uhmepV1 =>
         {
             var x = jsonObject["x"]?.ToString() ?? throw new InvalidOperationException("x cannot be null");
             var y = jsonObject["y"]?.ToString() ?? throw new InvalidOperationException("y cannot be null");
             var transitInfo = jsonObject["transitInfo"]?.ToString() ?? throw new InvalidOperationException("transitInfo cannot be null");
             var pseudonymInTransit = uhmepV1.Result?.PseudonymInTransitFactory.FromXYAndTransitInfo(x, y, transitInfo);
             var pseudonymAtRest = pseudonymInTransit?.AtRest();

             var response = new JObject();
             response.Add("id", new JValue(Guid.NewGuid().ToString()));
             response.Add("domain", new JValue(domainKey));
             response.Add("crv", new JValue("P-521"));
             response.Add("iat", new JValue(now.ToUnixTimeSeconds()));
             response.Add("exp", new JValue(now.AddHours(1).ToUnixTimeSeconds()));
             response.Add("x", new JValue(pseudonymAtRest?.X()));
             response.Add("y", new JValue(pseudonymAtRest?.Y()));

             return response.ToString();
         });
    }

    public Task<string> IdentifyMultiple(string domainKey, string payload)
    {
        var now = _dateTimeService.Now;
        var jsonObject = JObject.Parse(payload);
        var inputs = jsonObject["inputs"] as JArray;

        if (inputs == null)
        {
            throw new InvalidOperationException("inputs is null");
        }

        if (_internalPseudonymisationHelper == null)
        {
            throw new InvalidOperationException("internalPseudonymisationHelper is null");
        }

        var outputs = new JArray();
        var response = new JObject();
        response.Add("id", new JValue(Guid.NewGuid().ToString()));
        response.Add("domain", new JValue(domainKey));
        response.Add("outputs", outputs);

        for (int i = 0; i < inputs.Count; i++)
        {
            var input = JObject.Parse(inputs[i].ToString());
            var x = input["x"]?.ToString() ?? throw new InvalidOperationException("x cannot be null");
            var y = input["y"]?.ToString() ?? throw new InvalidOperationException("y cannot be null");
            var transitInfo = input["transitInfo"]?.ToString() ?? throw new InvalidOperationException("transitInfo cannot be null");

            _internalPseudonymisationHelper
                .GetDomain("uhmep_v1")
                .ContinueWith(uhmepV1 =>
                {
                    var pseudonymInTransit = uhmepV1.Result?.PseudonymInTransitFactory.FromXYAndTransitInfo(x, y, transitInfo);
                    var pseudonymAtRest = pseudonymInTransit?.AtRest();
                    var value = ((Pseudonym?)pseudonymAtRest)?.AsValue();

                    var outputObj = new JObject();
                    outputObj.Add("id", new JValue(Guid.NewGuid().ToString()));
                    outputObj.Add("domain", new JValue(domainKey));
                    outputObj.Add("iat", new JValue(now.ToUnixTimeSeconds()));
                    outputObj.Add("exp", new JValue(now.AddHours(1).ToUnixTimeSeconds()));
                    outputObj.Add("x", new JValue(value?.X()));
                    outputObj.Add("y", new JValue(value?.Y()));

                    outputs.Add(outputObj);
                }).Wait();
        }

        return Task.FromResult(response.ToString());
    }

    public async Task<string> Pseudonymize(string domainKey, string payload)
    {
        var now = _dateTimeService.Now;
        var jsonObject = JObject.Parse(payload);

        if (_internalPseudonymisationHelper == null)
        {
            throw new InvalidOperationException("internalPseudonymisationHelper is null");
        }

        return await _internalPseudonymisationHelper
            .GetDomain("uhmep_v1")
            .ContinueWith(uhmepV1 =>
            {
                var pseudonym = uhmepV1?.Result?.PseudonymFactory.FromXy(jsonObject["x"]?.ToString()!, jsonObject["y"]?.ToString()!);

                var fakePseudonym = uhmepV1?.Result?.PseudonymFactory.FromXy(pseudonym?.X()!, pseudonym?.Y()!);
                var pseudonymInTransit = fakePseudonym?.InTransit();

                var response = new JObject();
                response.Add("id", new JValue(Guid.NewGuid().ToString()));
                response.Add("domain", new JValue(domainKey));
                response.Add("crv", new JValue("P-521"));
                response.Add("iat", new JValue(now.ToUnixTimeSeconds()));
                response.Add("exp", new JValue(now.AddHours(1).ToUnixTimeSeconds()));
                response.Add("x", new JValue(pseudonymInTransit?.X()));
                response.Add("y", new JValue(pseudonymInTransit?.Y()));
                response.Add("transitInfo", new JValue(pseudonymInTransit?.GetTransitInfo().AsString()));
                return response.ToString();
            });
    }

    public Task<string> PseudonymizeMultiple(string domainKey, string payload)
    {
        var now = _dateTimeService.Now;
        var jsonObject = JObject.Parse(payload);
        var inputs = jsonObject["inputs"] as JArray;

        if (inputs == null)
        {
            throw new InvalidOperationException("inputs is null");
        }

        if (_internalPseudonymisationHelper == null)
        {
            throw new InvalidOperationException("internalPseudonymisationHelper is null");
        }

        // Use JObject for outputs, NOT JArray
        var outputs = new JObject();
        var response = new JObject();
        response.Add("id", new JValue(Guid.NewGuid().ToString()));
        response.Add("domain", new JValue(domainKey));
        response.Add("outputs", outputs);

        for (int i = 0; i < inputs.Count; i++)
        {
            var input = inputs[i];
            _internalPseudonymisationHelper
                .GetDomain("uhmep_v1")
                .ContinueWith(uhmepV1 =>
                {
                    var inputObj = JObject.Parse(input.ToString());
                    var pseudonym = uhmepV1.Result?.PseudonymFactory.FromXy(inputObj["x"]?.ToString()!, inputObj["y"]?.ToString()!);
                    var fakePseudonym = uhmepV1.Result?.PseudonymFactory.FromXy(pseudonym?.X()!, pseudonym?.Y()!);
                    var pseudonymInTransit = fakePseudonym?.InTransit();

                    var outputObj = new JObject();
                    outputObj.Add("id", new JValue(Guid.NewGuid().ToString()));
                    outputObj.Add("domain", new JValue(domainKey));
                    outputObj.Add("crv", new JValue("P-521"));
                    outputObj.Add("iat", new JValue(now.ToUnixTimeSeconds()));
                    outputObj.Add("exp", new JValue(now.AddHours(1).ToUnixTimeSeconds()));
                    outputObj.Add("x", new JValue(pseudonymInTransit?.X()));
                    outputObj.Add("y", new JValue(pseudonymInTransit?.Y()));
                    outputObj.Add("transitInfo", new JValue(pseudonymInTransit?.GetTransitInfo().AsString()));

                    // Use numeric string as key for JObject
                    outputs.Add(i.ToString(), outputObj);
                }).Wait();
        }

        return Task.FromResult(response.ToString());
    }

    public Task<string> ConvertMultipleTo(string fromDomainKey, string toDomainKey, string payload)
    {
        throw new NotImplementedException();
    }
}