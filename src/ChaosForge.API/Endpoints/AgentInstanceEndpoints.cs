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
using ChaosForge.Application.AgentInstances.Queries;
using ChaosForge.Domain.Enums;
using MediatR;

namespace ChaosForge.API.Endpoints;

public record CreateAgentInstanceRequest(Guid ProjectId, AgentRole Role, string PersonaName);

public record StartAgentWorkRequest(Guid TaskId);

public static class AgentInstanceEndpoints
{
    public static IEndpointRouteBuilder MapAgentInstanceEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/agent-instances");

        group.MapGet("/{id:guid}", async (
            Guid id,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var query = new GetAgentInstanceByIdQuery(id);
            var result = await mediator.Send(query, ct);

            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(new { error = result.Error });
        });

        group.MapGet("/by-project/{projectId:guid}", async (
            Guid projectId,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var query = new GetAgentInstancesByProjectIdQuery(projectId);
            var result = await mediator.Send(query, ct);

            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(new { error = result.Error });
        });

        group.MapGet("/by-status/{status}", async (
            AgentInstanceStatus status,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var query = new GetAgentInstancesByStatusQuery(status);
            var result = await mediator.Send(query, ct);

            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(new { error = result.Error });
        });

        group.MapPost("/", async (
            CreateAgentInstanceRequest request,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var command = new CreateAgentInstanceCommand(request.ProjectId, request.Role, request.PersonaName);
            var result = await mediator.Send(command, ct);

            return result.IsSuccess ? Results.Ok() : Results.BadRequest(new { error = result.Error });
        });

        group.MapPost("/{id:guid}/start-work", async (
            Guid id,
            StartAgentWorkRequest request,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var command = new StartAgentWorkCommand(id, request.TaskId);
            var result = await mediator.Send(command, ct);

            return result.IsSuccess ? Results.Ok() : Results.BadRequest(new { error = result.Error });
        });

        group.MapPost("/{id:guid}/finish-work", async (
            Guid id,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var command = new FinishAgentWorkCommand(id);
            var result = await mediator.Send(command, ct);

            return result.IsSuccess ? Results.Ok() : Results.BadRequest(new { error = result.Error });
        });

        group.MapPost("/{id:guid}/block", async (
            Guid id,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var command = new BlockAgentCommand(id);
            var result = await mediator.Send(command, ct);

            return result.IsSuccess ? Results.Ok() : Results.BadRequest(new { error = result.Error });
        });

        group.MapPost("/{id:guid}/mark-finished", async (
            Guid id,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var command = new MarkAgentFinishedCommand(id);
            var result = await mediator.Send(command, ct);

            return result.IsSuccess ? Results.Ok() : Results.BadRequest(new { error = result.Error });
        });

        return app;
    }
}
