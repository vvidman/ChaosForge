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

using ChaosForge.Infrastructure.LLM;
using FluentAssertions;
using Microsoft.Extensions.Options;

namespace ChaosForge.Infrastructure.Tests.LLM;

public sealed class LlamaSharpLlmProviderTests
{
    [Fact]
    public void Constructor_WithEmptyModelPath_ThrowsInvalidOperationException()
    {
        // Arrange
        var options = Options.Create(new LlamaSharpOptions { ModelPath = string.Empty });

        // Act
        var act = () => new LlamaSharpLlmProvider(options);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*ModelPath*");
    }

    [Fact]
    public void Constructor_WithNonExistentModelPath_ThrowsInvalidOperationException()
    {
        // Arrange
        var options = Options.Create(new LlamaSharpOptions { ModelPath = "/nonexistent/model.gguf" });

        // Act
        var act = () => new LlamaSharpLlmProvider(options);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*not found*");
    }
}
