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

namespace ChaosForge.Application.RevisionGates.Queries;

public record RevisionGateDto(
    Guid Id,
    Guid ProjectId,
    RevisionGateType Type,
    RevisionGateStatus Status,
    string AgentOutput,
    string? HumanEditedOutput,
    string? RejectionReason,
    RevisionGateAction? Action,
    DateTime? ResolvedAt,
    DateTime CreatedAt);

public record GetRevisionGateByIdQuery(Guid RevisionGateId) : IRequest<Result<RevisionGateDto>>;

internal sealed class GetRevisionGateByIdQueryHandler(IRevisionGateRepository revisionGateRepository)
    : IRequestHandler<GetRevisionGateByIdQuery, Result<RevisionGateDto>>
{
    public async Task<Result<RevisionGateDto>> Handle(
        GetRevisionGateByIdQuery request,
        CancellationToken cancellationToken)
    {
        var gate = await revisionGateRepository.GetByIdAsync(request.RevisionGateId, cancellationToken);

        if (gate is null)
        {
            return Result<RevisionGateDto>.Failure("Revision gate not found.");
        }

        var dto = new RevisionGateDto(
            gate.Id,
            gate.ProjectId,
            gate.Type,
            gate.Status,
            gate.AgentOutput,
            gate.HumanEditedOutput,
            gate.RejectionReason,
            gate.Action,
            gate.ResolvedAt,
            gate.CreatedAt);

        return Result<RevisionGateDto>.Success(dto);
    }
}
