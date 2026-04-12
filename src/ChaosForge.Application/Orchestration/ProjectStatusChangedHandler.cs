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

using ChaosForge.Application.AgentInstances.Commands;
using ChaosForge.Domain.Enums;
using ChaosForge.Domain.Events;
using ChaosForge.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ChaosForge.Application.Orchestration;

internal sealed class ProjectStatusChangedHandler(
    IMediator mediator,
    IAgentInstanceRepository agentInstanceRepository,
    ILogger<ProjectStatusChangedHandler> logger)
    : INotificationHandler<ProjectStatusChangedEvent>
{
    private static readonly IReadOnlySet<AgentRole> RequirementsPhaseRoles =
        new HashSet<AgentRole> { AgentRole.BusinessAnalyst };

    private static readonly IReadOnlySet<AgentRole> ArchitecturePhaseRoles =
        new HashSet<AgentRole> { AgentRole.Architect };

    private static readonly IReadOnlySet<AgentRole> SprintPlanningRoles =
        new HashSet<AgentRole> { AgentRole.ScrumMaster };

    private static readonly IReadOnlySet<AgentRole> DevelopmentRoles =
        new HashSet<AgentRole>
        {
            AgentRole.Developer,
            AgentRole.Tester,
            AgentRole.Reviewer,
            AgentRole.TechnicalWriter,
        };

    private static readonly IReadOnlySet<AgentRole> NoRoles = new HashSet<AgentRole>();

    public async Task Handle(ProjectStatusChangedEvent notification, CancellationToken cancellationToken)
    {
        var activeRoles = notification.NewStatus switch
        {
            ProjectStatus.RequirementsPhase => RequirementsPhaseRoles,
            ProjectStatus.ArchitecturePhase => ArchitecturePhaseRoles,
            ProjectStatus.SprintPlanning => SprintPlanningRoles,
            ProjectStatus.Development => DevelopmentRoles,
            _ => NoRoles,
        };

        var instances = await agentInstanceRepository.GetByProjectIdAsync(notification.ProjectId, cancellationToken);

        if (instances.Count == 0)
        {
            logger.LogInformation(
                "No agent instances found for project {ProjectId} — nothing to retire.",
                notification.ProjectId);

            return;
        }

        foreach (var instance in instances)
        {
            if (!activeRoles.Contains(instance.Role) && instance.Status is not AgentInstanceStatus.Finished)
            {
                await mediator.Send(new MarkAgentFinishedCommand(instance.Id), cancellationToken);
            }
        }
    }
}
