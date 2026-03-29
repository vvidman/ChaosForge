---
category: conventions
topic: "C# Coding Conventions"
last_updated: "2026-03-29"
related_adr: []
---

# C# Coding Conventions

Applies to all C# source files in `src/` and `tests/`. These rules are non-negotiable.
When a rule conflicts with an ADR, the ADR takes precedence.

---

## Language Version and Nullability

- **C# 13**, `<Nullable>enable</Nullable>` is on in all projects
- Trust the type system — no redundant null checks when the type is non-nullable
- Use `is null` / `is not null` — never `== null` or `!= null`
- Use `?` only where null is a valid business state, not for laziness
- **?** Does this nullable type represent a real domain concept, or just missing initialization?

---

## Naming

| Element | Convention | Example |
|---|---|---|
| Class, record, enum | PascalCase | `WorkTask`, `AttemptResult` |
| Interface | `I` + PascalCase | `ILLMProvider` |
| Method | PascalCase | `ResolveGateAsync` |
| Property | PascalCase | `RejectionReason` |
| Local variable | camelCase | `taskAttempt` |
| Private field | `_` + camelCase | `_unitOfWork` |
| Constant | PascalCase | `MaxRetryCount` |
| Async method | suffix `Async` | `GetBacklogAsync` |

- Use `nameof` instead of string literals for member names
- Do not abbreviate unless the abbreviation is domain-standard (`BA`, `URS`, `SRS`)

---

## Async

- Async all the way — no `.Result`, `.Wait()`, or `.GetAwaiter().GetResult()`
- Every async method accepts a `CancellationToken` and passes it to all async calls
- Never `async void` except event handlers (avoid event handlers in domain code)
- **?** Is there a synchronous call that wraps an async one anywhere in the call stack?

---

## Types and Records

- Prefer `record` for DTOs and value objects — immutability by default
- Prefer `sealed` for classes that are not designed for inheritance
- Use primary constructors where the body adds no logic
- Avoid `static` classes for business logic — use injected services

---

## Formatting

- No trailing whitespace
- Final `return` on its own line (no inline return in multi-line methods)
- One blank line between methods; no blank line after opening brace
- `var` is acceptable when the type is obvious from the right-hand side
- Braces always, even for single-line `if` / `foreach`

---

## License Header

Every new source file must begin with the license header from `docs/lic-snippet.txt`.
This applies to `.cs` files in both `src/` and `tests/`.

---

## Anti-patterns

### Null-forgiving operator abuse
`foo!.Bar` suppresses the compiler but does not fix the underlying model.
If null is possible, make the type nullable or restructure to eliminate it.

### Blocking async calls
`someTask.Result` in any layer deadlocks under ASP.NET Core's synchronization context.
There are no exceptions to the async-all-the-way rule.

### Static business logic
`static class OrderCalculator` with business methods cannot be injected or mocked.
Use a non-static service registered in DI.

## References
- See `testing.md` for async test conventions
- See `solid.md` (architecture) for class design rules
