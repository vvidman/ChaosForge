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

namespace ChaosForge.Infrastructure.Tests.LLM;

public sealed class InferRouterOptionsValidatorTests
{
    private readonly InferRouterOptionsValidator _validator = new();

    [Theory]
    [InlineData("http://localhost:5100")]
    [InlineData("http://host.docker.internal:5100")]
    [InlineData("https://inferrouter.example.com")]
    public void Validate_WithAbsoluteHttpUrl_Succeeds(string baseUrl)
    {
        // Arrange
        var options = new InferRouterOptions { BaseUrl = baseUrl };

        // Act
        var result = _validator.Validate(name: null, options);

        // Assert
        result.Succeeded.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_WithMissingBaseUrl_FailsWithRequiredMessage(string baseUrl)
    {
        // Arrange
        var options = new InferRouterOptions { BaseUrl = baseUrl };

        // Act
        var result = _validator.Validate(name: null, options);

        // Assert
        result.Failed.Should().BeTrue();
        result.FailureMessage.Should().Contain("InferRouter:BaseUrl is required");
    }

    [Theory]
    [InlineData("localhost:5100")]
    [InlineData("/v1")]
    [InlineData("ftp://inferrouter.example.com")]
    [InlineData("http://<your-inferrouter-host>:5100")]
    public void Validate_WithNonHttpOrRelativeUrl_FailsWithFormatMessage(string baseUrl)
    {
        // Arrange
        var options = new InferRouterOptions { BaseUrl = baseUrl };

        // Act
        var result = _validator.Validate(name: null, options);

        // Assert
        result.Failed.Should().BeTrue();
        result.FailureMessage.Should().Contain("must be an absolute http or https URL");
    }
}
