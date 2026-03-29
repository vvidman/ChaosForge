---
category: architecture
principle: "SOLID"
last_updated: "2026-03-29"
related_adr: []
---

# SOLID Principles

Five OO design principles applied as mandatory constraints. Every new class must satisfy all
five before it is considered done.

---

## SRP — Single Responsibility
One reason to change. `AgentWorkerService` polls for tasks — it does not format prompts.
`LlamaSharpProvider` runs inference — it does not decide which role uses it.

- **Must**: a class that combines I/O and business logic → split it
- **?** If this class changes, does the change affect more than one workflow concern?

---

## OCP — Open / Closed
Open for extension, closed for modification. New agent roles or providers are added by
implementing an interface — not by editing existing classes.

- **Must**: adding a new `AgentRole` must not require editing existing handlers
- **?** Would adding a new LLM provider require touching anything outside `AddInfrastructureServices`?

---

## LSP — Liskov Substitution
Any `ILLMProvider` implementation must honour the same contract: return a completion or
throw a typed exception — never silently return null or swallow errors.

- **Must**: `ILLMProvider` implementors must not return null on success
- **?** Can I swap `LlamaSharpProvider` for `GroqProvider` in a unit test without changing the test?

---

## ISP — Interface Segregation
`IProjectRepository` and `IWorkTaskRepository` are separate interfaces — a handler that
only reads tasks does not depend on project persistence.

- **Must**: one repository interface per aggregate root
- **?** Does this interface have any method that no current caller uses?

---

## DIP — Dependency Inversion
Application handlers depend on `ILLMProvider`, `IUnitOfWork` — never on concrete
infrastructure types. Concrete types are resolved exclusively in Infrastructure DI registration.

- **Must**: constructor parameters in Application and Domain must be interfaces or value types — never concrete infrastructure types
- **?** Is there a `new DbContext()` or `new HttpClient()` anywhere outside Infrastructure?

---

## Anti-patterns

### Role switch in a handler
`if (role == AgentRole.Developer) { … } else if (role == AgentRole.Tester) { … }` —
violates OCP. Use polymorphism or a strategy/factory.

### God repository
`IRepository<T>` with methods covering multiple aggregates — violates ISP. One interface
per aggregate root.

## References
- See `agent-design.md` — how agent roles apply these principles in practice
- See `llm-strategy.md` — DIP and LSP in the provider abstraction
