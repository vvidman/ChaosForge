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

using ChaosForge.Application.TaskAttempts.Queries;
using ChaosForge.Domain.Repositories;
using FluentAssertions;
using NSubstitute;

namespace ChaosForge.Application.Tests.TaskAttempts.Queries;

public sealed class GetTaskAttemptsByWorkTaskIdQueryHandlerTests
{
    private readonly ITaskAttemptRepository _taskAttemptRepository = Substitute.For<ITaskAttemptRepository>();

    private GetTaskAttemptsByWorkTaskIdQueryHandler CreateHandler() =>
        new(_taskAttemptRepository);

    [Fact]
    public async Task Handle_WhenNoAttemptsExist_ReturnsEmptyList()
    {
        // Arrange
        var workTaskId = Guid.NewGuid();
        _taskAttemptRepository.GetByWorkTaskIdAsync(workTaskId, Arg.Any<CancellationToken>())
            .Returns(new List<Domain.Entities.TaskAttempt>());

        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(new GetTaskAttemptsByWorkTaskIdQuery(workTaskId), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }
}
