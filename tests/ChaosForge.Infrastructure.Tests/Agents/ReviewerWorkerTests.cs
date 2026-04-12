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
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace ChaosForge.Infrastructure.Tests.Agents;

public sealed class ReviewerWorkerTests
{
    private readonly IMediator _mediator = Substitute.For<IMediator>();
    private readonly IUseCaseRepository _useCaseRepo = Substitute.For<IUseCaseRepository>();
    private readonly IURSRepository _ursRepo = Substitute.For<IURSRepository>();
    private readonly ISRSRepository _srsRepo = Substitute.For<ISRSRepository>();
    private readonly IWorkTaskRepository _workTaskRepo = Substitute.For<IWorkTaskRepository>();
    private readonly ITaskAttemptRepository _taskAttemptRepo = Substitute.For<ITaskAttemptRepository>();
    private readonly ILlmProviderSelector _llmSelector = Substitute.For<ILlmProviderSelector>();
    private readonly ILlmProvider _llm = Substitute.For<ILlmProvider>();

    private readonly ReviewerWorker _worker;
    private readonly IServiceScope _scope;

    public ReviewerWorkerTests()
    {
        var scopeFactory = Substitute.For<IServiceScopeFactory>();
        var options = Options.Create(new AgentWorkerOptions { PollingIntervalMs = 1000 });
        var logger = NullLogger<ReviewerWorker>.Instance;

        _worker = new ReviewerWorker(scopeFactory, options, logger);

        var provider = Substitute.For<IServiceProvider>();
        provider.GetService(typeof(IMediator)).Returns(_mediator);
        provider.GetService(typeof(IUseCaseRepository)).Returns(_useCaseRepo);
        provider.GetService(typeof(IURSRepository)).Returns(_ursRepo);
        provider.GetService(typeof(ISRSRepository)).Returns(_srsRepo);
        provider.GetService(typeof(IWorkTaskRepository)).Returns(_workTaskRepo);
        provider.GetService(typeof(ITaskAttemptRepository)).Returns(_taskAttemptRepo);
        provider.GetService(typeof(ILlmProviderSelector)).Returns(_llmSelector);
        provider.GetService(typeof(ILogger<ReviewerWorker>)).Returns(logger);

        _scope = Substitute.For<IServiceScope>();
        _scope.ServiceProvider.Returns(provider);

        _llmSelector.GetProviderForRole(AgentRole.Reviewer).Returns(_llm);
    }

    private (Guid projectId, AgentInstance instance, SRS srs) SetUpProjectHierarchy()
    {
        var projectId = Guid.NewGuid();
        var instance = new AgentInstance(projectId, AgentRole.Reviewer, "Rev");
        var useCase = new UseCase(projectId, "Login", "User logs in", 1);
        var urs = new URS(useCase.Id, "Login URS", "Requirements");
        var srs = new SRS(urs.Id, "Login SRS", "Technical description");

        _useCaseRepo.GetByProjectIdAsync(projectId, Arg.Any<CancellationToken>())
            .Returns(new List<UseCase> { useCase }.AsReadOnly());
        _ursRepo.GetByUseCaseIdAsync(useCase.Id, Arg.Any<CancellationToken>())
            .Returns(new List<URS> { urs }.AsReadOnly());
        _srsRepo.GetByURSIdAsync(urs.Id, Arg.Any<CancellationToken>())
            .Returns(new List<SRS> { srs }.AsReadOnly());

        return (projectId, instance, srs);
    }

    private WorkTask MakeInReviewTask(Guid srsId)
    {
        var task = new WorkTask(srsId, "Implement Login", "Wire up auth", 3);
        task.AssignToSprint(Guid.NewGuid());
        task.Start();
        task.SendToReview();

        return task;
    }

    [Fact]
    public async Task ExecuteWorkAsync_HappyPath_ExecutesApproveCommandSequence()
    {
        // Arrange
        var (_, instance, srs) = SetUpProjectHierarchy();
        var task = MakeInReviewTask(srs.Id);

        _workTaskRepo.GetBySRSIdAsync(srs.Id, Arg.Any<CancellationToken>())
            .Returns(new List<WorkTask> { task }.AsReadOnly());

        _taskAttemptRepo.GetByWorkTaskIdAsync(task.Id, Arg.Any<CancellationToken>())
            .Returns(new List<TaskAttempt>().AsReadOnly());

        _llm.CompleteAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns("Looks good, approved.");

        var attemptId = Guid.NewGuid();
        _mediator.Send(Arg.Any<IRequest<Result<Guid>>>(), Arg.Any<CancellationToken>())
            .Returns(Result<Guid>.Success(attemptId));
        _mediator.Send(Arg.Any<IRequest<Result>>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        // Act
        await _worker.InvokeExecuteWorkAsync(_scope, instance, CancellationToken.None);

        // Assert — attempt created with Review type
        await _mediator.Received(1).Send(
            Arg.Is<CreateTaskAttemptCommand>(c =>
                c.WorkTaskId == task.Id &&
                c.Type == AttemptType.Review),
            Arg.Any<CancellationToken>());

        // Assert — attempt approved
        await _mediator.Received(1).Send(
            Arg.Is<ApproveTaskAttemptCommand>(c => c.TaskAttemptId == attemptId),
            Arg.Any<CancellationToken>());

        // Assert — work task approved
        await _mediator.Received(1).Send(
            Arg.Is<ApproveWorkTaskCommand>(c => c.WorkTaskId == task.Id),
            Arg.Any<CancellationToken>());

        // Assert — agent released
        await _mediator.Received(1).Send(
            Arg.Is<FinishAgentWorkCommand>(c => c.AgentInstanceId == instance.Id),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteWorkAsync_WhenLlmThrows_ReleasesAgentAndLeavesTaskInCurrentStatus()
    {
        // Arrange
        var (_, instance, srs) = SetUpProjectHierarchy();
        var task = MakeInReviewTask(srs.Id);

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

        // Assert — no approve commands sent
        await _mediator.DidNotReceive().Send(
            Arg.Any<ApproveTaskAttemptCommand>(),
            Arg.Any<CancellationToken>());
        await _mediator.DidNotReceive().Send(
            Arg.Any<ApproveWorkTaskCommand>(),
            Arg.Any<CancellationToken>());
    }
}
