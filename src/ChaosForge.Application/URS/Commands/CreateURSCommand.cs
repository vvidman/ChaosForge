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

namespace ChaosForge.Application.URS.Commands;

public record CreateURSCommand(Guid UseCaseId, string Title, string Description) : IRequest<Result>;

public sealed class CreateURSCommandValidator : AbstractValidator<CreateURSCommand>
{
    public CreateURSCommandValidator()
    {
        RuleFor(x => x.UseCaseId).NotEmpty().WithMessage("UseCaseId must not be empty.");
        RuleFor(x => x.Title).NotEmpty().WithMessage("Title must not be empty.");
        RuleFor(x => x.Description).NotEmpty().WithMessage("Description must not be empty.");
    }
}

internal sealed class CreateURSCommandHandler(
    IURSRepository ursRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<CreateURSCommand, Result>
{
    public async Task<Result> Handle(CreateURSCommand request, CancellationToken cancellationToken)
    {
        Domain.Entities.URS urs;

        try
        {
            urs = new Domain.Entities.URS(request.UseCaseId, request.Title, request.Description);
        }
        catch (DomainException ex)
        {
            return Result.Failure(ex.Message);
        }

        await ursRepository.AddAsync(urs, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
