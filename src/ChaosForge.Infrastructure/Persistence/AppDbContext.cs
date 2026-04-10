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

using ChaosForge.Domain.Common;
using ChaosForge.Domain.Entities;
using ChaosForge.Domain.Events;
using ChaosForge.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ChaosForge.Infrastructure.Persistence;

/// <summary>
/// The EF Core database context for the ChaosForge application.
/// Implements <see cref="IUnitOfWork"/> by persisting changes and dispatching domain events.
/// </summary>
public sealed class AppDbContext(
    DbContextOptions<AppDbContext> options,
    IDomainEventDispatcher dispatcher) : DbContext(options), IUnitOfWork
{
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<UseCase> UseCases => Set<UseCase>();
    public DbSet<URS> URSs => Set<URS>();
    public DbSet<SRS> SRSs => Set<SRS>();
    public DbSet<WorkTask> WorkTasks => Set<WorkTask>();
    public DbSet<TaskAttempt> TaskAttempts => Set<TaskAttempt>();
    public DbSet<RevisionGate> RevisionGates => Set<RevisionGate>();
    public DbSet<AgentSlot> AgentSlots => Set<AgentSlot>();
    public DbSet<AgentInstance> AgentInstances => Set<AgentInstance>();

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var result = await base.SaveChangesAsync(cancellationToken);

        var entitiesWithEvents = ChangeTracker
            .Entries<EntityBase<Guid>>()
            .Select(e => e.Entity)
            .Where(e => e.DomainEvents.Count > 0)
            .ToList();

        var events = entitiesWithEvents
            .SelectMany(e => e.DomainEvents)
            .ToList();

        await dispatcher.DispatchAsync(events, cancellationToken);

        foreach (var entity in entitiesWithEvents)
        {
            entity.ClearDomainEvents();
        }

        return result;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
