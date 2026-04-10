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

using ChaosForge.Application.AgentSlots.Commands;
using ChaosForge.Application.AgentSlots.Queries;
using ChaosForge.Domain.Enums;
using MediatR;

namespace ChaosForge.API.Endpoints;

public record CreateAgentSlotRequest(Guid ProjectId, AgentRole Role, int Count);

public record UpdateAgentSlotCountRequest(int Count);

public static class AgentSlotEndpoints
{
    public static IEndpointRouteBuilder MapAgentSlotEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/agent-slots");

        group.MapGet("/by-project/{projectId:guid}", async (
            Guid projectId,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var query = new GetAgentSlotsByProjectIdQuery(projectId);
            var result = await mediator.Send(query, ct);

            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(new { error = result.Error });
        });

        group.MapPost("/", async (
            CreateAgentSlotRequest request,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var command = new CreateAgentSlotCommand(request.ProjectId, request.Role, request.Count);
            var result = await mediator.Send(command, ct);

            return result.IsSuccess ? Results.Ok() : Results.BadRequest(new { error = result.Error });
        });

        group.MapPatch("/{id:guid}/count", async (
            Guid id,
            UpdateAgentSlotCountRequest request,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var command = new UpdateAgentSlotCountCommand(id, request.Count);
            var result = await mediator.Send(command, ct);

            return result.IsSuccess ? Results.Ok() : Results.BadRequest(new { error = result.Error });
        });

        return app;
    }
}
