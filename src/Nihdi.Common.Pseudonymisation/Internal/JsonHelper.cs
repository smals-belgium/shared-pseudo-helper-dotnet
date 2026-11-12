// <copyright file="JsonHelper.cs" company="Riziv-Inami">
// Copyright (c) Riziv-Inami. All rights reserved.
// </copyright>

namespace Nihdi.Common.Pseudonymisation.Internal;

using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using Nihdi.Common.Pseudonymisation.Exceptions;

/// <summary>
/// Reads the responses of eHealth and writes the requests sent to it.
/// </summary>
internal static class JsonHelper
{
    // The default encoder rewrites `+`, `<`, `>`, `&`, `'` and non-ASCII characters as Unicode
    // escapes. The coordinates of a point are standard base64 and contain `+`, so the request
    // would no longer carry them as they are. The name "Unsafe" concerns insertion in a web page,
    // which does not apply to a request body.
    private static readonly JsonSerializerOptions _compact = new()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    private static readonly JsonSerializerOptions _indented = new()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        WriteIndented = true,
    };

    /// <summary>
    /// Parses a response of eHealth as a JSON object.
    /// </summary>
    /// <param name="jsonString">The raw response.</param>
    /// <returns>The JSON object.</returns>
    /// <exception cref="EHealthProblemException">
    /// The response is not a JSON object. The problem quotes the response. For text that is not JSON
    /// (an HTML error page, an empty body, a truncated response), it is an unparseable result and the
    /// reading error is the cause; for JSON of another shape (an array, `null`...), an unexpected result.
    /// </exception>
    internal static JsonObject Parse(string jsonString)
    {
        JsonNode? node;
        try
        {
            node = JsonNode.Parse(jsonString);
        }
        catch (JsonException e)
        {
            throw new EHealthProblemException(EHealthProblem.FromUnparseableRawResponse(jsonString), e);
        }

        // Valid JSON that is not an object (an array, `null`, a number...) is an unexpected result,
        // not an unparseable one: the text did parse. Do not narrow the protection to reading
        // errors only: an empty body or `null` would then escape as a raw exception.
        return node as JsonObject
            ?? throw new EHealthProblemException(EHealthProblem.FromUnexpectedRawResponse(jsonString));
    }

    /// <summary>
    /// Writes a JSON node on a single line, without escaping the characters that base64 uses.
    /// </summary>
    /// <param name="node">The node to write.</param>
    /// <returns>The JSON text.</returns>
    internal static string ToCompactString(JsonNode node)
    {
        return node.ToJsonString(_compact);
    }

    /// <summary>
    /// Writes a JSON node over several lines, to quote it in a message.
    /// </summary>
    /// <param name="node">The node to write.</param>
    /// <returns>The JSON text.</returns>
    internal static string ToIndentedString(JsonNode node)
    {
        return node.ToJsonString(_indented);
    }
}
