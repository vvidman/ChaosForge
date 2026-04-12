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

using ChaosForge.Application.Abstractions;
using ChaosForge.Application.AgentInstances.Commands;
using ChaosForge.Application.RevisionGates.Commands;
using ChaosForge.Application.WorkTasks.Commands;
using ChaosForge.Domain.Entities;
using ChaosForge.Domain.Enums;
using ChaosForge.Domain.Repositories;
using ChaosForge.Infrastructure.Agents;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace ChaosForge.Infrastructure.Tests.Agents;

public sealed class ScrumMasterWorkerTests
{
    private readonly IMediator _mediator = Substitute.For<IMediator>();
    private readonly IProjectRepository _projectRepo = Substitute.For<IProjectRepository>();
    private readonly IUseCaseRepository _useCaseRepo = Substitute.For<IUseCaseRepository>();
    private readonly IURSRepository _ursRepo = Substitute.For<IURSRepository>();
    private readonly ISRSRepository _srsRepo = Substitute.For<ISRSRepository>();
    private readonly IWorkTaskRepository _workTaskRepo = Substitute.For<IWorkTaskRepository>();
    private readonly IRevisionGateRepository _revisionGateRepo = Substitute.For<IRevisionGateRepository>();
    private readonly ILlmProviderSelector _llmSelector = Substitute.For<ILlmProviderSelector>();
    private readonly ILlmProvider _llm = Substitute.For<ILlmProvider>();

    private readonly ScrumMasterWorker _worker;
    private readonly IServiceScope _scope;

    public ScrumMasterWorkerTests()
    {
        var scopeFactory = Substitute.For<IServiceScopeFactory>();
        var options = Options.Create(new AgentWorkerOptions { PollingIntervalMs = 1000 });
        var logger = NullLogger<ScrumMasterWorker>.Instance;

        _worker = new ScrumMasterWorker(scopeFactory, options, logger);

        var provider = Substitute.For<IServiceProvider>();
        provider.GetService(typeof(IMediator)).Returns(_mediator);
        provider.GetService(typeof(IProjectRepository)).Returns(_projectRepo);
        provider.GetService(typeof(IUseCaseRepository)).Returns(_useCaseRepo);
        provider.GetService(typeof(IURSRepository)).Returns(_ursRepo);
        provider.GetService(typeof(ISRSRepository)).Returns(_srsRepo);
        provider.GetService(typeof(IWorkTaskRepository)).Returns(_workTaskRepo);
        provider.GetService(typeof(IRevisionGateRepository)).Returns(_revisionGateRepo);
        provider.GetService(typeof(ILlmProviderSelector)).Returns(_llmSelector);
        provider.GetService(typeof(ILogger<ScrumMasterWorker>)).Returns(logger);

        _scope = Substitute.For<IServiceScope>();
        _scope.ServiceProvider.Returns(provider);

        _llmSelector.GetProviderForRole(AgentRole.ScrumMaster).Returns(_llm);
    }

    [Fact]
    public async Task ExecuteWorkAsync_WhenOpenSprintPlanningGateExists_SkipsCycle()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var instance = new AgentInstance(projectId, AgentRole.ScrumMaster, "Sam");
        var openGate = new RevisionGate(projectId, RevisionGateType.SprintPlanning, "previous output");

        _revisionGateRepo.GetOpenByProjectIdAsync(projectId, Arg.Any<CancellationToken>())
            .Returns(openGate);

        // Act
        await _worker.InvokeExecuteWorkAsync(_scope, instance, CancellationToken.None);

        // Assert — no further calls at all
        await _mediator.DidNotReceive().Send(
            Arg.Any<IRequest<Application.Common.Result>>(),
            Arg.Any<CancellationToken>());
        await _llm.DidNotReceive().CompleteAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteWorkAsync_WhenNoBacklogTasks_SkipsCycleAndLogsWarning()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var instance = new AgentInstance(projectId, AgentRole.ScrumMaster, "Sam");
        var useCase = new UseCase(projectId, "Login", "User logs in", 1);
        var urs = new URS(useCase.Id, "Login URS", "User must be able to log in");
        var srs = new SRS(urs.Id, "Login SRS", "Technical description");

        _revisionGateRepo.GetOpenByProjectIdAsync(projectId, Arg.Any<CancellationToken>())
            .Returns((RevisionGate?)null);
        _useCaseRepo.GetByProjectIdAsync(projectId, Arg.Any<CancellationToken>())
            .Returns(new List<UseCase> { useCase }.AsReadOnly());
        _ursRepo.GetByUseCaseIdAsync(useCase.Id, Arg.Any<CancellationToken>())
            .Returns(new List<URS> { urs }.AsReadOnly());
        _srsRepo.GetByURSIdAsync(urs.Id, Arg.Any<CancellationToken>())
            .Returns(new List<SRS> { srs }.AsReadOnly());
        _workTaskRepo.GetBySRSIdAsync(srs.Id, Arg.Any<CancellationToken>())
            .Returns(new List<WorkTask>().AsReadOnly());

