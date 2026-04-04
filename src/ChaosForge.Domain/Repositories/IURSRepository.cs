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
/// Persistence contract for the <see cref="URS"/> aggregate.
/// </summary>
public interface IURSRepository : IRepository<URS, Guid>
{
    /// <summary>
    /// Returns all URS items belonging to the specified use case.
    /// </summary>
    /// <param name="useCaseId">The identifier of the parent use case.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    Task<IReadOnlyList<URS>> GetByUseCaseIdAsync(Guid useCaseId, CancellationToken cancellationToken = default);
}
