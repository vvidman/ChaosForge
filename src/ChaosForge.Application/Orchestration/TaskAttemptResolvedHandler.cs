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
using MediatR;
using Microsoft.Extensions.Logging;

namespace ChaosForge.Application.Orchestration;

internal sealed class TaskAttemptResolvedHandler(
    ILogger<TaskAttemptResolvedHandler> logger)
    : INotificationHandler<TaskAttemptResolvedEvent>
{
    public Task Handle(TaskAttemptResolvedEvent notification, CancellationToken cancellationToken)
    {
        if (notification.Result is AttemptResult.Approved)
        {
            logger.LogInformation(
                "Task attempt {TaskAttemptId} for work task {WorkTaskId} was approved.",
                notification.TaskAttemptId,
                notification.WorkTaskId);
        }
        else
        {
            logger.LogInformation(
                "Task attempt {TaskAttemptId} for work task {WorkTaskId} was rejected — task returned to Backlog.",
                notification.TaskAttemptId,
                notification.WorkTaskId);
        }

        return Task.CompletedTask;
    }
}
