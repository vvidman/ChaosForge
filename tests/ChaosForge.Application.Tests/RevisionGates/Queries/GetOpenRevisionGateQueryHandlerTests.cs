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

public sealed class GetOpenRevisionGateQueryHandlerTests
{
    private readonly IRevisionGateRepository _revisionGateRepository = Substitute.For<IRevisionGateRepository>();

    private GetOpenRevisionGateQueryHandler CreateHandler() =>
        new(_revisionGateRepository);

    [Fact]
    public async Task Handle_WhenNoOpenGateExists_ReturnsFailure()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        _revisionGateRepository.GetOpenByProjectIdAsync(projectId, Arg.Any<CancellationToken>())
            .Returns((RevisionGate?)null);

        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(new GetOpenRevisionGateQuery(projectId), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("No open revision gate found for this project.");
    }

    [Fact]
    public async Task Handle_WhenOpenGateExists_MapsAllFieldsCorrectly()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var gate = new RevisionGate(projectId, RevisionGateType.SprintPlanning, "Sprint plan output");

        _revisionGateRepository.GetOpenByProjectIdAsync(projectId, Arg.Any<CancellationToken>())
            .Returns(gate);

        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(new GetOpenRevisionGateQuery(projectId), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        var dto = result.Value!;
        dto.Id.Should().Be(gate.Id);
        dto.ProjectId.Should().Be(projectId);
        dto.Type.Should().Be(RevisionGateType.SprintPlanning);
        dto.Status.Should().Be(RevisionGateStatus.Open);
        dto.AgentOutput.Should().Be("Sprint plan output");
        dto.HumanEditedOutput.Should().BeNull();
        dto.RejectionReason.Should().BeNull();
        dto.Action.Should().BeNull();
        dto.ResolvedAt.Should().BeNull();
        dto.CreatedAt.Should().Be(gate.CreatedAt);
    }
}
