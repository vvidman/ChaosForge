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

namespace ChaosForge.Application.AgentInstances.Queries;

public record AgentInstanceDto(
    Guid Id,
    Guid ProjectId,
    AgentRole Role,
    string PersonaName,
    AgentInstanceStatus Status,
    Guid? CurrentTaskId,
    DateTime CreatedAt);

public record GetAgentInstanceByIdQuery(Guid AgentInstanceId) : IRequest<Result<AgentInstanceDto>>;

internal sealed class GetAgentInstanceByIdQueryHandler(IAgentInstanceRepository agentInstanceRepository)
    : IRequestHandler<GetAgentInstanceByIdQuery, Result<AgentInstanceDto>>
{
    public async Task<Result<AgentInstanceDto>> Handle(
        GetAgentInstanceByIdQuery request,
        CancellationToken cancellationToken)
    {
        var instance = await agentInstanceRepository.GetByIdAsync(request.AgentInstanceId, cancellationToken);

        if (instance is null)
        {
            return Result<AgentInstanceDto>.Failure("Agent instance not found.");
        }

        var dto = new AgentInstanceDto(
            instance.Id,
            instance.ProjectId,
            instance.Role,
            instance.PersonaName,
            instance.Status,
            instance.CurrentTaskId,
            instance.CreatedAt);

        return Result<AgentInstanceDto>.Success(dto);
    }
}
