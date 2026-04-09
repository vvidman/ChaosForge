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

namespace ChaosForge.Application.AgentSlots.Commands;

public record CreateAgentSlotCommand(Guid ProjectId, AgentRole Role, int Count) : IRequest<Result>;

public sealed class CreateAgentSlotCommandValidator : AbstractValidator<CreateAgentSlotCommand>
{
    public CreateAgentSlotCommandValidator()
    {
        RuleFor(x => x.ProjectId).NotEmpty().WithMessage("ProjectId must not be empty.");
        RuleFor(x => x.Count).GreaterThanOrEqualTo(1).WithMessage("Count must be >= 1.");
    }
}

internal sealed class CreateAgentSlotCommandHandler(
    IAgentSlotRepository agentSlotRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<CreateAgentSlotCommand, Result>
{
    public async Task<Result> Handle(CreateAgentSlotCommand request, CancellationToken cancellationToken)
    {
        AgentSlot agentSlot;

        try
        {
            agentSlot = new AgentSlot(request.ProjectId, request.Role, request.Count);
        }
        catch (DomainException ex)
        {
            return Result.Failure(ex.Message);
        }

        await agentSlotRepository.AddAsync(agentSlot, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
