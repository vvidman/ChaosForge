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

namespace ChaosForge.Application.UseCases.Commands;

public record CreateUseCaseCommand(Guid ProjectId, string Title, string Description, int Priority) : IRequest<Result>;

public sealed class CreateUseCaseCommandValidator : AbstractValidator<CreateUseCaseCommand>
{
    public CreateUseCaseCommandValidator()
    {
        RuleFor(x => x.ProjectId).NotEmpty().WithMessage("ProjectId must not be empty.");
        RuleFor(x => x.Title).NotEmpty().WithMessage("Title must not be empty.");
        RuleFor(x => x.Description).NotEmpty().WithMessage("Description must not be empty.");
        RuleFor(x => x.Priority).GreaterThanOrEqualTo(0).WithMessage("Priority must be >= 0.");
    }
}

internal sealed class CreateUseCaseCommandHandler(
    IUseCaseRepository useCaseRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<CreateUseCaseCommand, Result>
{
    public async Task<Result> Handle(CreateUseCaseCommand request, CancellationToken cancellationToken)
    {
        UseCase useCase;

        try
        {
            useCase = new UseCase(request.ProjectId, request.Title, request.Description, request.Priority);
        }
        catch (DomainException ex)
        {
            return Result.Failure(ex.Message);
        }

        await useCaseRepository.AddAsync(useCase, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
