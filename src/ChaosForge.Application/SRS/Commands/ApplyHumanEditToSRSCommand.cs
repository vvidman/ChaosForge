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

namespace ChaosForge.Application.SRS.Commands;

public record ApplyHumanEditToSRSCommand(Guid SRSId, string EditedDescription, string Note) : IRequest<Result>;

public sealed class ApplyHumanEditToSRSCommandValidator : AbstractValidator<ApplyHumanEditToSRSCommand>
{
    public ApplyHumanEditToSRSCommandValidator()
    {
        RuleFor(x => x.SRSId).NotEmpty().WithMessage("SRSId must not be empty.");
        RuleFor(x => x.EditedDescription).NotEmpty().WithMessage("EditedDescription must not be empty.");
        RuleFor(x => x.Note).NotEmpty().WithMessage("Note must not be empty.");
    }
}

internal sealed class ApplyHumanEditToSRSCommandHandler(
    ISRSRepository srsRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<ApplyHumanEditToSRSCommand, Result>
{
    public async Task<Result> Handle(ApplyHumanEditToSRSCommand request, CancellationToken cancellationToken)
    {
        var srs = await srsRepository.GetByIdAsync(request.SRSId, cancellationToken);

        if (srs is null)
        {
            return Result.Failure("SRS not found.");
        }

        try
        {
            srs.ApplyHumanEdit(request.EditedDescription, request.Note);
        }
        catch (DomainException ex)
        {
            return Result.Failure(ex.Message);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
