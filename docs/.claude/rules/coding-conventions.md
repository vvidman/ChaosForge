# Coding Conventions — ChaosForge

These conventions apply to all C# code written in this project.
They extend and complement the rules enforced by `.editorconfig`.

---

## Language and Syntax

- Use file-scoped namespace declarations
- Use single-line using directives
- Use `record` types for DTOs and value objects
- Use primary constructors where appropriate
- Use `sealed` on classes that are not designed for inheritance
- Use pattern matching and switch expressions wherever possible
- Use `nameof` instead of string literals when referring to member names
- Use `is null` or `is not null` instead of `== null` or `!= null`
- Prefer `?.` null-conditional operator where applicable (e.g. `scope?.Dispose()`)
- Use `ObjectDisposedException.ThrowIf` where applicable
- Explicit types preferred over `var` for non-obvious types

## Null Safety

- Nullable reference types are enabled (`<Nullable>enable</Nullable>`) — trust the type system
- Do NOT add null checks when the type system already guarantees a value cannot be null
- Only add null guards at system boundaries (e.g. API input, external LLM responses)

## Async

- Async all the way — no `.Result` or `.Wait()` calls anywhere
- All async methods must accept a `CancellationToken` parameter
- Name async methods with the `Async` suffix

## Formatting

- There must be no trailing whitespace on any line
- The final return statement of a method must be on its own line
- Add a blank line before XML documentation comments (`///`) when they follow
  other code (methods, properties, fields, etc.)

## Documentation

- All public APIs must have XML doc comments (`///`)
- XML doc comments must have a blank line before them when preceded by other code

## Testing

- When adding new unit tests, strongly prefer adding them to existing test files
  rather than creating new files
- When running tests, use filters and verify test run counts or check test logs
  to confirm the tests actually executed
- Do NOT finish work with any tests commented out or disabled that were not
  previously commented out or disabled
- Do NOT emit `// Arrange`, `// Act`, or `// Assert` comments in test methods
- Test method names follow the pattern: `MethodName_Scenario_ExpectedResult`

## Miscellaneous

- Do NOT update `global.json`
- Do NOT use `static` classes for business logic
- All domain state changes go through entity methods, never via direct property assignment
  from outside the entity

---

## Quick Reference Checklist

Before submitting any code, verify:

- [ ] File-scoped namespaces used
- [ ] No `== null` or `!= null` — using `is null` / `is not null`
- [ ] No unnecessary null checks where type system guarantees non-null
- [ ] Final return statement on its own line
- [ ] No trailing whitespace
- [ ] Blank line before `///` doc comments when preceded by code
- [ ] All async methods have `CancellationToken` parameter
- [ ] No `.Result` or `.Wait()` calls
- [ ] No `// Arrange / Act / Assert` comments in tests
- [ ] No tests disabled that were not already disabled
- [ ] `nameof` used instead of string literals for member names
