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
using ChaosForge.Domain.Enums;
using ChaosForge.Domain.Repositories;
using MediatR;

namespace ChaosForge.Application.TaskAttempts.Queries;

public record TaskAttemptDto(
    Guid Id,
    Guid WorkTaskId,
    Guid AgentInstanceId,
    AttemptType Type,
    string Output,
    string? ReviewNote,
    string? TestNote,
    AttemptResult Result,
    DateTime StartedAt,
    DateTime? CompletedAt,
    DateTime CreatedAt);

public record GetTaskAttemptByIdQuery(Guid TaskAttemptId) : IRequest<Result<TaskAttemptDto>>;

internal sealed class GetTaskAttemptByIdQueryHandler(ITaskAttemptRepository taskAttemptRepository)
    : IRequestHandler<GetTaskAttemptByIdQuery, Result<TaskAttemptDto>>
{
    public async Task<Result<TaskAttemptDto>> Handle(
        GetTaskAttemptByIdQuery request,
        CancellationToken cancellationToken)
    {
        var attempt = await taskAttemptRepository.GetByIdAsync(request.TaskAttemptId, cancellationToken);

        if (attempt is null)
        {
            return Result<TaskAttemptDto>.Failure("Task attempt not found.");
        }

        var dto = new TaskAttemptDto(
            attempt.Id,
            attempt.WorkTaskId,
            attempt.AgentInstanceId,
            attempt.Type,
            attempt.Output,
            attempt.ReviewNote,
            attempt.TestNote,
            attempt.Result,
            attempt.StartedAt,
            attempt.CompletedAt,
            attempt.CreatedAt);

        return Result<TaskAttemptDto>.Success(dto);
    }
}
