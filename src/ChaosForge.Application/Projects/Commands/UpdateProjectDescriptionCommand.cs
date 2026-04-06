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

namespace ChaosForge.Application.Projects.Commands;

public record UpdateProjectDescriptionCommand(Guid ProjectId, string Description) : IRequest<Result>;

public sealed class UpdateProjectDescriptionCommandValidator : AbstractValidator<UpdateProjectDescriptionCommand>
{
    public UpdateProjectDescriptionCommandValidator()
    {
        RuleFor(x => x.ProjectId).NotEmpty().WithMessage("ProjectId must not be empty.");
        RuleFor(x => x.Description).NotEmpty().WithMessage("Description must not be empty.");
    }
}

internal sealed class UpdateProjectDescriptionCommandHandler(
    IProjectRepository projectRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateProjectDescriptionCommand, Result>
{
    public async Task<Result> Handle(UpdateProjectDescriptionCommand request, CancellationToken cancellationToken)
    {
        var project = await projectRepository.GetByIdAsync(request.ProjectId, cancellationToken);

        if (project is null)
        {
            return Result.Failure("Project not found.");
        }

        try
        {
            project.UpdateDescription(request.Description);
        }
        catch (DomainException ex)
        {
            return Result.Failure(ex.Message);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
