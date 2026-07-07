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
using System.Text.Json.Serialization;
using ChaosForge.Application.Abstractions;

namespace ChaosForge.Infrastructure.LLM;

internal sealed class InferRouterLlmProvider : ILlmProvider
{
    private readonly HttpClient _httpClient;
    private readonly string _preferredProviderName;

    public InferRouterLlmProvider(HttpClient httpClient, string preferredProviderName)
    {
        _httpClient = httpClient;
        _preferredProviderName = preferredProviderName;
    }

    public async Task<string> CompleteAsync(
        string systemPrompt,
        string userPrompt,
        CancellationToken cancellationToken = default)
    {
        var request = new OpenAiChatRequest(
            Model: string.Empty,
            Messages:
            [
                new OpenAiMessage("system", systemPrompt),
                new OpenAiMessage("user", userPrompt)
            ],
            PreferredProviderName: _preferredProviderName);

        var response = await _httpClient.PostAsJsonAsync(
            "/v1/chat/completions",
            request,
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(
                $"InferRouter request failed: {(int)response.StatusCode} {response.ReasonPhrase}");
        }

        var body = await response.Content
            .ReadFromJsonAsync<OpenAiChatResponse>(cancellationToken)
            ?? throw new InvalidOperationException("InferRouter returned an empty response body.");

        if (body.Choices is not { Count: > 0 })
        {
            throw new InvalidOperationException(
                "InferRouter response did not contain a 'choices' array with at least one element.");
        }

        return body.Choices[0].Message.Content
            ?? throw new InvalidOperationException("InferRouter response 'choices[0].message.content' was null.");
    }
}

internal sealed record OpenAiChatRequest(
    [property: JsonPropertyName("model")] string Model,
    [property: JsonPropertyName("messages")] List<OpenAiMessage> Messages,
    [property: JsonPropertyName("preferred_provider_name")] string? PreferredProviderName = null);

internal sealed record OpenAiMessage(
    [property: JsonPropertyName("role")] string Role,
    [property: JsonPropertyName("content")] string? Content);

internal sealed record OpenAiChatResponse(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("object")] string Object,
    [property: JsonPropertyName("created")] long Created,
    [property: JsonPropertyName("model")] string Model,
    [property: JsonPropertyName("system_fingerprint")] string? SystemFingerprint,
    [property: JsonPropertyName("choices")] List<OpenAiChoice> Choices,
    [property: JsonPropertyName("usage")] OpenAiUsage? Usage);

internal sealed record OpenAiChoice(
    [property: JsonPropertyName("index")] int Index,
    [property: JsonPropertyName("message")] OpenAiMessage Message,
    [property: JsonPropertyName("finish_reason")] string? FinishReason);

internal sealed record OpenAiUsage(
    [property: JsonPropertyName("prompt_tokens")] int PromptTokens,
    [property: JsonPropertyName("completion_tokens")] int CompletionTokens,
    [property: JsonPropertyName("total_tokens")] int TotalTokens);
