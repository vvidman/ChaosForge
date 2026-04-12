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

using ChaosForge.Application.Projects.Commands;
using ChaosForge.Application.WorkTasks.Queries;
using ChaosForge.Domain.Enums;
using ChaosForge.Domain.Events;
using ChaosForge.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ChaosForge.Application.Orchestration;

internal sealed class WorkTaskStatusChangedHandler(
    IMediator mediator,
    IProjectRepository projectRepository,
    ILogger<WorkTaskStatusChangedHandler> logger)
    : INotificationHandler<WorkTaskStatusChangedEvent>
{
    private static readonly WorkTaskStatus[] NonDoneStatuses =
    [
        WorkTaskStatus.Backlog,
        WorkTaskStatus.InProgress,
        WorkTaskStatus.InReview,
        WorkTaskStatus.InTesting,
        WorkTaskStatus.InDocumentation,
    ];

    public async Task Handle(WorkTaskStatusChangedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Work task {WorkTaskId} transitioned from {OldStatus} to {NewStatus}.",
            notification.WorkTaskId,
            notification.OldStatus,
            notification.NewStatus);

        if (notification.NewStatus is not WorkTaskStatus.Done)
        {
            return;
        }

        var projects = await projectRepository.GetAllAsync(cancellationToken);
        var developmentProject = projects.FirstOrDefault(p => p.Status is ProjectStatus.Development);

        if (developmentProject is null)
        {
            logger.LogDebug(
                "Work task {WorkTaskId} completed but no project is currently in Development phase — skipping completion check.",
                notification.WorkTaskId);

            return;
        }

        foreach (var status in NonDoneStatuses)
        {
            var result = await mediator.Send(new GetWorkTasksByStatusQuery(status), cancellationToken);

            if (result.IsSuccess && result.Value is { Count: > 0 })
            {
                return;
            }
        }

        logger.LogInformation(
            "All work tasks are done — transitioning project {ProjectId} to Completed.",
            developmentProject.Id);

        var transitionResult = await mediator.Send(
            new TransitionProjectCommand(developmentProject.Id, ProjectStatus.Completed),
            cancellationToken);

        if (!transitionResult.IsSuccess)
        {
            logger.LogWarning(
                "Failed to transition project {ProjectId} to Completed: {Error}",
                developmentProject.Id,
                transitionResult.Error);
        }
    }
}
