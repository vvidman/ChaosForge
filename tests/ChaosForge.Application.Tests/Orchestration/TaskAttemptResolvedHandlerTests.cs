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

using ChaosForge.Application.Orchestration;
using ChaosForge.Domain.Enums;
using ChaosForge.Domain.Events;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;

namespace ChaosForge.Application.Tests.Orchestration;

public sealed class TaskAttemptResolvedHandlerTests
{
    private TaskAttemptResolvedHandler CreateHandler() =>
        new(NullLogger<TaskAttemptResolvedHandler>.Instance);

    [Fact]
    public async Task Handle_WhenResultIsApproved_CompletesWithoutThrowing()
    {
        // Arrange
        var notification = new TaskAttemptResolvedEvent(Guid.NewGuid(), Guid.NewGuid(), AttemptResult.Approved);
        var handler = CreateHandler();

        // Act
        var act = async () => await handler.Handle(notification, CancellationToken.None);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Handle_WhenResultIsRejected_CompletesWithoutThrowing()
    {
        // Arrange
        var notification = new TaskAttemptResolvedEvent(Guid.NewGuid(), Guid.NewGuid(), AttemptResult.Rejected);
        var handler = CreateHandler();

        // Act
        var act = async () => await handler.Handle(notification, CancellationToken.None);

        // Assert
        await act.Should().NotThrowAsync();
    }
}
