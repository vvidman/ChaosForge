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

using ChaosForge.Application.RevisionGates.Commands;
using ChaosForge.Application.RevisionGates.Queries;
using ChaosForge.Domain.Enums;
using MediatR;

namespace ChaosForge.API.Endpoints;

public record OpenRevisionGateRequest(Guid ProjectId, RevisionGateType Type, string AgentOutput);

public record EditAndAcceptRevisionGateRequest(string EditedOutput);

public record RejectRevisionGateRequest(string Reason);

public static class RevisionGateEndpoints
{
    public static IEndpointRouteBuilder MapRevisionGateEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/revision-gates");

        group.MapGet("/{id:guid}", async (
            Guid id,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var query = new GetRevisionGateByIdQuery(id);
            var result = await mediator.Send(query, ct);

            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(new { error = result.Error });
        });

        group.MapGet("/by-project/{projectId:guid}", async (
            Guid projectId,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var query = new GetRevisionGatesByProjectIdQuery(projectId);
            var result = await mediator.Send(query, ct);

            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(new { error = result.Error });
        });

        group.MapGet("/open/by-project/{projectId:guid}", async (
            Guid projectId,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var query = new GetOpenRevisionGateQuery(projectId);
            var result = await mediator.Send(query, ct);

            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(new { error = result.Error });
        });

        group.MapPost("/", async (
            OpenRevisionGateRequest request,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var command = new OpenRevisionGateCommand(request.ProjectId, request.Type, request.AgentOutput);
            var result = await mediator.Send(command, ct);

            return result.IsSuccess ? Results.Ok() : Results.BadRequest(new { error = result.Error });
        });

        group.MapPost("/{id:guid}/accept", async (
            Guid id,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var command = new AcceptRevisionGateCommand(id);
            var result = await mediator.Send(command, ct);

            return result.IsSuccess ? Results.Ok() : Results.BadRequest(new { error = result.Error });
        });

        group.MapPost("/{id:guid}/edit-and-accept", async (
            Guid id,
            EditAndAcceptRevisionGateRequest request,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var command = new EditAndAcceptRevisionGateCommand(id, request.EditedOutput);
            var result = await mediator.Send(command, ct);

            return result.IsSuccess ? Results.Ok() : Results.BadRequest(new { error = result.Error });
        });

        group.MapPost("/{id:guid}/reject", async (
            Guid id,
            RejectRevisionGateRequest request,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var command = new RejectRevisionGateCommand(id, request.Reason);
            var result = await mediator.Send(command, ct);

            return result.IsSuccess ? Results.Ok() : Results.BadRequest(new { error = result.Error });
        });

        return app;
    }
}
