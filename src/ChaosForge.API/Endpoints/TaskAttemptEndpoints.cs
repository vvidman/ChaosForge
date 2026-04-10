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

using ChaosForge.Application.TaskAttempts.Commands;
using ChaosForge.Application.TaskAttempts.Queries;
using ChaosForge.Domain.Enums;
using MediatR;

namespace ChaosForge.API.Endpoints;

public record CreateTaskAttemptRequest(Guid WorkTaskId, Guid AgentInstanceId, AttemptType Type);

public record CompleteTaskAttemptRequest(string Output);

public record RejectTaskAttemptRequest(string Note);

public static class TaskAttemptEndpoints
{
    public static IEndpointRouteBuilder MapTaskAttemptEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/task-attempts");

        group.MapGet("/{id:guid}", async (
            Guid id,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var query = new GetTaskAttemptByIdQuery(id);
            var result = await mediator.Send(query, ct);

            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(new { error = result.Error });
        });

        group.MapGet("/by-task/{workTaskId:guid}", async (
            Guid workTaskId,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var query = new GetTaskAttemptsByWorkTaskIdQuery(workTaskId);
            var result = await mediator.Send(query, ct);

            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(new { error = result.Error });
        });

        group.MapPost("/", async (
            CreateTaskAttemptRequest request,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var command = new CreateTaskAttemptCommand(request.WorkTaskId, request.AgentInstanceId, request.Type);
            var result = await mediator.Send(command, ct);

            return result.IsSuccess
                ? Results.Ok(new { id = result.Value })
                : Results.BadRequest(new { error = result.Error });
        });

        group.MapPost("/{id:guid}/complete", async (
            Guid id,
            CompleteTaskAttemptRequest request,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var command = new CompleteTaskAttemptCommand(id, request.Output);
            var result = await mediator.Send(command, ct);

            return result.IsSuccess ? Results.Ok() : Results.BadRequest(new { error = result.Error });
        });

        group.MapPost("/{id:guid}/approve", async (
            Guid id,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var command = new ApproveTaskAttemptCommand(id);
            var result = await mediator.Send(command, ct);

            return result.IsSuccess ? Results.Ok() : Results.BadRequest(new { error = result.Error });
        });

        group.MapPost("/{id:guid}/reject", async (
            Guid id,
            RejectTaskAttemptRequest request,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var command = new RejectTaskAttemptCommand(id, request.Note);
            var result = await mediator.Send(command, ct);

            return result.IsSuccess ? Results.Ok() : Results.BadRequest(new { error = result.Error });
        });

        return app;
    }
}
