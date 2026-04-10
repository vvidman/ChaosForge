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

using ChaosForge.Application.URS.Commands;
using ChaosForge.Application.URS.Queries;
using MediatR;

namespace ChaosForge.API.Endpoints;

public record CreateURSRequest(Guid UseCaseId, string Title, string Description);

public record ApplyHumanEditToURSRequest(string EditedDescription, string Note);

public static class URSEndpoints
{
    public static IEndpointRouteBuilder MapURSEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/urs");

        group.MapGet("/by-usecase/{useCaseId:guid}", async (
            Guid useCaseId,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var query = new GetURSsByUseCaseIdQuery(useCaseId);
            var result = await mediator.Send(query, ct);

            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(new { error = result.Error });
        });

        group.MapGet("/{id:guid}", async (
            Guid id,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var query = new GetURSByIdQuery(id);
            var result = await mediator.Send(query, ct);

            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(new { error = result.Error });
        });

        group.MapPost("/", async (
            CreateURSRequest request,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var command = new CreateURSCommand(request.UseCaseId, request.Title, request.Description);
            var result = await mediator.Send(command, ct);

            return result.IsSuccess ? Results.Ok() : Results.BadRequest(new { error = result.Error });
        });

        group.MapPatch("/{id:guid}/human-edit", async (
            Guid id,
            ApplyHumanEditToURSRequest request,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var command = new ApplyHumanEditToURSCommand(id, request.EditedDescription, request.Note);
            var result = await mediator.Send(command, ct);

            return result.IsSuccess ? Results.Ok() : Results.BadRequest(new { error = result.Error });
        });

        return app;
    }
}
