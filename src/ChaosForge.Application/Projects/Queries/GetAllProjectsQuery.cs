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

namespace ChaosForge.Application.Projects.Queries;

public record ProjectSummaryDto(
    Guid Id,
    string Name,
    string Description,
    ProjectStatus Status,
    DateTime? Deadline,
    DateTime CreatedAt);

public record ProjectDto(
    Guid Id,
    string Name,
    string Description,
    ProjectStatus Status,
    DateTime? Deadline,
    DateTime CreatedAt);

public record GetAllProjectsQuery : IRequest<Result<IReadOnlyList<ProjectSummaryDto>>>;

internal sealed class GetAllProjectsQueryHandler(IProjectRepository projectRepository)
    : IRequestHandler<GetAllProjectsQuery, Result<IReadOnlyList<ProjectSummaryDto>>>
{
    public async Task<Result<IReadOnlyList<ProjectSummaryDto>>> Handle(
        GetAllProjectsQuery request,
        CancellationToken cancellationToken)
    {
        var projects = await projectRepository.GetAllAsync(cancellationToken);

        var dtos = projects
            .Select(p => new ProjectSummaryDto(p.Id, p.Name, p.Description, p.Status, p.Deadline, p.CreatedAt))
            .ToList();

        return Result<IReadOnlyList<ProjectSummaryDto>>.Success(dtos);
    }
}
