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
using ChaosForge.Domain.Exceptions;
using ChaosForge.Domain.Repositories;
using FluentValidation;
using MediatR;

namespace ChaosForge.Application.AgentInstances.Commands;

public record StartAgentWorkCommand(Guid AgentInstanceId, Guid TaskId) : IRequest<Result>;

public sealed class StartAgentWorkCommandValidator : AbstractValidator<StartAgentWorkCommand>
{
    public StartAgentWorkCommandValidator()
    {
        RuleFor(x => x.AgentInstanceId).NotEmpty().WithMessage("AgentInstanceId must not be empty.");
        // TaskId may be Guid.Empty for phase-level agents (e.g. BusinessAnalyst) that have no
        // associated WorkTask. Design gap: tracked for future phase-level AgentTask concept.
    }
}

internal sealed class StartAgentWorkCommandHandler(
    IAgentInstanceRepository agentInstanceRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<StartAgentWorkCommand, Result>
{
    public async Task<Result> Handle(StartAgentWorkCommand request, CancellationToken cancellationToken)
    {
        var agentInstance = await agentInstanceRepository.GetByIdAsync(request.AgentInstanceId, cancellationToken);

        if (agentInstance is null)
        {
            return Result.Failure("AgentInstance not found.");
        }

        try
        {
            agentInstance.StartWork(request.TaskId);
        }
        catch (DomainException ex)
        {
            return Result.Failure(ex.Message);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
