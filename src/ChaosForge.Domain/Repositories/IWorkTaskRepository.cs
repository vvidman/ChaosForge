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

namespace ChaosForge.Domain.Repositories;

/// <summary>
/// Persistence contract for the <see cref="WorkTask"/> aggregate.
/// </summary>
public interface IWorkTaskRepository : IRepository<WorkTask, Guid>
{
    /// <summary>
    /// Returns all work tasks derived from the specified SRS item.
    /// </summary>
    /// <param name="srsId">The identifier of the parent SRS.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    Task<IReadOnlyList<WorkTask>> GetBySRSIdAsync(Guid srsId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns all work tasks assigned to the specified sprint.
    /// </summary>
    /// <param name="sprintId">The identifier of the sprint.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    Task<IReadOnlyList<WorkTask>> GetBySprintIdAsync(Guid sprintId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns all work tasks with the specified status.
    /// </summary>
    /// <param name="status">The status to filter by.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    Task<IReadOnlyList<WorkTask>> GetByStatusAsync(WorkTaskStatus status, CancellationToken cancellationToken = default);
}
