---
category: specs
title: "Application Pipeline"
branch: "app-pipeline"
status: done
date: "2026-04-03"
related_domain: []
related_adr: []
---

# Feature Spec — Application Pipeline

<!-- Reference this file in the implementation agent with: implement @docs/specs/04-application-pipeline.md -->

---

## Context

Before any use cases (commands/queries) can be implemented, the Application layer needs its
cross-cutting infrastructure: MediatR pipeline behaviors for validation and logging, a base
result type, and the DI registration extension consumed by the API. This is the plumbing
spec — nothing user-visible is built here, but every subsequent Application spec depends on it.

---

## Domain Impact

- New or modified entity: none
- New domain event: none
- New interface: none

---

## Architecture Decisions

- All code lives in `ChaosForge.Application`.
- Pipeline behaviors are registered in order: `LoggingBehavior<,>` → `ValidationBehavior<,>`.
- `ValidationBehavior<,>` uses FluentValidation. If no validators are registered for a
  request, the behavior passes through without error.
- `ValidationBehavior<,>` collects ALL validation errors and throws a single
  `ValidationException` (FluentValidation's own type) — not one exception per field.
- `LoggingBehavior<,>` logs request name, elapsed time, and whether it succeeded or threw.
  It uses `ILogger<T>` from `Microsoft.Extensions.Logging.Abstractions` — no third-party
  logging package.
- Result type: a generic `Result<T>` value type (readonly struct) with `IsSuccess`, `Value`,
  `Error` (string). Also a non-generic `Result` for commands that return no value. These are
  used by command/query handlers — pipeline behaviors do not wrap in `Result`.
- DI registration: a single `AddApplication(this IServiceCollection services)` extension
  method in `Application/DependencyInjection.cs` that registers MediatR, all behaviors,
  and all FluentValidation validators from the Application assembly.
- Do not reference `Microsoft.AspNetCore` anywhere in the Application project.

---

## Implementation Scope — What must be done

- [x] Create `Result` and `Result<T>` in `Application/Common/`:
  ```
  readonly struct Result
    bool IsSuccess
    string? Error
    static Result Success()
    static Result Failure(string error)

  readonly struct Result<T>
    bool IsSuccess
    T? Value
    string? Error
    static Result<T> Success(T value)
    static Result<T> Failure(string error)
  ```
- [x] Create `LoggingBehavior<TRequest, TResponse>` in `Application/Common/Behaviors/`
  - Implements `IPipelineBehavior<TRequest, TResponse>`
  - Logs: request type name on entry, elapsed ms and success/failure on exit
- [x] Create `ValidationBehavior<TRequest, TResponse>` in `Application/Common/Behaviors/`
  - Implements `IPipelineBehavior<TRequest, TResponse>`
  - Injects `IEnumerable<IValidator<TRequest>>`
  - If no validators: pass through
  - If validators: run all, collect failures, throw `ValidationException` if any
- [x] Create `DependencyInjection.cs` in `Application/`:
  - `AddApplication(this IServiceCollection services)` registers:
    - `services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(...).Assembly))`
    - `LoggingBehavior` and `ValidationBehavior` as open generic pipeline behaviors
    - `services.AddValidatorsFromAssembly(...)` for FluentValidation
- [x] Run `dotnet build` — zero warnings, zero errors

---

## Out of Scope — What must NOT be done

- Do not write any command or query handlers in this spec
- Do not add any domain-specific validators yet
- Do not call `AddApplication()` from the API project yet — that is the API wiring spec
- Do not add Serilog or any third-party logging package

---

## Test Expectations

- Unit tests required for:
  - `ValidationBehavior` — passes through when no validators, throws on failure, collects all errors
  - `Result` / `Result<T>` — success and failure factory methods
- Edge cases to cover: multiple validation failures collected into one exception

---

## Open Questions

- None.
