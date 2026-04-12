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

using ChaosForge.Domain.Entities;
using ChaosForge.Domain.Enums;
using ChaosForge.Domain.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ChaosForge.Infrastructure.Agents;

/// <summary>
/// Abstract base for all agent background workers. Handles the polling loop, scope management,
/// and idle instance resolution. Concrete agents override <see cref="ExecuteWorkAsync"/> and
/// declare their <see cref="Role"/> and <see cref="ActivePhase"/>.
/// </summary>
public abstract class AgentWorkerBase : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly AgentWorkerOptions _options;
    private readonly ILogger<AgentWorkerBase> _logger;

    protected AgentWorkerBase(
        IServiceScopeFactory scopeFactory,
        IOptions<AgentWorkerOptions> options,
        ILogger<AgentWorkerBase> logger)
    {
        _scopeFactory = scopeFactory;
        _options = options.Value;
        _logger = logger;
    }

    /// <summary>The agent role this worker serves.</summary>
    protected abstract AgentRole Role { get; }

    /// <summary>The project phase during which this worker is active.</summary>
    protected abstract ProjectStatus ActivePhase { get; }

    /// <summary>
    /// Performs one unit of agent work for the given idle instance.
    /// Called only when <see cref="ResolveIdleInstanceAsync"/> returns a non-null instance.
    /// </summary>
    protected abstract Task ExecuteWorkAsync(IServiceScope scope, AgentInstance instance, CancellationToken ct);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await RunCycleAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Unhandled exception in {Role} worker cycle.", Role);
            }

            await Task.Delay(_options.PollingIntervalMs, stoppingToken);
        }
    }

    private async Task RunCycleAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();

        var instance = await ResolveIdleInstanceAsync(scope, ct);
        if (instance is null)
        {
            return;
        }

        await ExecuteWorkAsync(scope, instance, ct);
    }

    /// <summary>
    /// Finds the first idle <see cref="AgentInstance"/> matching <see cref="Role"/> within a project
    /// that is currently in <see cref="ActivePhase"/>. Returns null if no eligible instance exists.
    /// </summary>
    protected async Task<AgentInstance?> ResolveIdleInstanceAsync(IServiceScope scope, CancellationToken ct)
    {
        var projectRepo = scope.ServiceProvider.GetRequiredService<IProjectRepository>();
        var agentRepo = scope.ServiceProvider.GetRequiredService<IAgentInstanceRepository>();

        var projects = await projectRepo.GetAllAsync(ct);

        foreach (var project in projects)
        {
            if (project.Status != ActivePhase)
            {
                continue;
            }

            var agents = await agentRepo.GetByProjectIdAsync(project.Id, ct);
            var idle = agents.FirstOrDefault(a => a.Role == Role && a.Status == AgentInstanceStatus.Idle);

            if (idle is not null)
            {
                return idle;
            }
        }

        return null;
    }
}
