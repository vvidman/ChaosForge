# Agent System Rules

## LLM Provider Rules

- `ILLMProvider` is defined in the Domain layer
- Concrete providers (LlamaSharpProvider, GroqProvider) live in Infrastructure only
- Provider selection goes through a factory: `ILLMProviderFactory.GetProvider(AgentRole role)`
- Prompts are never hardcoded in providers — always use `IPromptTemplateRepository`
- All LLM calls are async with CancellationToken

## AgentWorkerService Rules

- Implemented as a BackgroundService in Infrastructure
- Polls `IWorkTaskRepository.GetNextAvailableTaskAsync(AgentRole role, CancellationToken ct)`
- Publishes domain events via `IDomainEventDispatcher` — never calls SignalR directly
- On LLM failure: retry max 3 times with exponential backoff, then mark task as `Blocked`
- Creates a new `TaskAttempt` record before starting any work
- Injects previous attempt output and rejection notes into prompt context automatically

## Prompt Template Structure

Every agent prompt must include these sections in order:
1. Role persona definition
2. Current task description
3. Relevant upstream context (URS for Architect, SRS for Developer, etc.)
4. Previous attempt output — if this is a refinement round
5. Rejection or review notes — if present
6. Expected output format (structured JSON or Markdown)

## Singleton Role Enforcement

BusinessAnalyst and ScrumMaster are singletons — enforced at domain level:
- `Project.AddAgentSlot()` throws `DomainException` if count > 1 for singleton roles
- API returns HTTP 400 with a descriptive error message
- Architect is also singleton — same enforcement applies

## Task Assignment Rules

- A task in `Backlog` status with no active attempt is considered available
- Only one agent may hold a task in `InProgress` at a time
- If a Reviewer rejects a task, it returns to `Backlog` — any eligible Developer may pick it up
- The new Developer attempt always receives the full previous attempt output + review note
- Same rule applies when a Tester rejects: task returns to `Backlog` for any Developer

## Output Handling

- All agent outputs are stored in `TaskAttempt.Output` as raw text (Markdown or JSON)
- The frontend renders Markdown output for human readability
- Structured JSON outputs (e.g., URS lists, SRS lists) are parsed by the Application layer
  and persisted as proper domain entities — not stored as blobs
