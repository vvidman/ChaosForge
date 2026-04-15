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

using ChaosForge.Application.Common;
using ChaosForge.Application.Orchestration;
using ChaosForge.Application.Projects.Commands;
using ChaosForge.Domain.Entities;
using ChaosForge.Domain.Enums;
using ChaosForge.Domain.Events;
using ChaosForge.Domain.Repositories;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace ChaosForge.Application.Tests.Orchestration;

public sealed class WorkTaskStatusChangedHandlerTests
{
    private readonly IMediator _mediator = Substitute.For<IMediator>();
    private readonly IProjectRepository _projectRepository = Substitute.For<IProjectRepository>();
    private readonly IWorkTaskRepository _workTaskRepository = Substitute.For<IWorkTaskRepository>();

    private WorkTaskStatusChangedHandler CreateHandler() =>
        new(_mediator, _projectRepository, _workTaskRepository, NullLogger<WorkTaskStatusChangedHandler>.Instance);

    private static Project CreateDevelopmentProject()
    {
        var project = new Project("Test Project", "A test project.");
        project.TransitionTo(ProjectStatus.RequirementsPhase);
        project.TransitionTo(ProjectStatus.ArchitecturePhase);
        project.TransitionTo(ProjectStatus.SprintPlanning);
        project.TransitionTo(ProjectStatus.Development);

        return project;
    }

    private static WorkTask CreateWorkTask(WorkTaskStatus status)
    {
        var srsId = Guid.NewGuid();
        var task = new WorkTask(srsId, "Task", "Description", 3);

        if (status == WorkTaskStatus.Backlog)
        {
            return task;
        }

        task.AssignToSprint(Guid.NewGuid());
        task.Start(); // Backlog → InProgress

        if (status == WorkTaskStatus.InProgress)
        {
            return task;
        }

        task.SendToReview(); // InProgress → InReview

        if (status == WorkTaskStatus.InReview)
        {
            return task;
        }

        task.Approve(); // InReview → InTesting

        if (status == WorkTaskStatus.InTesting)
        {
            return task;
        }

        task.PassTesting(); // InTesting → InDocumentation

        if (status == WorkTaskStatus.InDocumentation)
        {
            return task;
        }

        task.Complete(); // InDocumentation → Done

        return task;
    }

    [Fact]
    public async Task Handle_WhenLastTaskTransitionsToDone_AllTasksDone_SendsCompletionCommand()
    {
        // Arrange
        var project = CreateDevelopmentProject();
        _projectRepository.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(new List<Project> { project });

        IReadOnlyList<WorkTask> allDone = [CreateWorkTask(WorkTaskStatus.Done)];
        _workTaskRepository.GetByProjectIdAsync(project.Id, Arg.Any<CancellationToken>())
            .Returns(allDone);

        _mediator.Send(Arg.Any<TransitionProjectCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        var notification = new WorkTaskStatusChangedEvent(
            Guid.NewGuid(),
            WorkTaskStatus.InDocumentation,
            WorkTaskStatus.Done);
        var handler = CreateHandler();

        // Act
        await handler.Handle(notification, CancellationToken.None);

        // Assert
        await _mediator.Received(1).Send(
            Arg.Is<TransitionProjectCommand>(c =>
                c.ProjectId == project.Id && c.NewStatus == ProjectStatus.Completed),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenTaskTransitionsToDone_OtherTasksStillInProgress_NoCompletionCommand()
    {
        // Arrange
        var project = CreateDevelopmentProject();
        _projectRepository.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(new List<Project> { project });

        IReadOnlyList<WorkTask> mixed =
        [
            CreateWorkTask(WorkTaskStatus.Done),
            CreateWorkTask(WorkTaskStatus.InProgress),
        ];
        _workTaskRepository.GetByProjectIdAsync(project.Id, Arg.Any<CancellationToken>())
            .Returns(mixed);

        var notification = new WorkTaskStatusChangedEvent(
            Guid.NewGuid(),
            WorkTaskStatus.InDocumentation,
            WorkTaskStatus.Done);
        var handler = CreateHandler();

        // Act
        await handler.Handle(notification, CancellationToken.None);

        // Assert
        await _mediator.DidNotReceive().Send(
            Arg.Any<TransitionProjectCommand>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenNewStatusIsNotDone_NoCompletionCheckNoCommand()
    {
        // Arrange
        var notification = new WorkTaskStatusChangedEvent(
            Guid.NewGuid(),
            WorkTaskStatus.Backlog,
            WorkTaskStatus.InProgress);
        var handler = CreateHandler();

        // Act
        await handler.Handle(notification, CancellationToken.None);

        // Assert
        await _projectRepository.DidNotReceive().GetAllAsync(Arg.Any<CancellationToken>());
        await _mediator.DidNotReceive().Send(
            Arg.Any<TransitionProjectCommand>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenProjectAlreadyCompleted_TransitionReturnsFailure_LogsAndDoesNotThrow()
    {
        // Arrange
        var project = CreateDevelopmentProject();
        _projectRepository.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(new List<Project> { project });

        IReadOnlyList<WorkTask> allDone = [CreateWorkTask(WorkTaskStatus.Done)];
        _workTaskRepository.GetByProjectIdAsync(project.Id, Arg.Any<CancellationToken>())
            .Returns(allDone);

        _mediator.Send(Arg.Any<TransitionProjectCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("Cannot transition project from Development to Completed. Only forward sequential transitions are allowed."));

        var notification = new WorkTaskStatusChangedEvent(
            Guid.NewGuid(),
            WorkTaskStatus.InDocumentation,
            WorkTaskStatus.Done);
        var handler = CreateHandler();

        // Act
        var act = async () => await handler.Handle(notification, CancellationToken.None);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Handle_WhenNoDevelopmentProject_DoesNotQueryWorkTasks()
    {
        // Arrange
        _projectRepository.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(new List<Project>());

        var notification = new WorkTaskStatusChangedEvent(
            Guid.NewGuid(),
            WorkTaskStatus.InDocumentation,
            WorkTaskStatus.Done);
        var handler = CreateHandler();

        // Act
        await handler.Handle(notification, CancellationToken.None);

        // Assert
        await _workTaskRepository.DidNotReceive().GetByProjectIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mediator.DidNotReceive().Send(
            Arg.Any<TransitionProjectCommand>(),
            Arg.Any<CancellationToken>());
    }
}
