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
using ChaosForge.Domain.Events;
using ChaosForge.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using NSubstitute;

namespace ChaosForge.Infrastructure.Tests.Persistence;

public sealed class AppDbContextSaveChangesTests : IAsyncDisposable
{
    private readonly SqliteConnection _connection;
    private readonly IDomainEventDispatcher _dispatcher = Substitute.For<IDomainEventDispatcher>();

    public AppDbContextSaveChangesTests()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();
    }

    public async ValueTask DisposeAsync()
    {
        await _connection.DisposeAsync();
    }

    private AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .Options;

        return new AppDbContext(options, _dispatcher);
    }

    private async Task EnsureSchemaAsync()
    {
        await using var ctx = CreateContext();
        await ctx.Database.EnsureCreatedAsync();
    }

    [Fact]
    public async Task SaveChangesAsync_WhenEntityHasDomainEvents_DispatchesEventsAfterSave()
    {
        // Arrange
        await EnsureSchemaAsync();
        await using var context = CreateContext();
        var project = new Project("Test Project", "A description");
        project.TransitionTo(ProjectStatus.RequirementsPhase);
        context.Projects.Add(project);

        // Act
        await context.SaveChangesAsync(CancellationToken.None);

        // Assert
        await _dispatcher.Received(1).DispatchAsync(
            Arg.Is<IReadOnlyList<IDomainEvent>>(e => e.Count == 1),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SaveChangesAsync_WhenEntityHasDomainEvents_ClearsDomainEventsAfterDispatch()
    {
        // Arrange
        await EnsureSchemaAsync();
        await using var context = CreateContext();
        var project = new Project("Test Project", "A description");
        project.TransitionTo(ProjectStatus.RequirementsPhase);
        context.Projects.Add(project);

        // Act
        await context.SaveChangesAsync(CancellationToken.None);

        // Assert
        project.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public async Task SaveChangesAsync_WhenEntityHasNoDomainEvents_CallsDispatcherWithEmptyList()
    {
        // Arrange
        await EnsureSchemaAsync();
        await using var context = CreateContext();
        var project = new Project("Test Project", "A description");
        context.Projects.Add(project);

        // Act
        await context.SaveChangesAsync(CancellationToken.None);

        // Assert
        await _dispatcher.Received(1).DispatchAsync(
            Arg.Is<IReadOnlyList<IDomainEvent>>(e => e.Count == 0),
            Arg.Any<CancellationToken>());
    }
}