        // Act
        await _worker.InvokeExecuteWorkAsync(_scope, instance, CancellationToken.None);

        // Assert — no LLM call, no commands sent
        await _llm.DidNotReceive().CompleteAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<CancellationToken>());
        await _mediator.DidNotReceive().Send(
            Arg.Any<IRequest<Application.Common.Result>>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteWorkAsync_HappyPath_AssignsOnlySelectedTasksAndOpensGate()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var instance = new AgentInstance(projectId, AgentRole.ScrumMaster, "Sam");
        var project = new Project("Test Project", "A project");
        var useCase = new UseCase(projectId, "Login", "User logs in", 1);
        var urs = new URS(useCase.Id, "Login URS", "User must be able to log in");
        var srs = new SRS(urs.Id, "Login SRS", "Technical description");
        var taskA = new WorkTask(srs.Id, "Task A", "Do A", 3);
        var taskB = new WorkTask(srs.Id, "Task B", "Do B", 2);

        // LLM selects only taskA
        var llmResponse = $$$"""{"sprintTaskIds":["{{{taskA.Id}}}"]}""";

        _revisionGateRepo.GetOpenByProjectIdAsync(projectId, Arg.Any<CancellationToken>())
            .Returns((RevisionGate?)null);
        _projectRepo.GetByIdAsync(projectId, Arg.Any<CancellationToken>())
            .Returns(project);
        _useCaseRepo.GetByProjectIdAsync(projectId, Arg.Any<CancellationToken>())
            .Returns(new List<UseCase> { useCase }.AsReadOnly());
        _ursRepo.GetByUseCaseIdAsync(useCase.Id, Arg.Any<CancellationToken>())
            .Returns(new List<URS> { urs }.AsReadOnly());
        _srsRepo.GetByURSIdAsync(urs.Id, Arg.Any<CancellationToken>())
            .Returns(new List<SRS> { srs }.AsReadOnly());
        _workTaskRepo.GetBySRSIdAsync(srs.Id, Arg.Any<CancellationToken>())
            .Returns(new List<WorkTask> { taskA, taskB }.AsReadOnly());

        _llm.CompleteAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(llmResponse);
        _mediator.Send(Arg.Any<IRequest<Application.Common.Result>>(), Arg.Any<CancellationToken>())
            .Returns(Application.Common.Result.Success());

        // Act
        await _worker.InvokeExecuteWorkAsync(_scope, instance, CancellationToken.None);

        // Assert — StartWork sent
        await _mediator.Received(1).Send(
            Arg.Is<StartAgentWorkCommand>(c => c.AgentInstanceId == instance.Id && c.TaskId == Guid.Empty),
            Arg.Any<CancellationToken>());

        // Assert — only taskA is assigned
        await _mediator.Received(1).Send(
            Arg.Is<AssignWorkTaskToSprintCommand>(c => c.WorkTaskId == taskA.Id),
            Arg.Any<CancellationToken>());
        await _mediator.DidNotReceive().Send(
            Arg.Is<AssignWorkTaskToSprintCommand>(c => c.WorkTaskId == taskB.Id),
            Arg.Any<CancellationToken>());

        // Assert — gate opened
        await _mediator.Received(1).Send(
            Arg.Is<OpenRevisionGateCommand>(c =>
                c.ProjectId == projectId &&
                c.Type == RevisionGateType.SprintPlanning),
            Arg.Any<CancellationToken>());

        // Assert — agent marked Finished
        await _mediator.Received(1).Send(
            Arg.Is<MarkAgentFinishedCommand>(c => c.AgentInstanceId == instance.Id),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteWorkAsync_WhenLlmReturnsInvalidJson_AssignsAllTasksAsFallback()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var instance = new AgentInstance(projectId, AgentRole.ScrumMaster, "Sam");
        var project = new Project("Test Project", "A project");
        var useCase = new UseCase(projectId, "Login", "User logs in", 1);
        var urs = new URS(useCase.Id, "Login URS", "User must be able to log in");
        var srs = new SRS(urs.Id, "Login SRS", "Technical description");
        var taskA = new WorkTask(srs.Id, "Task A", "Do A", 3);
        var taskB = new WorkTask(srs.Id, "Task B", "Do B", 2);

        _revisionGateRepo.GetOpenByProjectIdAsync(projectId, Arg.Any<CancellationToken>())
            .Returns((RevisionGate?)null);
        _projectRepo.GetByIdAsync(projectId, Arg.Any<CancellationToken>())
            .Returns(project);
        _useCaseRepo.GetByProjectIdAsync(projectId, Arg.Any<CancellationToken>())
            .Returns(new List<UseCase> { useCase }.AsReadOnly());
        _ursRepo.GetByUseCaseIdAsync(useCase.Id, Arg.Any<CancellationToken>())
            .Returns(new List<URS> { urs }.AsReadOnly());
        _srsRepo.GetByURSIdAsync(urs.Id, Arg.Any<CancellationToken>())
            .Returns(new List<SRS> { srs }.AsReadOnly());
        _workTaskRepo.GetBySRSIdAsync(srs.Id, Arg.Any<CancellationToken>())
            .Returns(new List<WorkTask> { taskA, taskB }.AsReadOnly());

        _llm.CompleteAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns("this is not {{ valid json");
        _mediator.Send(Arg.Any<IRequest<Application.Common.Result>>(), Arg.Any<CancellationToken>())
            .Returns(Application.Common.Result.Success());

        // Act
        await _worker.InvokeExecuteWorkAsync(_scope, instance, CancellationToken.None);

        // Assert — both tasks assigned (fail-safe)
        await _mediator.Received(1).Send(
            Arg.Is<AssignWorkTaskToSprintCommand>(c => c.WorkTaskId == taskA.Id),
            Arg.Any<CancellationToken>());
        await _mediator.Received(1).Send(
            Arg.Is<AssignWorkTaskToSprintCommand>(c => c.WorkTaskId == taskB.Id),
            Arg.Any<CancellationToken>());

        // Assert — gate still opened
        await _mediator.Received(1).Send(
            Arg.Is<OpenRevisionGateCommand>(c => c.Type == RevisionGateType.SprintPlanning),
            Arg.Any<CancellationToken>());

        // Assert — agent marked Finished
        await _mediator.Received(1).Send(
            Arg.Is<MarkAgentFinishedCommand>(c => c.AgentInstanceId == instance.Id),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteWorkAsync_WhenLlmThrows_BlocksAgentAndDoesNotOpenGate()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var instance = new AgentInstance(projectId, AgentRole.ScrumMaster, "Sam");
        var project = new Project("Test Project", "A project");
        var useCase = new UseCase(projectId, "Login", "User logs in", 1);
        var urs = new URS(useCase.Id, "Login URS", "User must be able to log in");
        var srs = new SRS(urs.Id, "Login SRS", "Technical description");
        var task = new WorkTask(srs.Id, "Task A", "Do A", 3);

        _revisionGateRepo.GetOpenByProjectIdAsync(projectId, Arg.Any<CancellationToken>())
            .Returns((RevisionGate?)null);
        _projectRepo.GetByIdAsync(projectId, Arg.Any<CancellationToken>())
            .Returns(project);
        _useCaseRepo.GetByProjectIdAsync(projectId, Arg.Any<CancellationToken>())
            .Returns(new List<UseCase> { useCase }.AsReadOnly());
        _ursRepo.GetByUseCaseIdAsync(useCase.Id, Arg.Any<CancellationToken>())
            .Returns(new List<URS> { urs }.AsReadOnly());
        _srsRepo.GetByURSIdAsync(urs.Id, Arg.Any<CancellationToken>())
            .Returns(new List<SRS> { srs }.AsReadOnly());
        _workTaskRepo.GetBySRSIdAsync(srs.Id, Arg.Any<CancellationToken>())
            .Returns(new List<WorkTask> { task }.AsReadOnly());

        _llm.CompleteAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("LLM unavailable"));
        _mediator.Send(Arg.Any<IRequest<Application.Common.Result>>(), Arg.Any<CancellationToken>())
            .Returns(Application.Common.Result.Success());

        // Act
        await _worker.InvokeExecuteWorkAsync(_scope, instance, CancellationToken.None);

        // Assert — agent blocked
        await _mediator.Received(1).Send(
            Arg.Is<BlockAgentCommand>(c => c.AgentInstanceId == instance.Id),
            Arg.Any<CancellationToken>());

        // Assert — gate NOT opened
        await _mediator.DidNotReceive().Send(
            Arg.Any<OpenRevisionGateCommand>(),
            Arg.Any<CancellationToken>());

        // Assert — agent NOT marked Finished
        await _mediator.DidNotReceive().Send(
            Arg.Any<MarkAgentFinishedCommand>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteWorkAsync_WhenLlmReturnsUnknownTaskIds_ThoseIdsAreSkipped()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var instance = new AgentInstance(projectId, AgentRole.ScrumMaster, "Sam");
        var project = new Project("Test Project", "A project");
        var useCase = new UseCase(projectId, "Login", "User logs in", 1);
        var urs = new URS(useCase.Id, "Login URS", "User must be able to log in");
        var srs = new SRS(urs.Id, "Login SRS", "Technical description");
        var task = new WorkTask(srs.Id, "Task A", "Do A", 3);
        var unknownId = Guid.NewGuid();

        // LLM returns the real task ID and one unknown ID
        var llmResponse = $$$"""{"sprintTaskIds":["{{{task.Id}}}","{{{unknownId}}}"]}""";

        _revisionGateRepo.GetOpenByProjectIdAsync(projectId, Arg.Any<CancellationToken>())
            .Returns((RevisionGate?)null);
        _projectRepo.GetByIdAsync(projectId, Arg.Any<CancellationToken>())
            .Returns(project);
        _useCaseRepo.GetByProjectIdAsync(projectId, Arg.Any<CancellationToken>())
            .Returns(new List<UseCase> { useCase }.AsReadOnly());
        _ursRepo.GetByUseCaseIdAsync(useCase.Id, Arg.Any<CancellationToken>())
            .Returns(new List<URS> { urs }.AsReadOnly());
        _srsRepo.GetByURSIdAsync(urs.Id, Arg.Any<CancellationToken>())
            .Returns(new List<SRS> { srs }.AsReadOnly());
        _workTaskRepo.GetBySRSIdAsync(srs.Id, Arg.Any<CancellationToken>())
            .Returns(new List<WorkTask> { task }.AsReadOnly());

        _llm.CompleteAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(llmResponse);
        _mediator.Send(Arg.Any<IRequest<Application.Common.Result>>(), Arg.Any<CancellationToken>())
            .Returns(Application.Common.Result.Success());

        // Act
        await _worker.InvokeExecuteWorkAsync(_scope, instance, CancellationToken.None);

        // Assert — only the valid task is assigned; unknown ID is silently skipped
        await _mediator.Received(1).Send(
            Arg.Is<AssignWorkTaskToSprintCommand>(c => c.WorkTaskId == task.Id),
            Arg.Any<CancellationToken>());
        await _mediator.DidNotReceive().Send(
            Arg.Is<AssignWorkTaskToSprintCommand>(c => c.WorkTaskId == unknownId),
            Arg.Any<CancellationToken>());

        // Assert — gate still opened and agent finished
        await _mediator.Received(1).Send(
            Arg.Is<OpenRevisionGateCommand>(c => c.Type == RevisionGateType.SprintPlanning),
            Arg.Any<CancellationToken>());
        await _mediator.Received(1).Send(
            Arg.Is<MarkAgentFinishedCommand>(c => c.AgentInstanceId == instance.Id),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteWorkAsync_WhenDeadlineIsSet_DeadlineAppearsInUserPrompt()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var instance = new AgentInstance(projectId, AgentRole.ScrumMaster, "Sam");
        var deadline = new DateTime(2026, 6, 1);
        var project = new Project("Test Project", "A project", deadline);
        var useCase = new UseCase(projectId, "Login", "User logs in", 1);
        var urs = new URS(useCase.Id, "Login URS", "User must be able to log in");
        var srs = new SRS(urs.Id, "Login SRS", "Technical description");
        var task = new WorkTask(srs.Id, "Task A", "Do A", 3);
        var llmResponse = $$$"""{"sprintTaskIds":["{{{task.Id}}}"]}""";

        _revisionGateRepo.GetOpenByProjectIdAsync(projectId, Arg.Any<CancellationToken>())
            .Returns((RevisionGate?)null);
        _projectRepo.GetByIdAsync(projectId, Arg.Any<CancellationToken>())
            .Returns(project);
        _useCaseRepo.GetByProjectIdAsync(projectId, Arg.Any<CancellationToken>())
            .Returns(new List<UseCase> { useCase }.AsReadOnly());
        _ursRepo.GetByUseCaseIdAsync(useCase.Id, Arg.Any<CancellationToken>())
            .Returns(new List<URS> { urs }.AsReadOnly());
        _srsRepo.GetByURSIdAsync(urs.Id, Arg.Any<CancellationToken>())
            .Returns(new List<SRS> { srs }.AsReadOnly());
        _workTaskRepo.GetBySRSIdAsync(srs.Id, Arg.Any<CancellationToken>())
            .Returns(new List<WorkTask> { task }.AsReadOnly());

        var capturedUserPrompts = new List<string>();
        _llm.CompleteAsync(
                Arg.Any<string>(),
                Arg.Do<string>(p => capturedUserPrompts.Add(p)),
                Arg.Any<CancellationToken>())
            .Returns(llmResponse);
        _mediator.Send(Arg.Any<IRequest<Application.Common.Result>>(), Arg.Any<CancellationToken>())
            .Returns(Application.Common.Result.Success());

        // Act
        await _worker.InvokeExecuteWorkAsync(_scope, instance, CancellationToken.None);

        // Assert — deadline appears in the user prompt
        capturedUserPrompts.Should().ContainSingle();
        capturedUserPrompts[0].Should().Contain("2026-06-01");
    }
}
