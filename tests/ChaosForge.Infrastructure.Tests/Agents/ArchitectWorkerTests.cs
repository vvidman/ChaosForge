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
using ChaosForge.Application.SRS.Commands;
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

public sealed class ArchitectWorkerTests
{
    private readonly IMediator _mediator = Substitute.For<IMediator>();
    private readonly IUseCaseRepository _useCaseRepo = Substitute.For<IUseCaseRepository>();
    private readonly IURSRepository _ursRepo = Substitute.For<IURSRepository>();
    private readonly ISRSRepository _srsRepo = Substitute.For<ISRSRepository>();
    private readonly IRevisionGateRepository _revisionGateRepo = Substitute.For<IRevisionGateRepository>();
    private readonly ILlmProviderSelector _llmSelector = Substitute.For<ILlmProviderSelector>();
    private readonly ILlmProvider _llm = Substitute.For<ILlmProvider>();

    private readonly ArchitectWorker _worker;
    private readonly IServiceScope _scope;

    public ArchitectWorkerTests()
    {
        var scopeFactory = Substitute.For<IServiceScopeFactory>();
        var options = Options.Create(new AgentWorkerOptions { PollingIntervalMs = 1000 });
        var logger = NullLogger<ArchitectWorker>.Instance;

        _worker = new ArchitectWorker(scopeFactory, options, logger);

        var provider = Substitute.For<IServiceProvider>();
        provider.GetService(typeof(IMediator)).Returns(_mediator);
        provider.GetService(typeof(IUseCaseRepository)).Returns(_useCaseRepo);
        provider.GetService(typeof(IURSRepository)).Returns(_ursRepo);
        provider.GetService(typeof(ISRSRepository)).Returns(_srsRepo);
        provider.GetService(typeof(IRevisionGateRepository)).Returns(_revisionGateRepo);
        provider.GetService(typeof(ILlmProviderSelector)).Returns(_llmSelector);
        provider.GetService(typeof(ILogger<ArchitectWorker>)).Returns(logger);

        _scope = Substitute.For<IServiceScope>();
        _scope.ServiceProvider.Returns(provider);

        _llmSelector.GetProviderForRole(AgentRole.Architect).Returns(_llm);
    }

    [Fact]
    public async Task ExecuteWorkAsync_WhenOpenAfterArchitectGateExists_SkipsCycle()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var instance = new AgentInstance(projectId, AgentRole.Architect, "Eve");
        var openGate = new RevisionGate(projectId, RevisionGateType.Architecture, "some output");

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
    public async Task ExecuteWorkAsync_WhenUrsListIsEmpty_SkipsCycle()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var instance = new AgentInstance(projectId, AgentRole.Architect, "Eve");
        var useCase = new UseCase(projectId, "Login", "User logs in", 1);

        _revisionGateRepo.GetOpenByProjectIdAsync(projectId, Arg.Any<CancellationToken>())
            .Returns((RevisionGate?)null);
        _useCaseRepo.GetByProjectIdAsync(projectId, Arg.Any<CancellationToken>())
            .Returns(new List<UseCase> { useCase }.AsReadOnly());
        _ursRepo.GetByUseCaseIdAsync(useCase.Id, Arg.Any<CancellationToken>())
            .Returns(new List<URS>().AsReadOnly());

        // Act
        await _worker.InvokeExecuteWorkAsync(_scope, instance, CancellationToken.None);

