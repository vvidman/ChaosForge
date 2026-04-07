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

namespace ChaosForge.Application.WorkTasks.Queries;

public record WorkTaskDto(
    Guid Id,
    Guid SRSId,
    Guid? SprintId,
    string Title,
    string Description,
    WorkTaskStatus Status,
    int StoryPoints,
    DateTime CreatedAt);

public record GetWorkTaskByIdQuery(Guid WorkTaskId) : IRequest<Result<WorkTaskDto>>;

internal sealed class GetWorkTaskByIdQueryHandler(IWorkTaskRepository workTaskRepository)
    : IRequestHandler<GetWorkTaskByIdQuery, Result<WorkTaskDto>>
{
    public async Task<Result<WorkTaskDto>> Handle(
        GetWorkTaskByIdQuery request,
        CancellationToken cancellationToken)
    {
        var task = await workTaskRepository.GetByIdAsync(request.WorkTaskId, cancellationToken);

        if (task is null)
        {
            return Result<WorkTaskDto>.Failure("Work task not found.");
        }

        var dto = new WorkTaskDto(
            task.Id,
            task.SRSId,
            task.SprintId,
            task.Title,
            task.Description,
            task.Status,
            task.StoryPoints,
            task.CreatedAt);

        return Result<WorkTaskDto>.Success(dto);
    }
}
