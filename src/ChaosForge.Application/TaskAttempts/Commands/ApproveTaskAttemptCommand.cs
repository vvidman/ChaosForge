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

namespace ChaosForge.Application.TaskAttempts.Commands;

public record ApproveTaskAttemptCommand(Guid TaskAttemptId) : IRequest<Result>;

public sealed class ApproveTaskAttemptCommandValidator : AbstractValidator<ApproveTaskAttemptCommand>
{
    public ApproveTaskAttemptCommandValidator()
    {
        RuleFor(x => x.TaskAttemptId).NotEmpty().WithMessage("TaskAttemptId must not be empty.");
    }
}

internal sealed class ApproveTaskAttemptCommandHandler(
    ITaskAttemptRepository taskAttemptRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<ApproveTaskAttemptCommand, Result>
{
    public async Task<Result> Handle(ApproveTaskAttemptCommand request, CancellationToken cancellationToken)
    {
        var attempt = await taskAttemptRepository.GetByIdAsync(request.TaskAttemptId, cancellationToken);

        if (attempt is null)
        {
            return Result.Failure("TaskAttempt not found.");
        }

        try
        {
            attempt.Approve();
        }
        catch (DomainException ex)
        {
            return Result.Failure(ex.Message);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
