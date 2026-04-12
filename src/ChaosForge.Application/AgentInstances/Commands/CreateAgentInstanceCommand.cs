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

namespace ChaosForge.Application.AgentInstances.Commands;

public record CreateAgentInstanceCommand(Guid ProjectId, AgentRole Role, string PersonaName) : IRequest<Result>;

public sealed class CreateAgentInstanceCommandValidator : AbstractValidator<CreateAgentInstanceCommand>
{
    public CreateAgentInstanceCommandValidator()
    {
        RuleFor(x => x.ProjectId).NotEmpty().WithMessage("ProjectId must not be empty.");
        RuleFor(x => x.PersonaName).NotEmpty().WithMessage("PersonaName must not be empty.");
    }
}

internal sealed class CreateAgentInstanceCommandHandler(
    IAgentInstanceRepository agentInstanceRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<CreateAgentInstanceCommand, Result>
{
    public async Task<Result> Handle(CreateAgentInstanceCommand request, CancellationToken cancellationToken)
    {
        AgentInstance agentInstance;

        try
        {
            agentInstance = new AgentInstance(request.ProjectId, request.Role, request.PersonaName);
        }
        catch (DomainException ex)
        {
            return Result.Failure(ex.Message);
        }

        await agentInstanceRepository.AddAsync(agentInstance, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
