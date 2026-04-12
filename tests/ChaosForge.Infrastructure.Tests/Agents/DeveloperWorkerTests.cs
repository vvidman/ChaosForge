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
using ChaosForge.Application.Common;
using ChaosForge.Application.TaskAttempts.Commands;
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

public sealed class DeveloperWorkerTests
{
    private readonly IMediator _mediator = Substitute.For<IMediator>();
    private readonly IUseCaseRepository _useCaseRepo = Substitute.For<IUseCaseRepository>();
    private readonly IURSRepository _ursRepo = Substitute.For<IURSRepository>();
    private readonly ISRSRepository _srsRepo = Substitute.For<ISRSRepository>();
    private readonly IWorkTaskRepository _workTaskRepo = Substitute.For<IWorkTaskRepository>();
    private readonly ITaskAttemptRepository _taskAttemptRepo = Substitute.For<ITaskAttemptRepository>();
    private readonly ILlmProviderSelector _llmSelector = Substitute.For<ILlmProviderSelector>();
    private readonly ILlmProvider _llm = Substitute.For<ILlmProvider>();

    private readonly DeveloperWorker _worker;
    private readonly IServiceScope _scope;

    public DeveloperWorkerTests()
    {
        var scopeFactory = Substitute.For<IServiceScopeFactory>();
        var options = Options.Create(new AgentWorkerOptions { PollingIntervalMs = 1000 });
        var logger = NullLogger<DeveloperWorker>.Instance;

        _worker = new DeveloperWorker(scopeFactory, options, logger);

        var provider = Substitute.For<IServiceProvider>();
        provider.GetService(typeof(IMediator)).Returns(_mediator);
        provider.GetService(typeof(IUseCaseRepository)).Returns(_useCaseRepo);
        provider.GetService(typeof(IURSRepository)).Returns(_ursRepo);
        provider.GetService(typeof(ISRSRepository)).Returns(_srsRepo);
        provider.GetService(typeof(IWorkTaskRepository)).Returns(_workTaskRepo);
        provider.GetService(typeof(ITaskAttemptRepository)).Returns(_taskAttemptRepo);
        provider.GetService(typeof(ILlmProviderSelector)).Returns(_llmSelector);
        provider.GetService(typeof(ILogger<DeveloperWorker>)).Returns(logger);

        _scope = Substitute.For<IServiceScope>();
        _scope.ServiceProvider.Returns(provider);

        _llmSelector.GetProviderForRole(AgentRole.Developer).Returns(_llm);
    }

    // --- Shared helpers ---

    private (Guid projectId, AgentInstance instance, UseCase useCase, URS urs, SRS srs) SetUpProjectHierarchy()
    {
        var projectId = Guid.NewGuid();
        var instance = new AgentInstance(projectId, AgentRole.Developer, "Dev");
        var useCase = new UseCase(projectId, "Login", "User logs in", 1);
        var urs = new URS(useCase.Id, "Login URS", "User must be able to log in");
        var srs = new SRS(urs.Id, "Login SRS", "Technical description");

        _useCaseRepo.GetByProjectIdAsync(projectId, Arg.Any<CancellationToken>())
            .Returns(new List<UseCase> { useCase }.AsReadOnly());
        _ursRepo.GetByUseCaseIdAsync(useCase.Id, Arg.Any<CancellationToken>())
            .Returns(new List<URS> { urs }.AsReadOnly());
        _srsRepo.GetByURSIdAsync(urs.Id, Arg.Any<CancellationToken>())
            .Returns(new List<SRS> { srs }.AsReadOnly());

        return (projectId, instance, useCase, urs, srs);
    }

    // --- Tests ---

