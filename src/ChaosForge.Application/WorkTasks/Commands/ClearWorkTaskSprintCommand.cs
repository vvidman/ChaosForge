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

namespace ChaosForge.Application.WorkTasks.Commands;

public record ClearWorkTaskSprintCommand(Guid WorkTaskId) : IRequest<Result>;

public sealed class ClearWorkTaskSprintCommandValidator : AbstractValidator<ClearWorkTaskSprintCommand>
{
    public ClearWorkTaskSprintCommandValidator()
    {
        RuleFor(x => x.WorkTaskId).NotEmpty().WithMessage("WorkTaskId must not be empty.");
    }
}

internal sealed class ClearWorkTaskSprintCommandHandler(
    IWorkTaskRepository workTaskRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<ClearWorkTaskSprintCommand, Result>
{
    public async Task<Result> Handle(ClearWorkTaskSprintCommand request, CancellationToken cancellationToken)
    {
        var task = await workTaskRepository.GetByIdAsync(request.WorkTaskId, cancellationToken);

        if (task is null)
        {
            return Result.Failure("Work task not found.");
        }

        task.ClearSprint();
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
