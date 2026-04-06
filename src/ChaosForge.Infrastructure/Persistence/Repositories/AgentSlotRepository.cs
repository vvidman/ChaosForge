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
using ChaosForge.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ChaosForge.Infrastructure.Persistence.Repositories;

internal sealed class AgentSlotRepository(AppDbContext context)
    : RepositoryBase<AgentSlot, Guid>(context), IAgentSlotRepository
{
    public async Task<IReadOnlyList<AgentSlot>> GetByProjectIdAsync(Guid projectId, CancellationToken cancellationToken = default)
    {
        return await _context.AgentSlots
            .AsNoTracking()
            .Where(s => s.ProjectId == projectId)
            .ToListAsync(cancellationToken);
    }
}
