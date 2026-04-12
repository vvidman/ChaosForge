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
using ChaosForge.Domain.Repositories;
using MediatR;

namespace ChaosForge.Application.TaskAttempts.Queries;

public record GetTaskAttemptsByWorkTaskIdQuery(Guid WorkTaskId) : IRequest<Result<IReadOnlyList<TaskAttemptDto>>>;

internal sealed class GetTaskAttemptsByWorkTaskIdQueryHandler(ITaskAttemptRepository taskAttemptRepository)
    : IRequestHandler<GetTaskAttemptsByWorkTaskIdQuery, Result<IReadOnlyList<TaskAttemptDto>>>
{
    public async Task<Result<IReadOnlyList<TaskAttemptDto>>> Handle(
        GetTaskAttemptsByWorkTaskIdQuery request,
        CancellationToken cancellationToken)
    {
        var attempts = await taskAttemptRepository.GetByWorkTaskIdAsync(request.WorkTaskId, cancellationToken);

        var dtos = attempts
            .Select(a => new TaskAttemptDto(
                a.Id,
                a.WorkTaskId,
                a.AgentInstanceId,
                a.Type,
                a.Output,
                a.ReviewNote,
                a.TestNote,
                a.Result,
                a.StartedAt,
                a.CompletedAt,
                a.CreatedAt))
            .ToList();

        return Result<IReadOnlyList<TaskAttemptDto>>.Success(dtos);
    }
}
