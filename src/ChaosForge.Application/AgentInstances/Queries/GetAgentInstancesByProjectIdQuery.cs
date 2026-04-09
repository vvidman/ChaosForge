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
using ChaosForge.Domain.Repositories;
using MediatR;

namespace ChaosForge.Application.AgentInstances.Queries;

public record GetAgentInstancesByProjectIdQuery(Guid ProjectId) : IRequest<Result<IReadOnlyList<AgentInstanceDto>>>;

internal sealed class GetAgentInstancesByProjectIdQueryHandler(IAgentInstanceRepository agentInstanceRepository)
    : IRequestHandler<GetAgentInstancesByProjectIdQuery, Result<IReadOnlyList<AgentInstanceDto>>>
{
    public async Task<Result<IReadOnlyList<AgentInstanceDto>>> Handle(
        GetAgentInstancesByProjectIdQuery request,
        CancellationToken cancellationToken)
    {
        var instances = await agentInstanceRepository.GetByProjectIdAsync(request.ProjectId, cancellationToken);

        var dtos = instances
            .Select(i => new AgentInstanceDto(
                i.Id,
                i.ProjectId,
                i.Role,
                i.PersonaName,
                i.Status,
                i.CurrentTaskId,
                i.CreatedAt))
            .ToList();

        return Result<IReadOnlyList<AgentInstanceDto>>.Success(dtos);
    }
}
