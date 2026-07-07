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
using Microsoft.Extensions.DependencyInjection;

namespace ChaosForge.Infrastructure.LLM;

internal sealed class LlmProviderSelector : ILlmProviderSelector
{
    private readonly ILlmProvider _cloudPreferred;
    private readonly ILlmProvider _localPreferred;

    public LlmProviderSelector(
        [FromKeyedServices("cloud-preferred")] ILlmProvider cloudPreferred,
        [FromKeyedServices("local-preferred")] ILlmProvider localPreferred)
    {
        _cloudPreferred = cloudPreferred;
        _localPreferred = localPreferred;
    }

    public ILlmProvider GetProviderForRole(AgentRole role) =>
        role switch
        {
            AgentRole.BusinessAnalyst => _cloudPreferred,
            AgentRole.Architect => _cloudPreferred,
            AgentRole.ScrumMaster => _cloudPreferred,
            AgentRole.Developer => _localPreferred,
            AgentRole.Tester => _localPreferred,
            AgentRole.Reviewer => _localPreferred,
            AgentRole.TechnicalWriter => _localPreferred,
            _ => throw new ArgumentOutOfRangeException(nameof(role), role, "No provider mapped for the given AgentRole.")
        };
}
