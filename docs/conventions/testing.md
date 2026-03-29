---
category: conventions
topic: "Testing Conventions"
last_updated: "2026-03-29"
related_adr: []
---

# Testing Conventions

Framework stack: **xUnit** (runner), **FluentAssertions** (assertions), **NSubstitute** (mocking).
Test projects: `ChaosForge.Domain.Tests`, `ChaosForge.Application.Tests`, `ChaosForge.Infrastructure.Tests`.

---

## Test Structure

Each test class maps to one production class. File and class naming:

```
Production:  ChaosForge.Application/Handlers/ResolveRevisionGateHandler.cs
Test:        ChaosForge.Application.Tests/Handlers/ResolveRevisionGateHandlerTests.cs
```

Test method naming — **`Method_Scenario_ExpectedResult`**:
```csharp
public async Task Handle_WhenGateIsAlreadyResolved_ThrowsDomainException()
public async Task Handle_WhenActionIsReject_RequiresRejectionReason()
```

---

## Arrange / Act / Assert

Every test follows AAA with blank line separators. No logic in Assert blocks.

```csharp
// Arrange
var gate = RevisionGateBuilder.Open(RevisionGateType.AfterBA);
var command = new ResolveRevisionGateCommand(gate.Id, RevisionGateAction.Reject, reason: null);

// Act
var act = async () => await _handler.Handle(command, CancellationToken.None);

// Assert
await act.Should().ThrowAsync<DomainException>()
    .WithMessage("*rejection reason*");
```

---

## Mocking with NSubstitute

- Mock only interfaces, never concrete classes
- Use `Substitute.For<IInterface>()` — one substitute per dependency per test
- Set up return values with `.Returns()`, verify calls with `.Received()`
- Do not share substitute instances across tests — create fresh in each test or `[Fact]`

```csharp
var repo = Substitute.For<IWorkTaskRepository>();
repo.GetByIdAsync(taskId, Arg.Any<CancellationToken>()).Returns(task);

await _handler.Handle(command, CancellationToken.None);

await repo.Received(1).SaveAsync(Arg.Is<WorkTask>(t => t.Status == WorkTaskStatus.InProgress), Arg.Any<CancellationToken>());
```

---

## FluentAssertions

- Always use FluentAssertions — never `Assert.Equal`, `Assert.True`, etc.
- For exceptions: `await act.Should().ThrowAsync<T>()` — never `try/catch` in tests
- For collections: `.Should().ContainSingle()`, `.Should().BeEmpty()`, `.Should().HaveCount(n)`
- **?** Is the assertion message clear enough to diagnose failure without reading the test body?

---

## What to test

| Layer | Test focus |
|---|---|
| Domain | Invariants, state machine transitions, domain exceptions |
| Application | Handler logic, command validation, correct repository/service calls |
| Infrastructure | Repository queries (use in-memory SQLite), provider contracts |

- Every domain invariant must have at least one test that verifies the violation throws
- Every command handler must have at least one happy path and one validation failure test
- Infrastructure tests use an in-memory SQLite database — no mocking of EF Core internals

---

## Rules

- No `Thread.Sleep` or `Task.Delay` in tests — use deterministic fakes
- No shared mutable state between tests — each test is fully isolated
- No `[Theory]` with more than 5 inline data cases — extract to a named dataset if more needed
- Test projects must not reference each other
- **?** Would this test fail for a reason other than the behaviour it claims to test?

## Anti-patterns

### Testing implementation details
Asserting that a private method was called, or that a field has a specific value via reflection.
Tests must assert observable behaviour: return values, exceptions, or state visible through the public API.

### Overspecified mocks
Setting up `.Returns()` for calls that the test does not exercise.
Only set up what the tested code path actually calls — extra setup is noise.

## References
- See `csharp.md` for async and naming conventions that apply in test code too
- See `clean-architecture.md` (architecture) for which layer each test project covers
