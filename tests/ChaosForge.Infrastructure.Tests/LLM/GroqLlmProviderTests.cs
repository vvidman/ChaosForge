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
using Microsoft.Extensions.Options;

namespace ChaosForge.Infrastructure.Tests.LLM;

public sealed class GroqLlmProviderTests
{
    private static GroqLlmProvider CreateProvider(HttpMessageHandler handler, string model = "llama-3.3-70b-versatile")
    {
        var client = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://api.groq.com")
        };
        var options = Options.Create(new GroqOptions { Model = model });

        return new GroqLlmProvider(client, options);
    }

    [Fact]
    public async Task CompleteAsync_WithValidResponse_ReturnsContent()
    {
        // Arrange
        const string expectedContent = "Hello from Groq!";
        var json = $$"""
            {
              "choices": [
                {
                  "message": {
                    "content": "{{expectedContent}}"
                  }
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
        var handler = new FakeHttpMessageHandler(HttpStatusCode.Unauthorized, string.Empty);
        var provider = CreateProvider(handler);

        // Act
        var act = async () => await provider.CompleteAsync("system", "user", CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*401*");
    }

    [Fact]
    public async Task CompleteAsync_WithMissingChoicesArray_ThrowsInvalidOperationException()
    {
        // Arrange
        const string json = """{ "id": "abc123" }""";
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
        const string json = """{ "choices": [] }""";
        var handler = new FakeHttpMessageHandler(HttpStatusCode.OK, json);
        var provider = CreateProvider(handler);

        // Act
        var act = async () => await provider.CompleteAsync("system", "user", CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*choices*");
    }

    private sealed class FakeHttpMessageHandler : HttpMessageHandler
    {
        private readonly HttpStatusCode _statusCode;
        private readonly string _body;

        public FakeHttpMessageHandler(HttpStatusCode statusCode, string body)
        {
            _statusCode = statusCode;
            _body = body;
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var response = new HttpResponseMessage(_statusCode)
            {
                Content = new StringContent(_body, Encoding.UTF8, "application/json")
            };

            return Task.FromResult(response);
        }
    }
}
