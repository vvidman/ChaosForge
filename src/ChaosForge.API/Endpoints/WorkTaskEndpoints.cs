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

using ChaosForge.Application.WorkTasks.Commands;
using ChaosForge.Application.WorkTasks.Queries;
using ChaosForge.Domain.Enums;
using MediatR;

namespace ChaosForge.API.Endpoints;

public record CreateWorkTaskRequest(Guid SRSId, string Title, string Description, int StoryPoints);

public record AssignWorkTaskToSprintRequest(Guid SprintId);

public static class WorkTaskEndpoints
{
    public static IEndpointRouteBuilder MapWorkTaskEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/worktasks");

        group.MapGet("/{id:guid}", async (
            Guid id,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var query = new GetWorkTaskByIdQuery(id);
            var result = await mediator.Send(query, ct);

            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(new { error = result.Error });
        });

        group.MapGet("/by-srs/{srsId:guid}", async (
            Guid srsId,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var query = new GetWorkTasksBySRSIdQuery(srsId);
            var result = await mediator.Send(query, ct);

            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(new { error = result.Error });
        });

        group.MapGet("/by-sprint/{sprintId:guid}", async (
            Guid sprintId,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var query = new GetWorkTasksBySprintIdQuery(sprintId);
            var result = await mediator.Send(query, ct);

            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(new { error = result.Error });
        });

        group.MapGet("/by-status/{status}", async (
            WorkTaskStatus status,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var query = new GetWorkTasksByStatusQuery(status);
            var result = await mediator.Send(query, ct);

            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(new { error = result.Error });
        });

        group.MapPost("/", async (
            CreateWorkTaskRequest request,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var command = new CreateWorkTaskCommand(request.SRSId, request.Title, request.Description, request.StoryPoints);
            var result = await mediator.Send(command, ct);

            return result.IsSuccess ? Results.Ok() : Results.BadRequest(new { error = result.Error });
        });

        group.MapPost("/{id:guid}/assign-sprint", async (
            Guid id,
            AssignWorkTaskToSprintRequest request,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var command = new AssignWorkTaskToSprintCommand(id, request.SprintId);
            var result = await mediator.Send(command, ct);

            return result.IsSuccess ? Results.Ok() : Results.BadRequest(new { error = result.Error });
        });

        group.MapPost("/{id:guid}/start", async (
            Guid id,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var command = new StartWorkTaskCommand(id);
            var result = await mediator.Send(command, ct);

            return result.IsSuccess ? Results.Ok() : Results.BadRequest(new { error = result.Error });
        });

        group.MapPost("/{id:guid}/send-to-review", async (
            Guid id,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var command = new SendWorkTaskToReviewCommand(id);
            var result = await mediator.Send(command, ct);

            return result.IsSuccess ? Results.Ok() : Results.BadRequest(new { error = result.Error });
        });

        group.MapPost("/{id:guid}/approve", async (
            Guid id,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var command = new ApproveWorkTaskCommand(id);
            var result = await mediator.Send(command, ct);

            return result.IsSuccess ? Results.Ok() : Results.BadRequest(new { error = result.Error });
        });

        group.MapPost("/{id:guid}/pass-testing", async (
            Guid id,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var command = new PassWorkTaskTestingCommand(id);
            var result = await mediator.Send(command, ct);

            return result.IsSuccess ? Results.Ok() : Results.BadRequest(new { error = result.Error });
        });

        group.MapPost("/{id:guid}/complete", async (
            Guid id,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var command = new CompleteWorkTaskCommand(id);
            var result = await mediator.Send(command, ct);

            return result.IsSuccess ? Results.Ok() : Results.BadRequest(new { error = result.Error });
        });

        group.MapPost("/{id:guid}/reject", async (
            Guid id,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var command = new RejectWorkTaskCommand(id);
            var result = await mediator.Send(command, ct);

            return result.IsSuccess ? Results.Ok() : Results.BadRequest(new { error = result.Error });
        });

        return app;
    }
}
