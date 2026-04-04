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

using ChaosForge.Domain.Events;

namespace ChaosForge.Domain.Common;

/// <summary>
/// Abstract base class for all domain entities. Provides identity, creation timestamp,
/// and a collection of domain events raised during the entity's lifetime.
/// </summary>
/// <typeparam name="TId">The type of the entity identifier. Must be non-null.</typeparam>
public abstract class EntityBase<TId> where TId : notnull
{
    private readonly List<IDomainEvent> _domainEvents = [];

    /// <summary>
    /// Parameterless constructor required by EF Core. Do not use directly.
    /// </summary>
    protected EntityBase()
    {
        Id = default!;
    }

    /// <summary>
    /// Initializes the entity with the given identifier and sets <see cref="CreatedAt"/> to UTC now.
    /// </summary>
    /// <param name="id">The unique identifier for this entity.</param>
    protected EntityBase(TId id)
    {
        Id = id;
        CreatedAt = DateTime.UtcNow;
    }

    /// <summary>Gets the unique identifier for this entity.</summary>
    public TId Id { get; private set; }

    /// <summary>Gets the UTC timestamp when this entity was created.</summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>Gets the domain events raised by this entity since the last <see cref="ClearDomainEvents"/> call.</summary>
    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    /// <summary>
    /// Records a domain event to be dispatched after the current unit of work completes.
    /// </summary>
    /// <param name="domainEvent">The event to record.</param>
    protected void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    /// <summary>
    /// Removes all recorded domain events. Called by the infrastructure after dispatching.
    /// </summary>
    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}