        // Assert — no LLM calls, no mediator commands
        await _llm.DidNotReceive().CompleteAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<CancellationToken>());
        await _mediator.DidNotReceive().Send(
            Arg.Any<IRequest<Application.Common.Result>>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteWorkAsync_HappyPath_SendsCreateSrsAndWorkTaskCommandsAndOpensGate()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var instance = new AgentInstance(projectId, AgentRole.Architect, "Eve");
        var useCase = new UseCase(projectId, "Login", "User logs in", 1);
        var urs = new URS(useCase.Id, "Login URS", "User must be able to log in");
        var srs = new SRS(urs.Id, "Login URS", "Technical implementation of login");
        var validTaskJson = """[{"title":"Task A","description":"Do A","storyPoints":3}]""";

        _revisionGateRepo.GetOpenByProjectIdAsync(projectId, Arg.Any<CancellationToken>())
            .Returns((RevisionGate?)null);
        _revisionGateRepo.GetByProjectIdAsync(projectId, Arg.Any<CancellationToken>())
            .Returns(new List<RevisionGate>().AsReadOnly());
        _useCaseRepo.GetByProjectIdAsync(projectId, Arg.Any<CancellationToken>())
            .Returns(new List<UseCase> { useCase }.AsReadOnly());
        _ursRepo.GetByUseCaseIdAsync(useCase.Id, Arg.Any<CancellationToken>())
            .Returns(new List<URS> { urs }.AsReadOnly());
        _srsRepo.GetByURSIdAsync(urs.Id, Arg.Any<CancellationToken>())
            .Returns(new List<SRS> { srs }.AsReadOnly());

        _llm.CompleteAsync(Arg.Any<string>(), Arg.Is<string>(s => s.Contains("Login URS")), Arg.Any<CancellationToken>())
            .Returns("Technical implementation of login");
        _llm.CompleteAsync(Arg.Any<string>(), Arg.Is<string>(s => s.Contains("Technical implementation")), Arg.Any<CancellationToken>())
            .Returns(validTaskJson);

        _mediator.Send(Arg.Any<IRequest<Application.Common.Result>>(), Arg.Any<CancellationToken>())
            .Returns(Application.Common.Result.Success());

        // Act
        await _worker.InvokeExecuteWorkAsync(_scope, instance, CancellationToken.None);

        // Assert — StartWork sent
        await _mediator.Received(1).Send(
            Arg.Is<StartAgentWorkCommand>(c => c.AgentInstanceId == instance.Id && c.TaskId == Guid.Empty),
            Arg.Any<CancellationToken>());

        // Assert — CreateSRSCommand sent for the URS
        await _mediator.Received(1).Send(
            Arg.Is<CreateSRSCommand>(c => c.URSId == urs.Id),
            Arg.Any<CancellationToken>());

        // Assert — CreateWorkTaskCommand sent for the parsed task
        await _mediator.Received(1).Send(
            Arg.Is<CreateWorkTaskCommand>(c =>
                c.SRSId == srs.Id &&
                c.Title == "Task A" &&
                c.StoryPoints == 3),
            Arg.Any<CancellationToken>());

        // Assert — gate opened with AfterArchitect type
        await _mediator.Received(1).Send(
            Arg.Is<OpenRevisionGateCommand>(c =>
                c.ProjectId == projectId &&
                c.Type == RevisionGateType.Architecture),
            Arg.Any<CancellationToken>());

        // Assert — agent marked Finished
        await _mediator.Received(1).Send(
            Arg.Is<MarkAgentFinishedCommand>(c => c.AgentInstanceId == instance.Id),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteWorkAsync_WhenUrsHasHumanEditNote_NoteAppearsInSrsPrompt()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var instance = new AgentInstance(projectId, AgentRole.Architect, "Eve");
        var useCase = new UseCase(projectId, "Login", "User logs in", 1);
        var urs = new URS(useCase.Id, "Login URS", "User must be able to log in");
        urs.ApplyHumanEdit("Revised: User must log in with MFA", "Please add MFA support");
        var srs = new SRS(urs.Id, "Login URS", "Technical implementation with MFA");
        var validTaskJson = """[{"title":"MFA Task","description":"Implement MFA","storyPoints":5}]""";

        _revisionGateRepo.GetOpenByProjectIdAsync(projectId, Arg.Any<CancellationToken>())
            .Returns((RevisionGate?)null);
        _revisionGateRepo.GetByProjectIdAsync(projectId, Arg.Any<CancellationToken>())
            .Returns(new List<RevisionGate>().AsReadOnly());
        _useCaseRepo.GetByProjectIdAsync(projectId, Arg.Any<CancellationToken>())
            .Returns(new List<UseCase> { useCase }.AsReadOnly());
        _ursRepo.GetByUseCaseIdAsync(useCase.Id, Arg.Any<CancellationToken>())
            .Returns(new List<URS> { urs }.AsReadOnly());
        _srsRepo.GetByURSIdAsync(urs.Id, Arg.Any<CancellationToken>())
            .Returns(new List<SRS> { srs }.AsReadOnly());

        // Capture all user prompts (arg index 1); return values for Pass 1 then Pass 2
        var capturedUserPrompts = new List<string>();
        _llm.CompleteAsync(
                Arg.Any<string>(),
                Arg.Do<string>(p => capturedUserPrompts.Add(p)),
                Arg.Any<CancellationToken>())
            .Returns("Technical implementation with MFA", validTaskJson);

        _mediator.Send(Arg.Any<IRequest<Application.Common.Result>>(), Arg.Any<CancellationToken>())
            .Returns(Application.Common.Result.Success());

        // Act
        await _worker.InvokeExecuteWorkAsync(_scope, instance, CancellationToken.None);

        // Assert — first call is SRS generation; it must include the HumanEditNote
        capturedUserPrompts.Should().NotBeEmpty();
        capturedUserPrompts[0].Should().Contain("Please add MFA support");
    }

    [Fact]
    public async Task ExecuteWorkAsync_WhenWorkTaskJsonIsInvalid_LogsAndSkipsSrs_CycleCompletes()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var instance = new AgentInstance(projectId, AgentRole.Architect, "Eve");
        var useCase = new UseCase(projectId, "Login", "User logs in", 1);
        var urs = new URS(useCase.Id, "Login URS", "User must be able to log in");
        var srs = new SRS(urs.Id, "Login URS", "Technical implementation of login");

        _revisionGateRepo.GetOpenByProjectIdAsync(projectId, Arg.Any<CancellationToken>())
            .Returns((RevisionGate?)null);
        _revisionGateRepo.GetByProjectIdAsync(projectId, Arg.Any<CancellationToken>())
            .Returns(new List<RevisionGate>().AsReadOnly());
        _useCaseRepo.GetByProjectIdAsync(projectId, Arg.Any<CancellationToken>())
            .Returns(new List<UseCase> { useCase }.AsReadOnly());
        _ursRepo.GetByUseCaseIdAsync(useCase.Id, Arg.Any<CancellationToken>())
            .Returns(new List<URS> { urs }.AsReadOnly());
        _srsRepo.GetByURSIdAsync(urs.Id, Arg.Any<CancellationToken>())
            .Returns(new List<SRS> { srs }.AsReadOnly());

        _llm.CompleteAsync(Arg.Any<string>(), Arg.Is<string>(s => s.Contains("Login URS")), Arg.Any<CancellationToken>())
            .Returns("Technical implementation of login");
        _llm.CompleteAsync(Arg.Any<string>(), Arg.Is<string>(s => s.Contains("Technical implementation")), Arg.Any<CancellationToken>())
            .Returns("this is not valid json {{ broken");

        _mediator.Send(Arg.Any<IRequest<Application.Common.Result>>(), Arg.Any<CancellationToken>())
            .Returns(Application.Common.Result.Success());

        // Act
        await _worker.InvokeExecuteWorkAsync(_scope, instance, CancellationToken.None);

        // Assert — no WorkTask created
        await _mediator.DidNotReceive().Send(
            Arg.Any<CreateWorkTaskCommand>(),
            Arg.Any<CancellationToken>());

        // Assert — gate still opened (cycle completed)
        await _mediator.Received(1).Send(
            Arg.Is<OpenRevisionGateCommand>(c => c.Type == RevisionGateType.Architecture),
            Arg.Any<CancellationToken>());

        // Assert — agent marked Finished
        await _mediator.Received(1).Send(
            Arg.Is<MarkAgentFinishedCommand>(c => c.AgentInstanceId == instance.Id),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteWorkAsync_WhenLlmFailsOnSrsPass_BlocksAgentAndDoesNotOpenGate()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var instance = new AgentInstance(projectId, AgentRole.Architect, "Eve");
        var useCase = new UseCase(projectId, "Login", "User logs in", 1);
        var urs = new URS(useCase.Id, "Login URS", "User must be able to log in");

        _revisionGateRepo.GetOpenByProjectIdAsync(projectId, Arg.Any<CancellationToken>())
            .Returns((RevisionGate?)null);
        _revisionGateRepo.GetByProjectIdAsync(projectId, Arg.Any<CancellationToken>())
            .Returns(new List<RevisionGate>().AsReadOnly());
        _useCaseRepo.GetByProjectIdAsync(projectId, Arg.Any<CancellationToken>())
            .Returns(new List<UseCase> { useCase }.AsReadOnly());
        _ursRepo.GetByUseCaseIdAsync(useCase.Id, Arg.Any<CancellationToken>())
            .Returns(new List<URS> { urs }.AsReadOnly());

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
    public async Task ExecuteWorkAsync_WhenStoryPointsMissingOrZero_DefaultsToOne()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var instance = new AgentInstance(projectId, AgentRole.Architect, "Eve");
        var useCase = new UseCase(projectId, "Login", "User logs in", 1);
        var urs = new URS(useCase.Id, "Login URS", "User must be able to log in");
        var srs = new SRS(urs.Id, "Login URS", "Technical implementation of login");

        // storyPoints = 0 → should default to 1
        var jsonWithZeroPoints = """[{"title":"Task Zero","description":"Zero points task","storyPoints":0}]""";

        _revisionGateRepo.GetOpenByProjectIdAsync(projectId, Arg.Any<CancellationToken>())
            .Returns((RevisionGate?)null);
        _revisionGateRepo.GetByProjectIdAsync(projectId, Arg.Any<CancellationToken>())
            .Returns(new List<RevisionGate>().AsReadOnly());
        _useCaseRepo.GetByProjectIdAsync(projectId, Arg.Any<CancellationToken>())
            .Returns(new List<UseCase> { useCase }.AsReadOnly());
        _ursRepo.GetByUseCaseIdAsync(useCase.Id, Arg.Any<CancellationToken>())
            .Returns(new List<URS> { urs }.AsReadOnly());
        _srsRepo.GetByURSIdAsync(urs.Id, Arg.Any<CancellationToken>())
            .Returns(new List<SRS> { srs }.AsReadOnly());

        _llm.CompleteAsync(Arg.Any<string>(), Arg.Is<string>(s => s.Contains("Login URS")), Arg.Any<CancellationToken>())
            .Returns("Technical implementation of login");
        _llm.CompleteAsync(Arg.Any<string>(), Arg.Is<string>(s => s.Contains("Technical implementation")), Arg.Any<CancellationToken>())
            .Returns(jsonWithZeroPoints);

        _mediator.Send(Arg.Any<IRequest<Application.Common.Result>>(), Arg.Any<CancellationToken>())
            .Returns(Application.Common.Result.Success());

        // Act
        await _worker.InvokeExecuteWorkAsync(_scope, instance, CancellationToken.None);

        // Assert — story points defaulted to 1
        await _mediator.Received(1).Send(
            Arg.Is<CreateWorkTaskCommand>(c => c.Title == "Task Zero" && c.StoryPoints == 1),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteWorkAsync_WhenPriorRejectionExists_RejectionReasonInSystemPrompt()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var instance = new AgentInstance(projectId, AgentRole.Architect, "Eve");
        var useCase = new UseCase(projectId, "Login", "User logs in", 1);
        var urs = new URS(useCase.Id, "Login URS", "User must be able to log in");
        var srs = new SRS(urs.Id, "Login URS", "Technical implementation of login");
        var validTaskJson = """[{"title":"Task","description":"Do it","storyPoints":2}]""";

        var rejectedGate = new RevisionGate(projectId, RevisionGateType.Architecture, "previous output");
        rejectedGate.Reject("The SRS lacked detail");

        _revisionGateRepo.GetOpenByProjectIdAsync(projectId, Arg.Any<CancellationToken>())
            .Returns((RevisionGate?)null);
        _revisionGateRepo.GetByProjectIdAsync(projectId, Arg.Any<CancellationToken>())
            .Returns(new List<RevisionGate> { rejectedGate }.AsReadOnly());
        _useCaseRepo.GetByProjectIdAsync(projectId, Arg.Any<CancellationToken>())
            .Returns(new List<UseCase> { useCase }.AsReadOnly());
        _ursRepo.GetByUseCaseIdAsync(useCase.Id, Arg.Any<CancellationToken>())
            .Returns(new List<URS> { urs }.AsReadOnly());
        _srsRepo.GetByURSIdAsync(urs.Id, Arg.Any<CancellationToken>())
            .Returns(new List<SRS> { srs }.AsReadOnly());

        // Capture all system prompts (arg index 0); return values for Pass 1 then Pass 2
        var capturedSystemPrompts = new List<string>();
        _llm.CompleteAsync(
                Arg.Do<string>(p => capturedSystemPrompts.Add(p)),
                Arg.Any<string>(),
                Arg.Any<CancellationToken>())
            .Returns("Technical implementation of login", validTaskJson);

        _mediator.Send(Arg.Any<IRequest<Application.Common.Result>>(), Arg.Any<CancellationToken>())
            .Returns(Application.Common.Result.Success());

        // Act
        await _worker.InvokeExecuteWorkAsync(_scope, instance, CancellationToken.None);

        // Assert — first call is SRS generation; its system prompt must contain the rejection reason
        capturedSystemPrompts.Should().NotBeEmpty();
        capturedSystemPrompts[0].Should().Contain("The SRS lacked detail");
    }
}
