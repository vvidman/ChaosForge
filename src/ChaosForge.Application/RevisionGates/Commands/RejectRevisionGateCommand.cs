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

namespace ChaosForge.Application.RevisionGates.Commands;

public record RejectRevisionGateCommand(Guid RevisionGateId, string Reason) : IRequest<Result>;

public sealed class RejectRevisionGateCommandValidator : AbstractValidator<RejectRevisionGateCommand>
{
    public RejectRevisionGateCommandValidator()
    {
        RuleFor(x => x.RevisionGateId).NotEmpty().WithMessage("RevisionGateId must not be empty.");
        RuleFor(x => x.Reason).NotEmpty().WithMessage("Reason must not be empty.");
    }
}

internal sealed class RejectRevisionGateCommandHandler(
    IRevisionGateRepository revisionGateRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<RejectRevisionGateCommand, Result>
{
    public async Task<Result> Handle(RejectRevisionGateCommand request, CancellationToken cancellationToken)
    {
        var gate = await revisionGateRepository.GetByIdAsync(request.RevisionGateId, cancellationToken);

        if (gate is null)
        {
            return Result.Failure("Revision gate not found.");
        }

        try
        {
            gate.Reject(request.Reason);
        }
        catch (DomainException ex)
        {
            return Result.Failure(ex.Message);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
