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

using ChaosForge.Application.Common;
using ChaosForge.Domain.Enums;
using ChaosForge.Domain.Repositories;
using MediatR;

namespace ChaosForge.Application.AgentSlots.Queries;

public record AgentSlotDto(
    Guid Id,
    Guid ProjectId,
    AgentRole Role,
    int Count,
    DateTime CreatedAt);

public record GetAgentSlotsByProjectIdQuery(Guid ProjectId) : IRequest<Result<IReadOnlyList<AgentSlotDto>>>;

internal sealed class GetAgentSlotsByProjectIdQueryHandler(IAgentSlotRepository agentSlotRepository)
    : IRequestHandler<GetAgentSlotsByProjectIdQuery, Result<IReadOnlyList<AgentSlotDto>>>
{
    public async Task<Result<IReadOnlyList<AgentSlotDto>>> Handle(
        GetAgentSlotsByProjectIdQuery request,
        CancellationToken cancellationToken)
    {
        var slots = await agentSlotRepository.GetByProjectIdAsync(request.ProjectId, cancellationToken);

        var dtos = slots
            .Select(s => new AgentSlotDto(s.Id, s.ProjectId, s.Role, s.Count, s.CreatedAt))
            .ToList();

        return Result<IReadOnlyList<AgentSlotDto>>.Success(dtos);
    }
}
