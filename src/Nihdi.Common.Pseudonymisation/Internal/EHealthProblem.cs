// <copyright file="EHealthProblem.cs" company="Riziv-Inami">
// Copyright (c) Riziv-Inami. All rights reserved.
// </copyright>

namespace Nihdi.Common.Pseudonymisation.Internal;

using Newtonsoft.Json.Linq;

/// <summary>
/// Implementation of <see cref="IEHealthProblem"/> representing an eHealth problem.
/// </summary>
public class EHealthProblem : IEHealthProblem
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
    public static EHealthProblem FromResponse(JObject response)
    {
        return response.ContainsKey("type")
            ? new EHealthProblem(
                response.Value<string>("type") ?? string.Empty,
                response.Value<string>("title") ?? string.Empty,
                response.Value<string>("status") ?? string.Empty,
                response.Value<string>("detail") ?? string.Empty)
            : new EHealthProblem(
                "urn:problem-type:ictreuse:pseudonymisation-helper:unexpected-result",
                "Unexpected Result",
                "undefined",
                $"Unable to convert the response from eHealth into a EHealthProblem. Response was: \n{response.ToString(Newtonsoft.Json.Formatting.Indented)}");
    }
}