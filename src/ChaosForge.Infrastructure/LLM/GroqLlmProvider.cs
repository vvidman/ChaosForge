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

using System.Net.Http.Json;
using System.Text.Json;
using ChaosForge.Application.Abstractions;
using Microsoft.Extensions.Options;

namespace ChaosForge.Infrastructure.LLM;

internal sealed class GroqLlmProvider : ILlmProvider
{
    private readonly HttpClient _httpClient;
    private readonly GroqOptions _options;

    public GroqLlmProvider(HttpClient httpClient, IOptions<GroqOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    public async Task<string> CompleteAsync(
        string systemPrompt,
        string userPrompt,
        CancellationToken cancellationToken = default)
    {
        var payload = new
        {
            model = _options.Model,
            messages = new[]
            {
                new { role = "system", content = systemPrompt },
                new { role = "user", content = userPrompt }
            },
            temperature = 0.7
        };

        var response = await _httpClient.PostAsJsonAsync(
            "/openai/v1/chat/completions",
            payload,
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(
                $"Groq API returned non-success status: {(int)response.StatusCode} {response.ReasonPhrase}");
        }

        using var document = await JsonDocument.ParseAsync(
            await response.Content.ReadAsStreamAsync(cancellationToken),
            cancellationToken: cancellationToken);

        if (!document.RootElement.TryGetProperty("choices", out var choices) ||
            choices.GetArrayLength() == 0)
        {
            throw new InvalidOperationException(
                "Groq API response did not contain a 'choices' array with at least one element.");
        }

        var content = choices[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString();

        if (content is null)
        {
            throw new InvalidOperationException(
                "Groq API response 'choices[0].message.content' was null.");
        }

        return content;
    }
}
