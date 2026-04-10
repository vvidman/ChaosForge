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

using ChaosForge.Application.SRS.Commands;
using ChaosForge.Application.SRS.Queries;
using MediatR;

namespace ChaosForge.API.Endpoints;

public record CreateSRSRequest(Guid URSId, string Title, string TechnicalDescription);

public record ApplyHumanEditToSRSRequest(string EditedDescription, string Note);

public static class SRSEndpoints
{
    public static IEndpointRouteBuilder MapSRSEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/srs");

        group.MapGet("/by-urs/{ursId:guid}", async (
            Guid ursId,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var query = new GetSRSsByURSIdQuery(ursId);
            var result = await mediator.Send(query, ct);

            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(new { error = result.Error });
        });

        group.MapGet("/{id:guid}", async (
            Guid id,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var query = new GetSRSByIdQuery(id);
            var result = await mediator.Send(query, ct);

            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(new { error = result.Error });
        });

        group.MapPost("/", async (
            CreateSRSRequest request,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var command = new CreateSRSCommand(request.URSId, request.Title, request.TechnicalDescription);
            var result = await mediator.Send(command, ct);

            return result.IsSuccess ? Results.Ok() : Results.BadRequest(new { error = result.Error });
        });

        group.MapPatch("/{id:guid}/human-edit", async (
            Guid id,
            ApplyHumanEditToSRSRequest request,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var command = new ApplyHumanEditToSRSCommand(id, request.EditedDescription, request.Note);
            var result = await mediator.Send(command, ct);

            return result.IsSuccess ? Results.Ok() : Results.BadRequest(new { error = result.Error });
        });

        return app;
    }
}
