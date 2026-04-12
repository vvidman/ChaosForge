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

using ChaosForge.Application.UseCases.Commands;
using ChaosForge.Application.UseCases.Queries;
using MediatR;

namespace ChaosForge.API.Endpoints;

public record CreateUseCaseRequest(Guid ProjectId, string Title, string Description, int Priority);

public record UpdateUseCasePriorityRequest(int Priority);

public static class UseCaseEndpoints
{
    public static IEndpointRouteBuilder MapUseCaseEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/usecases");

        group.MapGet("/by-project/{projectId:guid}", async (
            Guid projectId,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var query = new GetUseCasesByProjectIdQuery(projectId);
            var result = await mediator.Send(query, ct);

            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(new { error = result.Error });
        });

        group.MapGet("/{id:guid}", async (
            Guid id,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var query = new GetUseCaseByIdQuery(id);
            var result = await mediator.Send(query, ct);

            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(new { error = result.Error });
        });

        group.MapPost("/", async (
            CreateUseCaseRequest request,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var command = new CreateUseCaseCommand(request.ProjectId, request.Title, request.Description, request.Priority);
            var result = await mediator.Send(command, ct);

            return result.IsSuccess ? Results.Ok() : Results.BadRequest(new { error = result.Error });
        });

        group.MapPatch("/{id:guid}/priority", async (
            Guid id,
            UpdateUseCasePriorityRequest request,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var command = new UpdateUseCasePriorityCommand(id, request.Priority);
            var result = await mediator.Send(command, ct);

            return result.IsSuccess ? Results.Ok() : Results.BadRequest(new { error = result.Error });
        });

        return app;
    }
}
