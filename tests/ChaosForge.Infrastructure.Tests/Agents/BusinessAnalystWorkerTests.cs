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
using ChaosForge.Application.URS.Commands;
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

public sealed class BusinessAnalystWorkerTests
{
    private readonly IMediator _mediator = Substitute.For<IMediator>();
    private readonly IUseCaseRepository _useCaseRepo = Substitute.For<IUseCaseRepository>();
    private readonly IRevisionGateRepository _revisionGateRepo = Substitute.For<IRevisionGateRepository>();
    private readonly ILlmProviderSelector _llmSelector = Substitute.For<ILlmProviderSelector>();
    private readonly ILlmProvider _llm = Substitute.For<ILlmProvider>();

    private readonly BusinessAnalystWorker _worker;
    private readonly IServiceScope _scope;

    public BusinessAnalystWorkerTests()
    {
        var scopeFactory = Substitute.For<IServiceScopeFactory>();
        var options = Options.Create(new AgentWorkerOptions { PollingIntervalMs = 1000 });
        // NullLogger used because ILogger<BusinessAnalystWorker> cannot be proxied by Castle
        // DynamicProxy when the type parameter is internal (strong-named assembly restriction).
        var logger = NullLogger<BusinessAnalystWorker>.Instance;

        _worker = new BusinessAnalystWorker(scopeFactory, options, logger);

        var provider = Substitute.For<IServiceProvider>();
        provider.GetService(typeof(IMediator)).Returns(_mediator);
        provider.GetService(typeof(IUseCaseRepository)).Returns(_useCaseRepo);
        provider.GetService(typeof(IRevisionGateRepository)).Returns(_revisionGateRepo);
        provider.GetService(typeof(ILlmProviderSelector)).Returns(_llmSelector);
        provider.GetService(typeof(ILogger<BusinessAnalystWorker>)).Returns(logger);

        _scope = Substitute.For<IServiceScope>();
        _scope.ServiceProvider.Returns(provider);

        _llmSelector.GetProviderForRole(AgentRole.BusinessAnalyst).Returns(_llm);
    }

    [Fact]
    public async Task ExecuteWorkAsync_HappyPath_SendsCommandsInOrder()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var instance = new AgentInstance(projectId, AgentRole.BusinessAnalyst, "Alex");
        var useCase1 = new UseCase(projectId, "Login", "User logs in", 1);
        var useCase2 = new UseCase(projectId, "Logout", "User logs out", 2);

        _revisionGateRepo.GetOpenByProjectIdAsync(projectId, Arg.Any<CancellationToken>())
            .Returns((RevisionGate?)null);
        _revisionGateRepo.GetByProjectIdAsync(projectId, Arg.Any<CancellationToken>())
            .Returns(new List<RevisionGate>().AsReadOnly());
        _useCaseRepo.GetByProjectIdAsync(projectId, Arg.Any<CancellationToken>())
            .Returns(new List<UseCase> { useCase1, useCase2 }.AsReadOnly());

        _llm.CompleteAsync(Arg.Any<string>(), Arg.Is<string>(s => s.StartsWith("Login")), Arg.Any<CancellationToken>())
            .Returns("URS for Login");
        _llm.CompleteAsync(Arg.Any<string>(), Arg.Is<string>(s => s.StartsWith("Logout")), Arg.Any<CancellationToken>())
            .Returns("URS for Logout");

        _mediator.Send(Arg.Any<IRequest<Application.Common.Result>>(), Arg.Any<CancellationToken>())
            .Returns(Application.Common.Result.Success());

        // Act
        await _worker.InvokeExecuteWorkAsync(_scope, instance, CancellationToken.None);

        // Assert — StartWork with Guid.Empty sentinel
        await _mediator.Received(1).Send(
            Arg.Is<StartAgentWorkCommand>(c => c.AgentInstanceId == instance.Id && c.TaskId == Guid.Empty),
            Arg.Any<CancellationToken>());

        // Assert — one CreateURSCommand per use case
        await _mediator.Received(1).Send(
            Arg.Is<CreateURSCommand>(c => c.UseCaseId == useCase1.Id && c.Description == "URS for Login"),
            Arg.Any<CancellationToken>());
        await _mediator.Received(1).Send(
            Arg.Is<CreateURSCommand>(c => c.UseCaseId == useCase2.Id && c.Description == "URS for Logout"),
            Arg.Any<CancellationToken>());

        // Assert — gate opened with Requirements type
        await _mediator.Received(1).Send(
            Arg.Is<OpenRevisionGateCommand>(c => c.ProjectId == projectId && c.Type == RevisionGateType.Requirements),
            Arg.Any<CancellationToken>());

        // Assert — agent marked Finished
        await _mediator.Received(1).Send(
            Arg.Is<MarkAgentFinishedCommand>(c => c.AgentInstanceId == instance.Id),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteWorkAsync_WhenOpenRequirementsGateExists_SkipsCycle()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var instance = new AgentInstance(projectId, AgentRole.BusinessAnalyst, "Alex");
        var openGate = new RevisionGate(projectId, RevisionGateType.Requirements, "some output");

        _revisionGateRepo.GetOpenByProjectIdAsync(projectId, Arg.Any<CancellationToken>())
            .Returns(openGate);

        // Act
        await _worker.InvokeExecuteWorkAsync(_scope, instance, CancellationToken.None);

        // Assert — no mediator calls at all
        await _mediator.DidNotReceive().Send(
            Arg.Any<IRequest<Application.Common.Result>>(),
            Arg.Any<CancellationToken>());
        await _llm.DidNotReceive().CompleteAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteWorkAsync_WhenLlmThrows_BlocksAgentAndDoesNotOpenGate()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var instance = new AgentInstance(projectId, AgentRole.BusinessAnalyst, "Alex");
        var useCase = new UseCase(projectId, "Login", "User logs in", 1);

        _revisionGateRepo.GetOpenByProjectIdAsync(projectId, Arg.Any<CancellationToken>())
            .Returns((RevisionGate?)null);
        _revisionGateRepo.GetByProjectIdAsync(projectId, Arg.Any<CancellationToken>())
            .Returns(new List<RevisionGate>().AsReadOnly());
        _useCaseRepo.GetByProjectIdAsync(projectId, Arg.Any<CancellationToken>())
            .Returns(new List<UseCase> { useCase }.AsReadOnly());

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
    public async Task ExecuteWorkAsync_WhenNoUseCasesExist_SkipsCycleAndLogsWarning()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var instance = new AgentInstance(projectId, AgentRole.BusinessAnalyst, "Alex");

        _revisionGateRepo.GetOpenByProjectIdAsync(projectId, Arg.Any<CancellationToken>())
            .Returns((RevisionGate?)null);
        _useCaseRepo.GetByProjectIdAsync(projectId, Arg.Any<CancellationToken>())
            .Returns(new List<UseCase>().AsReadOnly());

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
}
