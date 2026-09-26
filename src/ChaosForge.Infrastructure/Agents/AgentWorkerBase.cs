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
    /// Called once per cycle for every instance returned by <see cref="ResolveIdleInstancesAsync"/>.
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

    /// <summary>
    /// Runs one polling cycle: every project in <see cref="ActivePhase"/> with an idle instance of
    /// <see cref="Role"/> gets one unit of work. Each instance runs in its own scope, so a project
    /// that cannot make progress (or throws) does not starve the others.
    /// </summary>
    internal async Task RunCycleAsync(CancellationToken ct)
    {
        IReadOnlyList<AgentInstance> instances;

        using (var resolveScope = _scopeFactory.CreateScope())
        {
            instances = await ResolveIdleInstancesAsync(resolveScope, ct);
        }

        foreach (var instance in instances)
        {
            using var scope = _scopeFactory.CreateScope();

            try
            {
                await ExecuteWorkAsync(scope, instance, ct);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(
                    ex,
                    "Unhandled exception in {Role} worker for project {ProjectId}.",
                    Role,
                    instance.ProjectId);
            }
        }
    }

    /// <summary>
    /// Returns the first idle <see cref="AgentInstance"/> matching <see cref="Role"/> for each project
    /// that is currently in <see cref="ActivePhase"/>. Returns an empty list if none exists.
    /// </summary>
    protected async Task<IReadOnlyList<AgentInstance>> ResolveIdleInstancesAsync(IServiceScope scope, CancellationToken ct)
    {
        var projectRepo = scope.ServiceProvider.GetRequiredService<IProjectRepository>();
        var agentRepo = scope.ServiceProvider.GetRequiredService<IAgentInstanceRepository>();

        var projects = await projectRepo.GetAllAsync(ct);
        var idleInstances = new List<AgentInstance>();

        foreach (var project in projects.Where(p => p.Status == ActivePhase))
        {
            var agents = await agentRepo.GetByProjectIdAsync(project.Id, ct);
            var idle = agents.FirstOrDefault(a => a.Role == Role && a.Status == AgentInstanceStatus.Idle);

            if (idle is not null)
            {
                idleInstances.Add(idle);
            }
        }

        return idleInstances;
    }
}
