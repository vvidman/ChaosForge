/*
   Copyright 2026 Viktor Vidman (vvidman)

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/

using System.Text.Json;
using System.Text.RegularExpressions;

namespace ChaosForge.Infrastructure.Agents;

/// <summary>
/// Tolerant parsing of JSON returned by an LLM. Models asked for "JSON only" still often wrap it
/// in a Markdown code fence, add a sentence around it, or nest the expected array in an object.
/// </summary>
internal static partial class LlmJson
{
    [GeneratedRegex(@"```[A-Za-z0-9_-]*\s*(?<body>.*?)\s*```", RegexOptions.Singleline)]
    private static partial Regex CodeFence();

    /// <summary>
    /// Returns the JSON payload of <paramref name="raw"/>: the body of the first code fence if there
    /// is one, trimmed to the span from the first <c>[</c> or <c>{</c> to the matching last closer.
    /// Returns the trimmed input unchanged when no JSON-looking span is found.
    /// </summary>
    public static string Extract(string raw)
    {
        var text = raw.Trim();

        var fence = CodeFence().Match(text);
        if (fence.Success)
        {
            text = fence.Groups["body"].Value.Trim();
        }

        var start = text.IndexOfAny(['[', '{']);
        if (start < 0)
        {
            return text;
        }

        var closer = text[start] == '[' ? ']' : '}';
        var end = text.LastIndexOf(closer);

        return end > start ? text[start..(end + 1)] : text;
    }

    /// <summary>
    /// Deserializes a list from <paramref name="raw"/>. Accepts a bare JSON array or an object whose
    /// first array-valued property holds the items (e.g. <c>{"tasks":[...]}</c>).
    /// </summary>
    /// <exception cref="JsonException">The payload is not valid JSON or contains no array.</exception>
    public static List<T> DeserializeList<T>(string raw, JsonSerializerOptions options)
    {
        using var document = JsonDocument.Parse(Extract(raw));
        var root = document.RootElement;

        var array = root.ValueKind switch
        {
            JsonValueKind.Array => root,
            JsonValueKind.Object => root.EnumerateObject()
                .Select(property => property.Value)
                .FirstOrDefault(value => value.ValueKind == JsonValueKind.Array),
            _ => default,
        };

        if (array.ValueKind != JsonValueKind.Array)
        {
            throw new JsonException($"Expected a JSON array but found {root.ValueKind}.");
        }

        return array.Deserialize<List<T>>(options) ?? [];
    }
}
