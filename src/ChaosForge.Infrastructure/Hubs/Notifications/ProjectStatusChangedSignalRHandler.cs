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
using MediatR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace ChaosForge.Infrastructure.Hubs.Notifications;

internal sealed class ProjectStatusChangedSignalRHandler
    : INotificationHandler<ProjectStatusChangedEvent>
{
    private readonly IHubContext<ChaosForgeHub> _hubContext;
    private readonly ILogger<ProjectStatusChangedSignalRHandler> _logger;

    public ProjectStatusChangedSignalRHandler(
        IHubContext<ChaosForgeHub> hubContext,
        ILogger<ProjectStatusChangedSignalRHandler> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task Handle(ProjectStatusChangedEvent notification, CancellationToken cancellationToken)
    {
        try
        {
            var message = new SignalRMessage("ProjectStatusChanged", new
            {
                projectId = notification.ProjectId,
                oldStatus = notification.OldStatus,
                newStatus = notification.NewStatus,
            });

            await _hubContext.Clients.All.SendAsync("ReceiveEvent", message, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SignalR broadcast failed for {Event}", nameof(ProjectStatusChangedEvent));
        }
    }
}
