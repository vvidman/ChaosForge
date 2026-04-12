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

using ChaosForge.Domain.Enums;
using ChaosForge.Domain.Events;
using ChaosForge.Infrastructure.Hubs;
using ChaosForge.Infrastructure.Hubs.Notifications;
using FluentAssertions;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace ChaosForge.Infrastructure.Tests.Hubs.Notifications;

public sealed class ProjectStatusChangedSignalRHandlerTests
{
    private readonly IHubContext<ChaosForgeHub> _hubContext = Substitute.For<IHubContext<ChaosForgeHub>>();
    private readonly IHubClients _hubClients = Substitute.For<IHubClients>();
    private readonly IClientProxy _clientProxy = Substitute.For<IClientProxy>();

    public ProjectStatusChangedSignalRHandlerTests()
    {
        _hubContext.Clients.Returns(_hubClients);
        _hubClients.All.Returns(_clientProxy);
    }

    private ProjectStatusChangedSignalRHandler CreateHandler() =>
        new(_hubContext, NullLogger<ProjectStatusChangedSignalRHandler>.Instance);

    [Fact]
    public async Task Handle_Always_SendsReceiveEventWithCorrectTypeString()
    {
        // Arrange
        var notification = new ProjectStatusChangedEvent(
            Guid.NewGuid(),
            ProjectStatus.RequirementsPhase,
            ProjectStatus.ArchitecturePhase);
        var handler = CreateHandler();

        // Act
        await handler.Handle(notification, CancellationToken.None);

        // Assert
        await _clientProxy.Received(1).SendCoreAsync(
            "ReceiveEvent",
            Arg.Is<object[]>(args =>
                args.Length == 1 &&
                ((SignalRMessage)args[0]).Type == "ProjectStatusChanged"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Always_PayloadContainsCorrectFields()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var notification = new ProjectStatusChangedEvent(
            projectId,
            ProjectStatus.RequirementsPhase,
            ProjectStatus.ArchitecturePhase);
        var handler = CreateHandler();
        SignalRMessage? captured = null;
        await _clientProxy.SendCoreAsync(
            Arg.Any<string>(),
            Arg.Do<object[]>(args => captured = (SignalRMessage)args[0]),
            Arg.Any<CancellationToken>());

        // Act
        await handler.Handle(notification, CancellationToken.None);

        // Assert
        captured.Should().NotBeNull();
        var payload = captured!.Payload;
        var payloadType = payload.GetType();
        payloadType.GetProperty("projectId")!.GetValue(payload).Should().Be(projectId);
        payloadType.GetProperty("oldStatus")!.GetValue(payload).Should().Be(ProjectStatus.RequirementsPhase);
        payloadType.GetProperty("newStatus")!.GetValue(payload).Should().Be(ProjectStatus.ArchitecturePhase);
    }

    [Fact]
    public async Task Handle_WhenSendAsyncThrows_DoesNotPropagate()
    {
        // Arrange
        _clientProxy.SendCoreAsync(Arg.Any<string>(), Arg.Any<object[]>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("SignalR transport failure"));
        var notification = new ProjectStatusChangedEvent(
            Guid.NewGuid(),
            ProjectStatus.RequirementsPhase,
            ProjectStatus.ArchitecturePhase);
        var handler = CreateHandler();

        // Act
        var act = async () => await handler.Handle(notification, CancellationToken.None);

        // Assert
        await act.Should().NotThrowAsync();
    }
}
