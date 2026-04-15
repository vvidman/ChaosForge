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

using ChaosForge.Domain.Entities;
using ChaosForge.Domain.Enums;
using ChaosForge.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ChaosForge.Infrastructure.Persistence.Repositories;

internal sealed class WorkTaskRepository(AppDbContext context)
    : RepositoryBase<WorkTask, Guid>(context), IWorkTaskRepository
{
    public async Task<IReadOnlyList<WorkTask>> GetByStatusAsync(WorkTaskStatus status, CancellationToken cancellationToken = default)
    {
        return await _context.WorkTasks
            .AsNoTracking()
            .Where(t => t.Status == status)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<WorkTask>> GetBySprintIdAsync(Guid sprintId, CancellationToken cancellationToken = default)
    {
        return await _context.WorkTasks
            .AsNoTracking()
            .Where(t => t.SprintId == sprintId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<WorkTask>> GetBySRSIdAsync(Guid srsId, CancellationToken cancellationToken = default)
    {
        return await _context.WorkTasks
            .AsNoTracking()
            .Where(t => t.SRSId == srsId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<WorkTask>> GetByProjectIdAsync(Guid projectId, CancellationToken cancellationToken = default)
    {
        return await _context.WorkTasks
            .AsNoTracking()
            .Where(t => _context.SRSs
                .Join(_context.URSs, srs => srs.URSId, urs => urs.Id, (srs, urs) => new { srs.Id, urs.UseCaseId })
                .Join(_context.UseCases, su => su.UseCaseId, uc => uc.Id, (su, uc) => new { SRSId = su.Id, uc.ProjectId })
                .Any(su => su.SRSId == t.SRSId && su.ProjectId == projectId))
            .ToListAsync(cancellationToken);
    }
}
