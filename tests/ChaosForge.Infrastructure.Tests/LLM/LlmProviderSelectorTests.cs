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

using ChaosForge.Application.Abstractions;
using ChaosForge.Domain.Enums;
using ChaosForge.Infrastructure.LLM;
using FluentAssertions;
using NSubstitute;

namespace ChaosForge.Infrastructure.Tests.LLM;

public sealed class LlmProviderSelectorTests
{
    private readonly ILlmProvider _cloudProvider = Substitute.For<ILlmProvider>();
    private readonly ILlmProvider _localProvider = Substitute.For<ILlmProvider>();
    private readonly LlmProviderSelector _selector;

    public LlmProviderSelectorTests()
    {
        _selector = new LlmProviderSelector(_cloudProvider, _localProvider);
    }

    [Theory]
    [InlineData(AgentRole.BusinessAnalyst)]
    [InlineData(AgentRole.Architect)]
    [InlineData(AgentRole.ScrumMaster)]
    public void GetProviderForRole_WithCloudPreferredRole_ReturnsCloudProvider(AgentRole role)
    {
        // Act
        var result = _selector.GetProviderForRole(role);

        // Assert
        result.Should().BeSameAs(_cloudProvider);
    }

    [Theory]
    [InlineData(AgentRole.Developer)]
    [InlineData(AgentRole.Tester)]
    [InlineData(AgentRole.Reviewer)]
    [InlineData(AgentRole.TechnicalWriter)]
    public void GetProviderForRole_WithLocalPreferredRole_ReturnsLocalProvider(AgentRole role)
    {
        // Act
        var result = _selector.GetProviderForRole(role);

        // Assert
        result.Should().BeSameAs(_localProvider);
    }
}
