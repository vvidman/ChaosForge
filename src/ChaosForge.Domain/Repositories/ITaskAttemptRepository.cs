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

namespace ChaosForge.Domain.Repositories;

/// <summary>
/// Persistence contract for the <see cref="TaskAttempt"/> aggregate.
/// </summary>
public interface ITaskAttemptRepository : IRepository<TaskAttempt, Guid>
{
    /// <summary>
    /// Returns all attempts made for the specified work task.
    /// </summary>
    /// <param name="workTaskId">The identifier of the parent work task.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    Task<IReadOnlyList<TaskAttempt>> GetByWorkTaskIdAsync(Guid workTaskId, CancellationToken cancellationToken = default);
}
