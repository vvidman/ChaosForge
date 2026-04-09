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
using FluentValidation;
using MediatR;

namespace ChaosForge.Application.AgentInstances.Commands;

public record MarkAgentFinishedCommand(Guid AgentInstanceId) : IRequest<Result>;

public sealed class MarkAgentFinishedCommandValidator : AbstractValidator<MarkAgentFinishedCommand>
{
    public MarkAgentFinishedCommandValidator()
    {
        RuleFor(x => x.AgentInstanceId).NotEmpty().WithMessage("AgentInstanceId must not be empty.");
    }
}

internal sealed class MarkAgentFinishedCommandHandler(
    IAgentInstanceRepository agentInstanceRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<MarkAgentFinishedCommand, Result>
{
    public async Task<Result> Handle(MarkAgentFinishedCommand request, CancellationToken cancellationToken)
    {
        var agentInstance = await agentInstanceRepository.GetByIdAsync(request.AgentInstanceId, cancellationToken);

        if (agentInstance is null)
        {
            return Result.Failure("AgentInstance not found.");
        }

        agentInstance.MarkFinished();
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
