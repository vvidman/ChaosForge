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

namespace ChaosForge.Application.WorkTasks.Queries;

public record GetWorkTasksBySprintIdQuery(Guid SprintId) : IRequest<Result<IReadOnlyList<WorkTaskDto>>>;

internal sealed class GetWorkTasksBySprintIdQueryHandler(IWorkTaskRepository workTaskRepository)
    : IRequestHandler<GetWorkTasksBySprintIdQuery, Result<IReadOnlyList<WorkTaskDto>>>
{
    public async Task<Result<IReadOnlyList<WorkTaskDto>>> Handle(
        GetWorkTasksBySprintIdQuery request,
        CancellationToken cancellationToken)
    {
        var tasks = await workTaskRepository.GetBySprintIdAsync(request.SprintId, cancellationToken);

        var dtos = tasks
            .Select(t => new WorkTaskDto(t.Id, t.SRSId, t.SprintId, t.Title, t.Description, t.Status, t.StoryPoints, t.CreatedAt))
            .ToList();

        return Result<IReadOnlyList<WorkTaskDto>>.Success(dtos);
    }
}
