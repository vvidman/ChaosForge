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

using ChaosForge.Application.RevisionGates.Queries;
using ChaosForge.Domain.Entities;
using ChaosForge.Domain.Enums;
using ChaosForge.Domain.Repositories;
using FluentAssertions;
using NSubstitute;

namespace ChaosForge.Application.Tests.RevisionGates.Queries;

public sealed class GetRevisionGatesByProjectIdQueryHandlerTests
{
    private readonly IRevisionGateRepository _revisionGateRepository = Substitute.For<IRevisionGateRepository>();

    private GetRevisionGatesByProjectIdQueryHandler CreateHandler() =>
        new(_revisionGateRepository);

    [Fact]
    public async Task Handle_WhenNoGatesExist_ReturnsSuccessWithEmptyList()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        _revisionGateRepository.GetByProjectIdAsync(projectId, Arg.Any<CancellationToken>())
            .Returns(new List<RevisionGate>().AsReadOnly());

        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(new GetRevisionGatesByProjectIdQuery(projectId), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WhenGatesExist_MapsAllFieldsCorrectly()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var gate = new RevisionGate(projectId, RevisionGateType.Requirements, "Requirements output");

        _revisionGateRepository.GetByProjectIdAsync(projectId, Arg.Any<CancellationToken>())
            .Returns(new List<RevisionGate> { gate }.AsReadOnly());

        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(new GetRevisionGatesByProjectIdQuery(projectId), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().ContainSingle();

        var dto = result.Value![0];
        dto.Id.Should().Be(gate.Id);
        dto.ProjectId.Should().Be(projectId);
        dto.Type.Should().Be(RevisionGateType.Requirements);
        dto.AgentOutput.Should().Be("Requirements output");
        dto.CreatedAt.Should().Be(gate.CreatedAt);
    }
}
