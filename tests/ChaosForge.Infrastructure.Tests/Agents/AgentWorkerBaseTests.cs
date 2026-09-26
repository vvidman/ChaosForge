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
using ChaosForge.Infrastructure.Agents;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace ChaosForge.Infrastructure.Tests.Agents;

public sealed class AgentWorkerBaseTests
{
    private readonly IProjectRepository _projectRepo = Substitute.For<IProjectRepository>();
    private readonly IAgentInstanceRepository _agentRepo = Substitute.For<IAgentInstanceRepository>();
    private readonly RecordingWorker _worker;

    public AgentWorkerBaseTests()
    {
        var provider = Substitute.For<IServiceProvider>();
        provider.GetService(typeof(IProjectRepository)).Returns(_projectRepo);
        provider.GetService(typeof(IAgentInstanceRepository)).Returns(_agentRepo);

        var scope = Substitute.For<IServiceScope>();
        scope.ServiceProvider.Returns(provider);

        var scopeFactory = Substitute.For<IServiceScopeFactory>();
        scopeFactory.CreateScope().Returns(scope);

        _worker = new RecordingWorker(scopeFactory);
    }

    [Fact]
    public async Task RunCycleAsync_WithIdleInstancesInSeveralActiveProjects_ExecutesWorkForEachProject()
    {
        // Arrange
        var stuckProject = ProjectIn(ProjectStatus.ArchitecturePhase);
        var newProject = ProjectIn(ProjectStatus.ArchitecturePhase);
        var stuckAgent = new AgentInstance(stuckProject.Id, AgentRole.Architect, "Stuck");
        var newAgent = new AgentInstance(newProject.Id, AgentRole.Architect, "New");

        _projectRepo.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(new List<Project> { stuckProject, newProject }.AsReadOnly());
        _agentRepo.GetByProjectIdAsync(stuckProject.Id, Arg.Any<CancellationToken>())
            .Returns(new List<AgentInstance> { stuckAgent }.AsReadOnly());
        _agentRepo.GetByProjectIdAsync(newProject.Id, Arg.Any<CancellationToken>())
            .Returns(new List<AgentInstance> { newAgent }.AsReadOnly());

        // Act
        await _worker.RunCycleAsync(CancellationToken.None);

        // Assert
        _worker.ExecutedInstanceIds.Should().Equal(stuckAgent.Id, newAgent.Id);
    }

    [Fact]
    public async Task RunCycleAsync_WhenWorkThrowsForOneProject_StillExecutesTheOthers()
    {
        // Arrange
        var failingProject = ProjectIn(ProjectStatus.ArchitecturePhase);
        var healthyProject = ProjectIn(ProjectStatus.ArchitecturePhase);
        var failingAgent = new AgentInstance(failingProject.Id, AgentRole.Architect, "Failing");
        var healthyAgent = new AgentInstance(healthyProject.Id, AgentRole.Architect, "Healthy");
        _worker.FailFor = failingAgent.Id;

        _projectRepo.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(new List<Project> { failingProject, healthyProject }.AsReadOnly());
        _agentRepo.GetByProjectIdAsync(failingProject.Id, Arg.Any<CancellationToken>())
            .Returns(new List<AgentInstance> { failingAgent }.AsReadOnly());
        _agentRepo.GetByProjectIdAsync(healthyProject.Id, Arg.Any<CancellationToken>())
            .Returns(new List<AgentInstance> { healthyAgent }.AsReadOnly());

        // Act
        var act = () => _worker.RunCycleAsync(CancellationToken.None);

        // Assert
        await act.Should().NotThrowAsync();
        _worker.ExecutedInstanceIds.Should().Equal(failingAgent.Id, healthyAgent.Id);
    }

    [Fact]
    public async Task RunCycleAsync_IgnoresProjectsInOtherPhasesAndNonIdleOrOtherRoleAgents()
    {
        // Arrange
        var otherPhase = ProjectIn(ProjectStatus.RequirementsPhase);
        var activeProject = ProjectIn(ProjectStatus.ArchitecturePhase);
        var busyArchitect = new AgentInstance(activeProject.Id, AgentRole.Architect, "Busy");
        busyArchitect.StartWork(Guid.Empty);
        var idleAnalyst = new AgentInstance(activeProject.Id, AgentRole.BusinessAnalyst, "Analyst");

        _projectRepo.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(new List<Project> { otherPhase, activeProject }.AsReadOnly());
        _agentRepo.GetByProjectIdAsync(activeProject.Id, Arg.Any<CancellationToken>())
            .Returns(new List<AgentInstance> { busyArchitect, idleAnalyst }.AsReadOnly());

        // Act
        await _worker.RunCycleAsync(CancellationToken.None);

        // Assert
        _worker.ExecutedInstanceIds.Should().BeEmpty();
        await _agentRepo.DidNotReceive().GetByProjectIdAsync(otherPhase.Id, Arg.Any<CancellationToken>());
    }

    private static Project ProjectIn(ProjectStatus status)
    {
        var project = new Project("Project", "Description", DateTime.UtcNow.AddDays(30));

        foreach (var next in Enum.GetValues<ProjectStatus>().Where(s => s > ProjectStatus.Setup && s <= status))
        {
            project.TransitionTo(next);
        }

        return project;
    }

    private sealed class RecordingWorker(IServiceScopeFactory scopeFactory)
        : AgentWorkerBase(
            scopeFactory,
            Options.Create(new AgentWorkerOptions { PollingIntervalMs = 1000 }),
            NullLogger<AgentWorkerBase>.Instance)
    {
        public List<Guid> ExecutedInstanceIds { get; } = [];

        public Guid? FailFor { get; set; }

        protected override AgentRole Role => AgentRole.Architect;

        protected override ProjectStatus ActivePhase => ProjectStatus.ArchitecturePhase;

        protected override Task ExecuteWorkAsync(IServiceScope scope, AgentInstance instance, CancellationToken ct)
        {
            ExecutedInstanceIds.Add(instance.Id);

            return instance.Id == FailFor
                ? throw new InvalidOperationException("Simulated failure")
                : Task.CompletedTask;
        }
    }
}
