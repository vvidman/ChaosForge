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

using System.Net.Http.Headers;
using ChaosForge.Application.Abstractions;
using ChaosForge.Domain.Events;
using ChaosForge.Domain.Repositories;
using ChaosForge.Infrastructure.Events;
using ChaosForge.Infrastructure.LLM;
using ChaosForge.Infrastructure.Persistence;
using ChaosForge.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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

        services.AddGroqLlmProvider(configuration);
        services.AddLlamaSharpLlmProvider(configuration);

        return services;
    }

    private static IServiceCollection AddGroqLlmProvider(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<GroqOptions>(configuration.GetSection("Groq"));

        var apiKey = configuration["Groq:ApiKey"] ?? string.Empty;

        services.AddHttpClient<GroqLlmProvider>(client =>
        {
            client.BaseAddress = new Uri("https://api.groq.com");
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", apiKey);
        });

        services.AddScoped<ILlmProvider>(sp => sp.GetRequiredService<GroqLlmProvider>());
        services.AddKeyedScoped<ILlmProvider>("groq", (sp, _) => sp.GetRequiredService<GroqLlmProvider>());

        return services;
    }

    private static IServiceCollection AddLlamaSharpLlmProvider(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<LlamaSharpOptions>(configuration.GetSection("LlamaSharp"));

        services.AddSingleton<LlamaSharpLlmProvider>();
        services.AddKeyedSingleton<ILlmProvider>("llama", (sp, _) => sp.GetRequiredService<LlamaSharpLlmProvider>());

        services.AddScoped<ILlmProviderSelector, LlmProviderSelector>();

        return services;
    }
}
