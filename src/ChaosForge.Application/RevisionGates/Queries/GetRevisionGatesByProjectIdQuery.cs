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

namespace ChaosForge.Application.RevisionGates.Queries;

public record GetRevisionGatesByProjectIdQuery(Guid ProjectId) : IRequest<Result<IReadOnlyList<RevisionGateDto>>>;

internal sealed class GetRevisionGatesByProjectIdQueryHandler(IRevisionGateRepository revisionGateRepository)
    : IRequestHandler<GetRevisionGatesByProjectIdQuery, Result<IReadOnlyList<RevisionGateDto>>>
{
    public async Task<Result<IReadOnlyList<RevisionGateDto>>> Handle(
        GetRevisionGatesByProjectIdQuery request,
        CancellationToken cancellationToken)
    {
        var gates = await revisionGateRepository.GetByProjectIdAsync(request.ProjectId, cancellationToken);

        var dtos = gates
            .Select(g => new RevisionGateDto(g.Id, g.ProjectId, g.Type, g.Status, g.AgentOutput, g.HumanEditedOutput, g.RejectionReason, g.Action, g.ResolvedAt, g.CreatedAt))
            .ToList();

        return Result<IReadOnlyList<RevisionGateDto>>.Success(dtos);
    }
}
