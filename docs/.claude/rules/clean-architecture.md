# Clean Architecture Enforcement Rules

## Layer Dependency Check — YOU MUST follow this

When adding a new class, always verify:
1. Which layer does it belong to?
2. Does it reference anything from a more outer layer?
3. If yes → STOP and refactor using Dependency Inversion

## New Feature Implementation Order

For every new feature, implement in this order:
1. Domain entity or value object (if needed)
2. Domain event (if state changes are relevant)
3. Repository interface in Domain
4. Application Command or Query + Handler
5. FluentValidation validator for the Command
6. Infrastructure: EF configuration + repository implementation
7. API: thin controller action calling mediator
8. SignalR: publish relevant domain event from the handler

Before marking any class done, run the SOLID checklist from @.claude/rules/solid-principles.md.

## Naming Conventions

- Commands: `VerbNounCommand` (e.g., `CreateProjectCommand`, `ResolveRevisionGateCommand`)
- Queries: `GetNounQuery` or `GetNounByXQuery`
- Handlers: `VerbNounCommandHandler`, `GetNounQueryHandler`
- Events: `NounVerbedEvent` past tense (e.g., `TaskAssignedEvent`, `RevisionGateOpenedEvent`)
- Repositories: `I{Entity}Repository`
- DTOs: `{Entity}Dto`, `{Entity}Response`, `{Entity}Request`

## EF Core Rules

- Entity configurations in separate `IEntityTypeConfiguration<T>` classes
- No data annotations on domain entities — Fluent API only
- Migration names must be descriptive: `Add{Entity}Table`, `Add{Column}To{Entity}`

## File Placement Reference

| Type | Project | Namespace |
|---|---|---|
| Entity | Domain | `ChaosForge.Domain.Entities` |
| Domain Event | Domain | `ChaosForge.Domain.Events` |
| Repository Interface | Domain | `ChaosForge.Domain.Interfaces` |
| Command + Handler | Application | `ChaosForge.Application.Commands` |
| Query + Handler | Application | `ChaosForge.Application.Queries` |
| Validator | Application | `ChaosForge.Application.Validators` |
| EF Configuration | Infrastructure | `ChaosForge.Infrastructure.Persistence.Configurations` |
| Repository Impl | Infrastructure | `ChaosForge.Infrastructure.Persistence` |
| LLM Provider | Infrastructure | `ChaosForge.Infrastructure.LLM` |
| Controller | API | `ChaosForge.API.Controllers` |
| SignalR Hub | API | `ChaosForge.API.Hubs` |
