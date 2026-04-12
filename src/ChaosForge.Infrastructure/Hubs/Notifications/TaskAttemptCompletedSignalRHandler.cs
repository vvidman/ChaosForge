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

internal sealed class TaskAttemptCompletedSignalRHandler
    : INotificationHandler<TaskAttemptCompletedEvent>
{
    private readonly IHubContext<ChaosForgeHub> _hubContext;
    private readonly ILogger<TaskAttemptCompletedSignalRHandler> _logger;

    public TaskAttemptCompletedSignalRHandler(
        IHubContext<ChaosForgeHub> hubContext,
        ILogger<TaskAttemptCompletedSignalRHandler> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task Handle(TaskAttemptCompletedEvent notification, CancellationToken cancellationToken)
    {
        try
        {
            var message = new SignalRMessage("TaskAttemptCompleted", new
            {
                taskAttemptId = notification.TaskAttemptId,
                workTaskId = notification.WorkTaskId,
                type = notification.Type,
            });

            await _hubContext.Clients.All.SendAsync("ReceiveEvent", message, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SignalR broadcast failed for {Event}", nameof(TaskAttemptCompletedEvent));
        }
    }
}
