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
using FluentAssertions;

namespace ChaosForge.Domain.Tests.Entities;

public sealed class EntityBaseTests
{
    [Fact]
    public void Constructor_Always_SetsNonEmptyId()
    {
        // Arrange / Act
        var entity = new Project("Name", "Description");

        // Assert
        entity.Id.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void Constructor_Always_SetsCreatedAtToUtc()
    {
        // Arrange
        var before = DateTime.UtcNow;

        // Act
        var entity = new Project("Name", "Description");

        // Assert
        entity.CreatedAt.Kind.Should().Be(DateTimeKind.Utc);
        entity.CreatedAt.Should().BeOnOrAfter(before);
        entity.CreatedAt.Should().BeOnOrBefore(DateTime.UtcNow);
    }
}
