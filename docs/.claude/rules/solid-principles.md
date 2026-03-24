# SOLID Principles — ChaosForge

All code written in this project must follow SOLID principles.
Every principle below includes ChaosForge-specific examples.

---

## S — Single Responsibility Principle

**A class should have only one reason to change.**

✅ Correct:
```csharp
// AgentWorkerService: task polling and dispatch ONLY
// PromptBuilder: prompt assembly ONLY
// RevisionGateService: gate logic ONLY
public sealed class PromptBuilder(IPromptTemplateRepository templates)
{
    public string Build(AgentRole role, WorkTask task, TaskAttempt? previous) { ... }
}
```

❌ Wrong:
```csharp
// AgentWorkerService must NOT handle prompt building, LLM calls AND persistence at once
public class AgentWorkerService
{
    public async Task DoEverything(WorkTask task) // SRP violation
    {
        var prompt = $"You are a {task.Role}..."; // prompt building here? no.
        var result = await _llm.Complete(prompt);  // LLM call here? no.
        await _db.SaveChangesAsync();              // persistence here? no.
    }
}
```

**ChaosForge SRP boundaries:**
- `AgentWorkerService` → task lifecycle coordination only
- `PromptBuilder` → prompt assembly from role + context only
- `ILLMProvider` implementations → LLM communication only
- `RevisionGateService` → gate open/close + butterfly effect trigger only
- Repositories → data access only, no business logic

---

## O — Open/Closed Principle

**Open for extension, closed for modification.**

✅ Correct — adding a new LLM provider without modifying existing code:
```csharp
// Domain interface — never changes
public interface ILLMProvider
{
    Task<string> CompleteAsync(string prompt, CancellationToken ct);
}

// New provider: just add it, nothing else changes
public sealed class MistralProvider(HttpClient http) : ILLMProvider
{
    public async Task<string> CompleteAsync(string prompt, CancellationToken ct) { ... }
}
```

✅ Correct — adding a new AgentRole using strategy pattern:
```csharp
// New role = new IPromptTemplate implementation + registration
// Existing code is not touched
public sealed class PromptTemplateRepository : IPromptTemplateRepository
{
    private readonly Dictionary<AgentRole, IPromptTemplate> _templates;
}
```

❌ Wrong:
```csharp
// switch/if-else per role = OCP violation, every new role requires modification
public string BuildPrompt(AgentRole role, WorkTask task) =>
    role switch
    {
        AgentRole.Developer => $"You are a developer...",
        AgentRole.Tester    => $"You are a tester...",
        // New role → must touch this code = OCP violation ❌
    };
```

---

## L — Liskov Substitution Principle

**Subtypes must be substitutable for their base types.**

✅ Correct:
```csharp
// Any ILLMProvider implementation is interchangeable
// The caller does not know and does not care which one it gets
public sealed class AgentWorkerService(ILLMProvider llm, ...) { }
// Works with LlamaSharpProvider, GroqProvider, and test mocks equally
```

**ChaosForge LSP rules:**
- All `ILLMProvider` implementations must use `LLMException` only — no provider-specific exceptions leaking out
- `InMemoryRepository` (for tests) and `EfCoreRepository` must behave identically from the caller's perspective
- Repository null handling: return `null` for not-found, never throw `NotFoundException` from the repository itself

---

## I — Interface Segregation Principle

**No client should be forced to depend on interfaces it does not use.**

✅ Correct — split repository interfaces:
```csharp
public interface IWorkTaskReader
{
    Task<WorkTask?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<IReadOnlyList<WorkTask>> GetAvailableForRoleAsync(AgentRole role, CancellationToken ct);
}

public interface IWorkTaskWriter
{
    Task AddAsync(WorkTask task, CancellationToken ct);
    Task UpdateAsync(WorkTask task, CancellationToken ct);
}
```

✅ Correct — split LLM interfaces:
```csharp
public interface ILLMProvider           // standard completion
public interface IStreamingLLMProvider  // streaming, only where needed
public interface IEmbeddingProvider     // embeddings, entirely separate concern
```

❌ Wrong:
```csharp
public interface IAgentRepository  // ISP violation — too many responsibilities
{
    Task<AgentInstance> GetByIdAsync(Guid id);
    Task UpdateStatusAsync(Guid id, AgentInstanceStatus status);
    Task<IReadOnlyList<WorkTask>> GetTasksForAgentAsync(Guid id); // belongs to IWorkTaskReader
    Task<string> BuildPromptAsync(Guid agentId, Guid taskId);     // belongs to PromptBuilder
}
```

---

## D — Dependency Inversion Principle

**High-level modules must not depend on low-level modules. Both must depend on abstractions.**

✅ Correct — full DIP across Clean Architecture layers:
```csharp
// Domain (innermost) defines the interface
namespace ChaosForge.Domain.Interfaces;
public interface ILLMProvider { ... }
public interface IProjectRepository { ... }

// Application depends only on Domain abstractions
namespace ChaosForge.Application.Commands;
public sealed class AssignTaskCommandHandler(
    IProjectRepository projects,   // abstraction, not EF class
    ILLMProvider llm,              // abstraction, not LlamaSharp
    IUnitOfWork uow) : IRequestHandler<AssignTaskCommand> { ... }

// Infrastructure implements the interfaces
namespace ChaosForge.Infrastructure.Persistence;
public sealed class EfCoreProjectRepository(ChaosForgeDbContext ctx) : IProjectRepository { ... }
```

❌ Wrong:
```csharp
// Application layer directly referencing LlamaSharp = DIP violation
using LLama; // Infrastructure dependency inside Application ❌
public sealed class AssignTaskCommandHandler
{
    private readonly LLamaWeights _model; // concrete implementation ❌
}
```

**DI registration location:** always in `ChaosForge.API` or dedicated extension methods:
`AddDomainServices()`, `AddApplicationServices()`, `AddInfrastructureServices()`

---

## SOLID + Clean Architecture Checklist

Run this before marking any class complete:

- [ ] **S**: Does this class have more than one reason to change? → split it
- [ ] **O**: Adding new behavior requires modifying this class? → introduce strategy/factory
- [ ] **L**: Are all implementations of this interface fully substitutable? → align contracts
- [ ] **I**: Does the client use every method on the interface? → split if not
- [ ] **D**: Does any dependency point to a concrete class? → introduce an interface in Domain
