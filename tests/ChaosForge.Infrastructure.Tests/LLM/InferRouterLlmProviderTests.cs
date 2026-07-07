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

using System.Net;
using System.Text;
using ChaosForge.Infrastructure.LLM;
using FluentAssertions;

namespace ChaosForge.Infrastructure.Tests.LLM;

public sealed class InferRouterLlmProviderTests
{
    private static InferRouterLlmProvider CreateProvider(
        HttpMessageHandler handler,
        string preferredProviderName = "groq")
    {
        var client = new HttpClient(handler)
        {
            BaseAddress = new Uri("http://inferrouter.local:5100")
        };

        return new InferRouterLlmProvider(client, preferredProviderName);
    }

    [Fact]
    public async Task CompleteAsync_WithValidResponse_ReturnsContent()
    {
        // Arrange
        const string expectedContent = "Hello from InferRouter!";
        var json = $$"""
            {
              "id": "abc123",
              "object": "chat.completion",
              "created": 1234567890,
              "model": "",
              "choices": [
                {
                  "index": 0,
                  "message": {
                    "role": "assistant",
                    "content": "{{expectedContent}}"
                  },
                  "finish_reason": "stop"
                }
              ]
            }
            """;

        var handler = new FakeHttpMessageHandler(HttpStatusCode.OK, json);
        var provider = CreateProvider(handler);

        // Act
        var result = await provider.CompleteAsync("system", "user", CancellationToken.None);

        // Assert
        result.Should().Be(expectedContent);
    }

    [Fact]
    public async Task CompleteAsync_WithNonSuccessStatus_ThrowsInvalidOperationException()
    {
        // Arrange
        var handler = new FakeHttpMessageHandler(HttpStatusCode.ServiceUnavailable, string.Empty);
        var provider = CreateProvider(handler);

        // Act
        var act = async () => await provider.CompleteAsync("system", "user", CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*503*");
    }

    [Fact]
    public async Task CompleteAsync_WithMissingChoicesArray_ThrowsInvalidOperationException()
    {
        // Arrange
        const string json = """{ "id": "abc123", "object": "chat.completion", "created": 1, "model": "" }""";
        var handler = new FakeHttpMessageHandler(HttpStatusCode.OK, json);
        var provider = CreateProvider(handler);

        // Act
        var act = async () => await provider.CompleteAsync("system", "user", CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*choices*");
    }

    [Fact]
    public async Task CompleteAsync_WithEmptyChoicesArray_ThrowsInvalidOperationException()
    {
        // Arrange
        const string json = """{ "id": "abc123", "object": "chat.completion", "created": 1, "model": "", "choices": [] }""";
        var handler = new FakeHttpMessageHandler(HttpStatusCode.OK, json);
        var provider = CreateProvider(handler);

        // Act
        var act = async () => await provider.CompleteAsync("system", "user", CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*choices*");
    }

    [Fact]
    public async Task CompleteAsync_WithNullContent_ThrowsInvalidOperationException()
    {
        // Arrange
        const string json = """
            {
              "id": "abc123", "object": "chat.completion", "created": 1, "model": "",
              "choices": [ { "index": 0, "message": { "role": "assistant", "content": null }, "finish_reason": "stop" } ]
            }
            """;
        var handler = new FakeHttpMessageHandler(HttpStatusCode.OK, json);
        var provider = CreateProvider(handler);

        // Act
        var act = async () => await provider.CompleteAsync("system", "user", CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*content*");
    }

    [Theory]
    [InlineData("groq")]
    [InlineData("local-llama")]
    public async Task CompleteAsync_SendsPreferredProviderNameInRequestBody(string preferredProviderName)
    {
        // Arrange
        const string json = """
            {
              "id": "abc123", "object": "chat.completion", "created": 1, "model": "",
              "choices": [ { "index": 0, "message": { "role": "assistant", "content": "ok" }, "finish_reason": "stop" } ]
            }
            """;
        var handler = new FakeHttpMessageHandler(HttpStatusCode.OK, json);
        var provider = CreateProvider(handler, preferredProviderName);

        // Act
        await provider.CompleteAsync("system", "user", CancellationToken.None);

        // Assert
        handler.LastRequestBody.Should().Contain($"\"preferred_provider_name\":\"{preferredProviderName}\"");
    }

    [Fact]
    public async Task CompleteAsync_WhenInferRouterUnreachable_PropagatesHttpRequestException()
    {
        // Arrange
        var handler = new ThrowingHttpMessageHandler(new HttpRequestException("Connection refused"));
        var provider = CreateProvider(handler);

        // Act
        var act = async () => await provider.CompleteAsync("system", "user", CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<HttpRequestException>();
    }

    [Fact]
    public async Task CompleteAsync_WithCancelledToken_ThrowsOperationCanceledException()
    {
        // Arrange
        var handler = new FakeHttpMessageHandler(HttpStatusCode.OK, "{}");
        var provider = CreateProvider(handler);
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        // Act
        var act = async () => await provider.CompleteAsync("system", "user", cts.Token);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    private sealed class FakeHttpMessageHandler : HttpMessageHandler
    {
        private readonly HttpStatusCode _statusCode;
        private readonly string _body;

        public string? LastRequestBody { get; private set; }

        public FakeHttpMessageHandler(HttpStatusCode statusCode, string body)
        {
            _statusCode = statusCode;
            _body = body;
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            LastRequestBody = request.Content is null
                ? null
                : await request.Content.ReadAsStringAsync(cancellationToken);

            var response = new HttpResponseMessage(_statusCode)
            {
                Content = new StringContent(_body, Encoding.UTF8, "application/json")
            };

            return response;
        }
    }

    private sealed class ThrowingHttpMessageHandler : HttpMessageHandler
    {
        private readonly Exception _exception;

        public ThrowingHttpMessageHandler(Exception exception)
        {
            _exception = exception;
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
            => throw _exception;
    }
}
