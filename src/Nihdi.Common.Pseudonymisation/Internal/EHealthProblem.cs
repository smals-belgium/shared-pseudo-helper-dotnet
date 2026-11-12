// <copyright file="EHealthProblem.cs" company="Riziv-Inami">
// Copyright (c) Riziv-Inami. All rights reserved.
// </copyright>

namespace Nihdi.Common.Pseudonymisation.Internal;

using System.Text.Json.Nodes;

/// <summary>
/// Implementation of <see cref="IEHealthProblem"/> representing an eHealth problem.
/// </summary>
internal class EHealthProblem : IEHealthProblem
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EHealthProblem"/> class.
    /// </summary>
    /// <param name="type">The type of the problem.</param>
    /// <param name="title">The title of the problem.</param>
    /// <param name="status">The status code associated with the problem.</param>
    /// <param name="detail">The detail of the problem.</param>
    public EHealthProblem(string type, string title, string status, string detail)
    {
        Type = type;
        Title = title;
        Status = status;
        Detail = detail;
    }

    /// <inheritdoc/>
    public string Type
    {
        get;
    }

    /// <inheritdoc/>
    public string Title
    {
        get;
    }

    /// <inheritdoc/>
    public string Status
    {
        get;
    }

    /// <inheritdoc/>
    public string Detail
    {
        get;
    }

    /// <summary>
    /// Creates an <see cref="EHealthProblem"/> instance from a JSON response.
    /// </summary>
    /// <param name="response">The JSON response from the eHealth API.</param>
    /// <returns>An instance of <see cref="EHealthProblem"/>.</returns>
    public static EHealthProblem FromResponse(JsonObject response)
    {
        return response.ContainsKey("type")
            ? new EHealthProblem(
                response["type"]?.ToString() ?? string.Empty,
                response["title"]?.ToString() ?? string.Empty,
                response["status"]?.ToString() ?? string.Empty,
                response["detail"]?.ToString() ?? string.Empty)
            : new EHealthProblem(
                "urn:problem-type:smals:pseudo-helper:unexpected-result",
                "Unexpected Result",
                "undefined",
                $"Unable to convert the response from eHealth into a EHealthProblem. Response was: \n{JsonHelper.ToIndentedString(response)}");
    }

    /// <summary>
    /// Creates an <see cref="EHealthProblem"/> instance for a response that is valid JSON, but not
    /// an object (an array, `null`, a number...).
    /// </summary>
    /// <param name="rawResponse">The raw response, quoted as it was received.</param>
    /// <returns>An instance of <see cref="EHealthProblem"/>.</returns>
    public static EHealthProblem FromUnexpectedRawResponse(string rawResponse)
    {
        return new EHealthProblem(
            "urn:problem-type:smals:pseudo-helper:unexpected-result",
            "Unexpected Result",
            "undefined",
            $"Unable to convert the response from eHealth into a EHealthProblem. Response was: \n{rawResponse}");
    }

    /// <summary>
    /// Creates an <see cref="EHealthProblem"/> instance for a response that cannot be read as JSON.
    /// </summary>
    /// <param name="rawResponse">The raw response, quoted as it was received.</param>
    /// <returns>An instance of <see cref="EHealthProblem"/>.</returns>
    public static EHealthProblem FromUnparseableRawResponse(string rawResponse)
    {
        return new EHealthProblem(
            "urn:problem-type:smals:pseudo-helper:unparseable-result",
            "Unparseable Result",
            "undefined",
            $"Unable to parse the response from eHealth. Response was: \n{rawResponse}");
    }
}