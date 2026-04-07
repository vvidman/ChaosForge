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
using ChaosForge.Domain.Exceptions;
using ChaosForge.Domain.Repositories;
using FluentValidation;
using MediatR;

namespace ChaosForge.Application.WorkTasks.Commands;

public record CreateWorkTaskCommand(Guid SRSId, string Title, string Description, int StoryPoints) : IRequest<Result>;

public sealed class CreateWorkTaskCommandValidator : AbstractValidator<CreateWorkTaskCommand>
{
    public CreateWorkTaskCommandValidator()
    {
        RuleFor(x => x.SRSId).NotEmpty().WithMessage("SRSId must not be empty.");
        RuleFor(x => x.Title).NotEmpty().WithMessage("Title must not be empty.");
        RuleFor(x => x.Description).NotEmpty().WithMessage("Description must not be empty.");
        RuleFor(x => x.StoryPoints).GreaterThanOrEqualTo(1).WithMessage("StoryPoints must be at least 1.");
    }
}

internal sealed class CreateWorkTaskCommandHandler(
    IWorkTaskRepository workTaskRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<CreateWorkTaskCommand, Result>
{
    public async Task<Result> Handle(CreateWorkTaskCommand request, CancellationToken cancellationToken)
    {
        WorkTask task;

        try
        {
            task = new WorkTask(request.SRSId, request.Title, request.Description, request.StoryPoints);
        }
        catch (DomainException ex)
        {
            return Result.Failure(ex.Message);
        }

        await workTaskRepository.AddAsync(task, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
