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

using System.Text.Json.Serialization;
using ChaosForge.Application.Abstractions;
using ChaosForge.Domain.Events;
using ChaosForge.Domain.Repositories;
using ChaosForge.Infrastructure.Agents;
using ChaosForge.Infrastructure.Events;
using ChaosForge.Infrastructure.LLM;
using ChaosForge.Infrastructure.Persistence;
using ChaosForge.Infrastructure.Persistence.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace ChaosForge.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IProjectRepository, ProjectRepository>();
        services.AddScoped<IUseCaseRepository, UseCaseRepository>();
        services.AddScoped<IURSRepository, URSRepository>();
        services.AddScoped<ISRSRepository, SRSRepository>();
        services.AddScoped<IWorkTaskRepository, WorkTaskRepository>();
        services.AddScoped<ITaskAttemptRepository, TaskAttemptRepository>();
        services.AddScoped<IRevisionGateRepository, RevisionGateRepository>();
        services.AddScoped<IAgentSlotRepository, AgentSlotRepository>();
        services.AddScoped<IAgentInstanceRepository, AgentInstanceRepository>();

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<AppDbContext>());

        services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();

        services.AddSignalR().AddJsonProtocol(options =>
        {
            options.PayloadSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        });

        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));

        services.AddInferRouterLlmProvider(configuration);
        services.AddAgentWorkers(configuration);

        return services;
    }

    private const string InferRouterHttpClientName = "InferRouter";

    private static IServiceCollection AddInferRouterLlmProvider(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<InferRouterOptions>(configuration.GetSection("InferRouter"));

        services.AddHttpClient(InferRouterHttpClientName, (sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<InferRouterOptions>>().Value;
            client.BaseAddress = new Uri(options.BaseUrl);
        });

        services.AddKeyedScoped<ILlmProvider>("cloud-preferred", (sp, _) =>
            new InferRouterLlmProvider(
                sp.GetRequiredService<IHttpClientFactory>().CreateClient(InferRouterHttpClientName),
                preferredProviderName: "groq"));

        services.AddKeyedScoped<ILlmProvider>("local-preferred", (sp, _) =>
            new InferRouterLlmProvider(
                sp.GetRequiredService<IHttpClientFactory>().CreateClient(InferRouterHttpClientName),
                preferredProviderName: "local-llama"));

        services.AddScoped<ILlmProviderSelector, LlmProviderSelector>();

        return services;
    }

    private static IServiceCollection AddAgentWorkers(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<AgentWorkerOptions>(configuration.GetSection("Agents"));
        services.AddHostedService<BusinessAnalystWorker>();
        services.AddHostedService<ArchitectWorker>();
        services.AddHostedService<ScrumMasterWorker>();
        services.AddHostedService<DeveloperWorker>();
        services.AddHostedService<ReviewerWorker>();
        services.AddHostedService<TesterWorker>();
        services.AddHostedService<TechnicalWriterWorker>();

        return services;
    }
}
