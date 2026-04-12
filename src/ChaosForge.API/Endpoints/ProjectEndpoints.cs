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
using ChaosForge.Application.Projects.Queries;
using ChaosForge.Domain.Enums;
using MediatR;

namespace ChaosForge.API.Endpoints;

public record CreateProjectRequest(string Name, string Description, DateTime? Deadline);

public record TransitionProjectRequest(ProjectStatus NewStatus);

public record UpdateProjectDescriptionRequest(string Description);

public static class ProjectEndpoints
{
    public static IEndpointRouteBuilder MapProjectEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/projects");

        group.MapGet("/", async (
            IMediator mediator,
            CancellationToken ct) =>
        {
            var result = await mediator.Send(new GetAllProjectsQuery(), ct);

            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(new { error = result.Error });
        });

        group.MapGet("/{id:guid}", async (
            Guid id,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var result = await mediator.Send(new GetProjectByIdQuery(id), ct);

            return result.IsSuccess ? Results.Ok(result.Value) : Results.NotFound(new { error = result.Error });
        });

        group.MapPost("/", async (
            CreateProjectRequest request,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var command = new CreateProjectCommand(request.Name, request.Description, request.Deadline);
            var result = await mediator.Send(command, ct);

            return result.IsSuccess ? Results.Ok() : Results.BadRequest(result.Error);
        });

        group.MapPost("/{id:guid}/transition", async (
            Guid id,
            TransitionProjectRequest request,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var command = new TransitionProjectCommand(id, request.NewStatus);
            var result = await mediator.Send(command, ct);

            return result.IsSuccess ? Results.Ok() : Results.BadRequest(result.Error);
        });

        group.MapPatch("/{id:guid}/description", async (
            Guid id,
            UpdateProjectDescriptionRequest request,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var command = new UpdateProjectDescriptionCommand(id, request.Description);
            var result = await mediator.Send(command, ct);

            return result.IsSuccess ? Results.Ok() : Results.BadRequest(result.Error);
        });

        return app;
    }
}
