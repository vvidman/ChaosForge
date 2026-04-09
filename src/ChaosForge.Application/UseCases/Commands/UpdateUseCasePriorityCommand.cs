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

namespace ChaosForge.Application.UseCases.Commands;

public record UpdateUseCasePriorityCommand(Guid UseCaseId, int Priority) : IRequest<Result>;

public sealed class UpdateUseCasePriorityCommandValidator : AbstractValidator<UpdateUseCasePriorityCommand>
{
    public UpdateUseCasePriorityCommandValidator()
    {
        RuleFor(x => x.UseCaseId).NotEmpty().WithMessage("UseCaseId must not be empty.");
        RuleFor(x => x.Priority).GreaterThanOrEqualTo(0).WithMessage("Priority must be >= 0.");
    }
}

internal sealed class UpdateUseCasePriorityCommandHandler(
    IUseCaseRepository useCaseRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateUseCasePriorityCommand, Result>
{
    public async Task<Result> Handle(UpdateUseCasePriorityCommand request, CancellationToken cancellationToken)
    {
        var useCase = await useCaseRepository.GetByIdAsync(request.UseCaseId, cancellationToken);

        if (useCase is null)
        {
            return Result.Failure("UseCase not found.");
        }

        try
        {
            useCase.UpdatePriority(request.Priority);
        }
        catch (DomainException ex)
        {
            return Result.Failure(ex.Message);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
