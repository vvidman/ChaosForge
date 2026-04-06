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
using ChaosForge.Domain.Entities;
using ChaosForge.Domain.Enums;
using ChaosForge.Domain.Exceptions;
using ChaosForge.Domain.Repositories;
using FluentValidation;
using MediatR;

namespace ChaosForge.Application.RevisionGates.Commands;

public record OpenRevisionGateCommand(Guid ProjectId, RevisionGateType Type, string AgentOutput) : IRequest<Result>;

public sealed class OpenRevisionGateCommandValidator : AbstractValidator<OpenRevisionGateCommand>
{
    public OpenRevisionGateCommandValidator()
    {
        RuleFor(x => x.ProjectId).NotEmpty().WithMessage("ProjectId must not be empty.");
        RuleFor(x => x.AgentOutput).NotEmpty().WithMessage("AgentOutput must not be empty.");
    }
}

internal sealed class OpenRevisionGateCommandHandler(
    IRevisionGateRepository revisionGateRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<OpenRevisionGateCommand, Result>
{
    public async Task<Result> Handle(OpenRevisionGateCommand request, CancellationToken cancellationToken)
    {
        var existing = await revisionGateRepository.GetOpenByProjectIdAsync(request.ProjectId, cancellationToken);

        if (existing is not null)
        {
            return Result.Failure("A revision gate is already open for this project.");
        }

        RevisionGate gate;

        try
        {
            gate = new RevisionGate(request.ProjectId, request.Type, request.AgentOutput);
        }
        catch (DomainException ex)
        {
            return Result.Failure(ex.Message);
        }

        await revisionGateRepository.AddAsync(gate, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
