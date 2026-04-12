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

using ChaosForge.API.Endpoints;
using ChaosForge.Application;
using ChaosForge.Infrastructure;
using ChaosForge.Infrastructure.Hubs;
using ChaosForge.Infrastructure.Persistence;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddOpenApi();
}

var app = builder.Build();

app.UseExceptionHandler(exceptionHandlerApp =>
{
    exceptionHandlerApp.Run(async context =>
    {
        var feature = context.Features.Get<IExceptionHandlerFeature>();
        if (feature?.Error is ValidationException validationException)
        {
            var errors = validationException.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => e.ErrorMessage).ToArray());

            await Results.ValidationProblem(errors).ExecuteAsync(context);

            return;
        }

        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await Results.Problem("An unexpected error occurred.").ExecuteAsync(context);
    });
});

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseCors(policy => policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await context.Database.MigrateAsync();
}

app.MapProjectEndpoints();
app.MapUseCaseEndpoints();
app.MapURSEndpoints();
app.MapSRSEndpoints();
app.MapWorkTaskEndpoints();
app.MapRevisionGateEndpoints();
app.MapAgentSlotEndpoints();
app.MapAgentInstanceEndpoints();
app.MapTaskAttemptEndpoints();

app.MapHub<ChaosForgeHub>("/hubs/chaosforge");

app.Run();