    [Fact]
    public async Task ExecuteWorkAsync_WhenNoEligibleTask_SkipsCycle()
    {
        // Arrange
        var (projectId, instance, _, _, srs) = SetUpProjectHierarchy();

        _workTaskRepo.GetBySRSIdAsync(srs.Id, Arg.Any<CancellationToken>())
            .Returns(new List<WorkTask>().AsReadOnly());

        // Act
        await _worker.InvokeExecuteWorkAsync(_scope, instance, CancellationToken.None);

        // Assert — no commands sent and LLM not called
        await _mediator.DidNotReceive().Send(
            Arg.Any<IRequest<Result>>(),
            Arg.Any<CancellationToken>());
        await _llm.DidNotReceive().CompleteAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteWorkAsync_WhenTaskHasNullSprintId_SkipsItAndIdlesCycle()
    {
        // Arrange
        var (projectId, instance, _, _, srs) = SetUpProjectHierarchy();

        // A Backlog task with no sprint assigned — Developer must skip it
        var taskNoSprint = new WorkTask(srs.Id, "Unassigned Task", "Not in a sprint", 2);

        _workTaskRepo.GetBySRSIdAsync(srs.Id, Arg.Any<CancellationToken>())
            .Returns(new List<WorkTask> { taskNoSprint }.AsReadOnly());

        // Act
        await _worker.InvokeExecuteWorkAsync(_scope, instance, CancellationToken.None);

        // Assert — nothing sent
        await _mediator.DidNotReceive().Send(
            Arg.Any<IRequest<Result>>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteWorkAsync_HappyPath_ExecutesCorrectCommandSequence()
    {
        // Arrange
        var (projectId, instance, _, _, srs) = SetUpProjectHierarchy();

        var task = new WorkTask(srs.Id, "Implement Login", "Wire up auth endpoint", 3);
        task.AssignToSprint(Guid.NewGuid());

        _workTaskRepo.GetBySRSIdAsync(srs.Id, Arg.Any<CancellationToken>())
            .Returns(new List<WorkTask> { task }.AsReadOnly());

        _taskAttemptRepo.GetByWorkTaskIdAsync(task.Id, Arg.Any<CancellationToken>())
            .Returns(new List<TaskAttempt>().AsReadOnly());

        _llm.CompleteAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns("## Implementation\n```csharp\n// code\n```");

        var attemptId = Guid.NewGuid();
        _mediator.Send(Arg.Any<IRequest<Result<Guid>>>(), Arg.Any<CancellationToken>())
            .Returns(Result<Guid>.Success(attemptId));
        _mediator.Send(Arg.Any<IRequest<Result>>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        // Act
        await _worker.InvokeExecuteWorkAsync(_scope, instance, CancellationToken.None);

        // Assert — StartWork with task ID
        await _mediator.Received(1).Send(
            Arg.Is<StartAgentWorkCommand>(c => c.AgentInstanceId == instance.Id && c.TaskId == task.Id),
            Arg.Any<CancellationToken>());

        // Assert — CreateTaskAttempt with Implementation type
        await _mediator.Received(1).Send(
            Arg.Is<CreateTaskAttemptCommand>(c =>
                c.WorkTaskId == task.Id &&
                c.AgentInstanceId == instance.Id &&
                c.Type == AttemptType.Implementation),
            Arg.Any<CancellationToken>());

        // Assert — CompleteTaskAttempt
        await _mediator.Received(1).Send(
            Arg.Is<CompleteTaskAttemptCommand>(c => c.TaskAttemptId == attemptId),
            Arg.Any<CancellationToken>());

        // Assert — StartWorkTask
        await _mediator.Received(1).Send(
            Arg.Is<StartWorkTaskCommand>(c => c.WorkTaskId == task.Id),
            Arg.Any<CancellationToken>());

        // Assert — SendWorkTaskToReview
        await _mediator.Received(1).Send(
            Arg.Is<SendWorkTaskToReviewCommand>(c => c.WorkTaskId == task.Id),
            Arg.Any<CancellationToken>());

        // Assert — FinishAgentWork
        await _mediator.Received(1).Send(
            Arg.Is<FinishAgentWorkCommand>(c => c.AgentInstanceId == instance.Id),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteWorkAsync_WhenPriorRejectedReviewAttemptExists_PromptContainsRejectionContext()
    {
        // Arrange
        var (projectId, instance, _, _, srs) = SetUpProjectHierarchy();

        var task = new WorkTask(srs.Id, "Implement Login", "Wire up auth endpoint", 3);
        task.AssignToSprint(Guid.NewGuid());

        _workTaskRepo.GetBySRSIdAsync(srs.Id, Arg.Any<CancellationToken>())
            .Returns(new List<WorkTask> { task }.AsReadOnly());

        // Create a prior rejected Review attempt — the developer sees why the previous
        // implementation failed review so it can improve the next attempt.
        var priorAttempt = new TaskAttempt(task.Id, Guid.NewGuid(), AttemptType.Review);
        priorAttempt.Complete("old output");
        priorAttempt.Reject("Does not handle null input correctly.");

        _taskAttemptRepo.GetByWorkTaskIdAsync(task.Id, Arg.Any<CancellationToken>())
            .Returns(new List<TaskAttempt> { priorAttempt }.AsReadOnly());

        var capturedUserPrompts = new List<string>();
        _llm.CompleteAsync(
                Arg.Any<string>(),
                Arg.Do<string>(p => capturedUserPrompts.Add(p)),
                Arg.Any<CancellationToken>())
            .Returns("implementation output");

        _mediator.Send(Arg.Any<IRequest<Result<Guid>>>(), Arg.Any<CancellationToken>())
            .Returns(Result<Guid>.Success(Guid.NewGuid()));
        _mediator.Send(Arg.Any<IRequest<Result>>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        // Act
        await _worker.InvokeExecuteWorkAsync(_scope, instance, CancellationToken.None);

        // Assert — prompt contains prior attempt output
        capturedUserPrompts.Should().ContainSingle();
        capturedUserPrompts[0].Should().Contain("old output");
        capturedUserPrompts[0].Should().Contain("Previous Attempt");
    }

    [Fact]
    public async Task ExecuteWorkAsync_WhenLlmThrows_ReleasesAgentAndLeavesTaskInCurrentStatus()
    {
        // Arrange
        var (projectId, instance, _, _, srs) = SetUpProjectHierarchy();

        var task = new WorkTask(srs.Id, "Implement Login", "Wire up auth endpoint", 3);
        task.AssignToSprint(Guid.NewGuid());

        _workTaskRepo.GetBySRSIdAsync(srs.Id, Arg.Any<CancellationToken>())
            .Returns(new List<WorkTask> { task }.AsReadOnly());

        _taskAttemptRepo.GetByWorkTaskIdAsync(task.Id, Arg.Any<CancellationToken>())
            .Returns(new List<TaskAttempt>().AsReadOnly());

        _llm.CompleteAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("LLM unavailable"));

        _mediator.Send(Arg.Any<IRequest<Result>>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        // Act
        await _worker.InvokeExecuteWorkAsync(_scope, instance, CancellationToken.None);

        // Assert — agent released
        await _mediator.Received(1).Send(
            Arg.Is<FinishAgentWorkCommand>(c => c.AgentInstanceId == instance.Id),
            Arg.Any<CancellationToken>());

        // Assert — no attempt created and task NOT advanced
        await _mediator.DidNotReceive().Send(
            Arg.Any<CreateTaskAttemptCommand>(),
            Arg.Any<CancellationToken>());
        await _mediator.DidNotReceive().Send(
            Arg.Any<StartWorkTaskCommand>(),
            Arg.Any<CancellationToken>());
        await _mediator.DidNotReceive().Send(
            Arg.Any<SendWorkTaskToReviewCommand>(),
            Arg.Any<CancellationToken>());

        // Assert — task is still Backlog
        task.Status.Should().Be(WorkTaskStatus.Backlog);
    }
}
