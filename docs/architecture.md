# ChaosForge – Architecture Decisions

## Clean Architecture Layer Rules

```
Domain ← Application ← Infrastructure
                  ↑
                 API
```

### Domain Layer (ChaosForge.Domain)
- Entities: Project, UseCase, URS, SRS, WorkTask, TaskAttempt, Sprint, AgentSlot, AgentInstance, RevisionGate
- Enums: ProjectStatus, AgentRole, WorkTaskStatus, AttemptType, AttemptResult, RevisionGateAction
- Domain Events: TaskAssigned, TaskCompleted, TaskRejected, RevisionGateOpened, RevisionGateResolved
- Interfaces: ILLMProvider, IProjectRepository, IWorkTaskRepository, IUnitOfWork, IDomainEventDispatcher
- Domain Services: OrchestratorService, ButterflyService (ripple effect triggered by EditAndAccept)
- NO external NuGet dependencies

### Application Layer (ChaosForge.Application)
- MediatR Commands: CreateProjectCommand, StartSprintCommand, ResolveRevisionGateCommand, AssignTaskCommand
- MediatR Queries: GetBacklogQuery, GetSprintBoardQuery, GetAgentActivityQuery, GetProjectQuery
- Validators: FluentValidation for all commands
- DTOs: separate request/response records per use case
- Depends only on Domain

### Infrastructure Layer (ChaosForge.Infrastructure)
- EF Core DbContext + SQLite configuration
- Repository implementations (EfCoreProjectRepository, EfCoreWorkTaskRepository, etc.)
- LlamaSharpProvider, GroqProvider, OpenAICompatibleProvider
- AgentWorkerService (BackgroundService) — polls for available tasks per role
- SignalR event dispatcher — publishes domain events to hub
- Prompt templates per AgentRole

### API Layer (ChaosForge.API)
- Controllers: ProjectController, TaskController, RevisionGateController
- SignalR Hub: ChaosForgeHub
- Thin layer only: mediator.Send() + SignalR notifications
- No business logic

---

## Key Design Decisions

### Why CQRS with MediatR?
Each agent action maps cleanly to a Command. Query side stays fast and independent.
Enables future event sourcing without major refactoring.

### Why BackgroundService for agents?
Agent workers poll for available tasks independently. This models true parallelism —
multiple developers can pick up tasks simultaneously without coordination.

### Why ILLMProvider abstraction?
Allows per-role provider configuration at runtime. Local models for repetitive tasks,
cloud models for complex reasoning. Swap without touching business logic.

### Why RevisionGate as a separate entity?
Stores original agent output, human edits, and decision — full audit trail.
EditAndAccept triggers a ButterflyService event: modified output propagates downstream,
potentially invalidating SRS items or sprint assignments.

### Why TaskAttempt per cycle?
A Developer sees exactly what the previous attempt produced and why it was rejected.
This context is injected into the prompt for each refinement round.

---

## SignalR Event Types

```csharp
AgentStartedTask    { AgentId, AgentRole, TaskId, TaskTitle }
AgentCompletedTask  { AgentId, AgentRole, TaskId, Output }
TaskStatusChanged   { TaskId, OldStatus, NewStatus }
RevisionGateOpened  { GateId, GateType, AgentOutput }
SprintStarted       { SprintId, SprintNumber, TaskCount }
ProjectPhaseChanged { ProjectId, NewPhase }
```

---

## Dependency Injection Registration Pattern

Each layer exposes an extension method. Registered in API only.

```csharp
// ChaosForge.Domain
services.AddDomainServices();

// ChaosForge.Application
services.AddApplicationServices();

// ChaosForge.Infrastructure
services.AddInfrastructureServices(configuration);
```
