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

namespace ChaosForge.Domain.Repositories;

/// <summary>
/// Generic repository contract for basic persistence operations on a domain entity.
/// </summary>
/// <typeparam name="TEntity">The aggregate root type.</typeparam>
/// <typeparam name="TId">The type of the entity's identifier.</typeparam>
public interface IRepository<TEntity, TId>
{
    /// <summary>
    /// Returns the entity with the given <paramref name="id"/>, or <see langword="null"/> if it does not exist.
    /// </summary>
    /// <param name="id">The identifier to look up.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    Task<TEntity?> GetByIdAsync(TId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds <paramref name="entity"/> to the persistence store.
    /// Changes are not committed until <see cref="IUnitOfWork.SaveChangesAsync"/> is called.
    /// </summary>
    /// <param name="entity">The entity to add.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks <paramref name="entity"/> for deletion.
    /// Changes are not committed until <see cref="IUnitOfWork.SaveChangesAsync"/> is called.
    /// </summary>
    /// <param name="entity">The entity to delete.</param>
    void Delete(TEntity entity);
}
