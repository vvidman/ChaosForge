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

namespace ChaosForge.Application.Projects.Commands;

public record CreateProjectCommand(string Name, string Description, DateTime? Deadline) : IRequest<Result>;

public sealed class CreateProjectCommandValidator : AbstractValidator<CreateProjectCommand>
{
    public CreateProjectCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Name must not be empty.");
        RuleFor(x => x.Description).NotEmpty().WithMessage("Description must not be empty.");
    }
}

internal sealed class CreateProjectCommandHandler(
    IProjectRepository projectRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<CreateProjectCommand, Result>
{
    public async Task<Result> Handle(CreateProjectCommand request, CancellationToken cancellationToken)
    {
        Project project;

        try
        {
            project = new Project(request.Name, request.Description, request.Deadline);
        }
        catch (DomainException ex)
        {
            return Result.Failure(ex.Message);
        }

        await projectRepository.AddAsync(project, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
