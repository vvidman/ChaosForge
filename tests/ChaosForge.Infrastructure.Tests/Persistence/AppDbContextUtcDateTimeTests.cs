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
using ChaosForge.Domain.Events;
using ChaosForge.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using NSubstitute;

namespace ChaosForge.Infrastructure.Tests.Persistence;

public sealed class AppDbContextUtcDateTimeTests : IAsyncDisposable
{
    private readonly SqliteConnection _connection;

    public AppDbContextUtcDateTimeTests()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();
    }

    public async ValueTask DisposeAsync()
    {
        await _connection.DisposeAsync();
    }

    [Fact]
    public async Task ReadingEntity_DateTimeProperties_AreMarkedAsUtc()
    {
        // Arrange
        var deadline = new DateTime(2026, 10, 31, 0, 0, 0, DateTimeKind.Unspecified);
        var project = new Project("Project", "Description", deadline);
        await SaveAsync(project);

        // Act
        await using var context = CreateContext();
        var loaded = await context.Projects.SingleAsync(p => p.Id == project.Id);

        // Assert
        loaded.CreatedAt.Kind.Should().Be(DateTimeKind.Utc);
        loaded.CreatedAt.Should().BeCloseTo(project.CreatedAt, TimeSpan.FromMilliseconds(1));
        loaded.Deadline.Should().NotBeNull();
        loaded.Deadline!.Value.Kind.Should().Be(DateTimeKind.Utc);
        loaded.Deadline.Value.Should().Be(new DateTime(2026, 10, 31, 0, 0, 0, DateTimeKind.Utc));
    }

    [Fact]
    public async Task SavingEntity_WithLocalDateTime_StoresItAsUtc()
    {
        // Arrange
        var localDeadline = new DateTime(2026, 10, 31, 12, 0, 0, DateTimeKind.Local);
        var project = new Project("Project", "Description", localDeadline);
        await SaveAsync(project);

        // Act
        await using var context = CreateContext();
        var loaded = await context.Projects.SingleAsync(p => p.Id == project.Id);

        // Assert
        loaded.Deadline.Should().Be(localDeadline.ToUniversalTime());
    }

    private async Task SaveAsync(Project project)
    {
        await using var context = CreateContext();
        await context.Database.EnsureCreatedAsync();
        context.Projects.Add(project);
        await context.SaveChangesAsync(CancellationToken.None);
    }

    private AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .Options;

        return new AppDbContext(options, Substitute.For<IDomainEventDispatcher>());
    }
}
