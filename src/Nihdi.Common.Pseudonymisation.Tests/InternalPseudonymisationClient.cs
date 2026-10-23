// <copyright file="InternalPseudonymisationClient.cs" company="Riziv-Inami">
// Copyright (c) Riziv-Inami. All rights reserved.
// </copyright>

namespace Nihdi.Common.Pseudonymisation.Tests;

using System.Text.Json.Nodes;
using Nihdi.Common.Pseudonymisation;

internal class InternalPseudonymisationClient : IPseudonymisationClient
{
    private readonly PseudonymisationHelper? _internalPseudonymisationHelper;
    private readonly TimeProvider _timeProvider;
    private readonly string _domainJson;

    public InternalPseudonymisationClient(PseudonymisationHelper? pseudonymisationHelper, TimeProvider? timeProvider = null, string? domainJson = null)
    {
        _internalPseudonymisationHelper = pseudonymisationHelper;
        this._timeProvider = timeProvider ?? TimeProvider.System;
        this._domainJson = domainJson ?? string.Empty;
    }

    public Task<string> ConvertTo(string fromDomainKey, string toDomainKey, string payload, CancellationToken cancellationToken)
    {
        return null!;
    }

    public Task<string> GetDomain(string domainKey, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(_domainJson))
        {
            throw new InvalidOperationException("domainJson cannot be null or empty");
        }

        return Task.FromResult(_domainJson);
    }

    public async Task<string> Identify(string domainKey, string payload, CancellationToken cancellationToken)
    {
        var now = _timeProvider.GetUtcNow();
        var jsonObject = JsonNode.Parse(payload)!.AsObject();

        return await _internalPseudonymisationHelper!
            .GetDomain("uhmep_v1")
            .ContinueWith(uhmepV1 =>
         {
             var x = jsonObject["x"]?.ToString() ?? throw new InvalidOperationException("x cannot be null");
             var y = jsonObject["y"]?.ToString() ?? throw new InvalidOperationException("y cannot be null");
             var transitInfo = jsonObject["transitInfo"]?.ToString() ?? throw new InvalidOperationException("transitInfo cannot be null");
             var pseudonymInTransit = uhmepV1.Result?.PseudonymInTransitFactory.FromXYAndTransitInfo(x, y, transitInfo);
             var pseudonymAtRest = pseudonymInTransit?.AtRest();

             var response = new JsonObject();
             response.Add("id", Guid.NewGuid().ToString());
             response.Add("domain", domainKey);
             response.Add("crv", "P-521");
             response.Add("iat", now.ToUnixTimeSeconds());
             response.Add("exp", now.AddHours(1).ToUnixTimeSeconds());
             response.Add("x", pseudonymAtRest?.X());
             response.Add("y", pseudonymAtRest?.Y());

             return response.ToJsonString();
         });
    }

    public Task<string> IdentifyMultiple(string domainKey, string payload, CancellationToken cancellationToken)
    {
        var now = _timeProvider.GetUtcNow();
        var jsonObject = JsonNode.Parse(payload)!.AsObject();
        var inputs = jsonObject["inputs"] as JsonArray;

        if (inputs == null)
        {
            throw new InvalidOperationException("inputs is null");
        }

        if (_internalPseudonymisationHelper == null)
        {
            throw new InvalidOperationException("internalPseudonymisationHelper is null");
        }

        var outputs = new JsonArray();
        var response = new JsonObject();
        response.Add("id", Guid.NewGuid().ToString());
        response.Add("domain", domainKey);
        response.Add("outputs", outputs);

        for (int i = 0; i < inputs.Count; i++)
        {
            var input = (JsonObject)inputs[i]!;
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

                    var outputObj = new JsonObject();
                    outputObj.Add("id", Guid.NewGuid().ToString());
                    outputObj.Add("domain", domainKey);
                    outputObj.Add("iat", now.ToUnixTimeSeconds());
                    outputObj.Add("exp", now.AddHours(1).ToUnixTimeSeconds());
                    outputObj.Add("x", value?.X());
                    outputObj.Add("y", value?.Y());

                    outputs.Add(outputObj);
                }).Wait();
        }

        return Task.FromResult(response.ToJsonString());
    }

    public async Task<string> Pseudonymize(string domainKey, string payload, CancellationToken cancellationToken)
    {
        var now = _timeProvider.GetUtcNow();
        var jsonObject = JsonNode.Parse(payload)!.AsObject();

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

                var response = new JsonObject();
                response.Add("id", Guid.NewGuid().ToString());
                response.Add("domain", domainKey);
                response.Add("crv", "P-521");
                response.Add("iat", now.ToUnixTimeSeconds());
                response.Add("exp", now.AddHours(1).ToUnixTimeSeconds());
                response.Add("x", pseudonymInTransit?.X());
                response.Add("y", pseudonymInTransit?.Y());
                response.Add("transitInfo", pseudonymInTransit?.GetTransitInfo().AsString());
                return response.ToJsonString();
            });
    }

    public Task<string> PseudonymizeMultiple(string domainKey, string payload, CancellationToken cancellationToken)
    {
        var now = _timeProvider.GetUtcNow();
        var jsonObject = JsonNode.Parse(payload)!.AsObject();
        var inputs = jsonObject["inputs"] as JsonArray;

        if (inputs == null)
        {
            throw new InvalidOperationException("inputs is null");
        }

        if (_internalPseudonymisationHelper == null)
        {
            throw new InvalidOperationException("internalPseudonymisationHelper is null");
        }

        var outputs = new JsonArray();
        var response = new JsonObject();
        response.Add("id", Guid.NewGuid().ToString());
        response.Add("domain", domainKey);
        response.Add("outputs", outputs);

        for (int i = 0; i < inputs.Count; i++)
        {
            var input = inputs[i];
            _internalPseudonymisationHelper
                .GetDomain("uhmep_v1")
                .ContinueWith(uhmepV1 =>
                {
                    var inputObj = (JsonObject)input!;
                    var pseudonym = uhmepV1.Result?.PseudonymFactory.FromXy(inputObj["x"]?.ToString()!, inputObj["y"]?.ToString()!);
                    var fakePseudonym = uhmepV1.Result?.PseudonymFactory.FromXy(pseudonym?.X()!, pseudonym?.Y()!);
                    var pseudonymInTransit = fakePseudonym?.InTransit();

                    var outputObj = new JsonObject();
                    outputObj.Add("id", Guid.NewGuid().ToString());
                    outputObj.Add("domain", domainKey);
                    outputObj.Add("crv", "P-521");
                    outputObj.Add("iat", now.ToUnixTimeSeconds());
                    outputObj.Add("exp", now.AddHours(1).ToUnixTimeSeconds());
                    outputObj.Add("x", pseudonymInTransit?.X());
                    outputObj.Add("y", pseudonymInTransit?.Y());
                    outputObj.Add("transitInfo", pseudonymInTransit?.GetTransitInfo().AsString());

                    outputs.Add(outputObj);
                }).Wait();
        }

        return Task.FromResult(response.ToJsonString());
    }

    public Task<string> ConvertMultipleTo(string fromDomainKey, string toDomainKey, string payload, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}