---
category: conventions
topic: "Git Conventions"
last_updated: "2026-03-29"
related_adr: []
---

# Git Conventions

Branch model: `main` (stable) ← `dev` (integration) ← feature branches.
All AI-driven implementation targets `dev` via PR. The human manages `main`.

---

## Branch Naming

Source of truth: the `branch` field in the spec frontmatter (`docs/specs/<feature>.md`).
Format: `kebab-case`, max 12 characters, no special characters except `-`.

```
feat/prj-setup
fix/gate-nullref
chore/efcore-upg
```

Prefixes: `feat/` new feature, `fix/` bug fix, `chore/` tooling/dependency, `refactor/` no behaviour change.

- Branch is created by the implementation agent from the spec `branch` field — not improvised
- One feature spec = one branch = one PR
- **?** Does the branch name match the spec frontmatter exactly?

---

## Commit Messages

Format: `<type>(<scope>): <short summary>` — imperative mood, max 72 characters.

```
feat(revision-gate): add RejectionReason required validation
fix(task-attempt): set CompletedAt on rejection result
chore(deps): update LlamaSharp to 0.14.0
test(handlers): add ResolveRevisionGateHandler edge cases
```

Types: `feat`, `fix`, `chore`, `test`, `refactor`, `docs`
Scope: domain area or layer (`domain`, `application`, `infra`, `api`, `web`, `deps`)

- One logical change per commit — do not bundle unrelated fixes
- Do not commit commented-out code, debug logs, or TODO left from implementation
- **?** Would this commit message make sense in a `git log` six months from now?

---

## Pull Requests

PR target: always `dev`. Never open a PR directly to `main`.

PR title = last commit message (keep consistent).
PR description must include:
- **Files created** — list with one-line purpose each
- **Files modified** — list with summary of change
- **Tests added** — list with what each covers
- **Manual steps** — migrations, config changes, environment variables needed

The implementation agent produces this description. The human reviews before merge.

---

## Rules

- **Must**: implementation agents never run `git add`, `git commit`, `git push`, `git merge` outside the defined workflow
- **Must**: every PR has at least one passing `dotnet build` before opening
- **Must**: no secrets, API keys, or connection strings in any committed file — use `dotnet user-secrets` or environment variables
- **Must**: merge strategy is squash-merge into `dev` — the human decides when to merge
- **?** Is there anything in this commit that should not be in version control (secrets, local paths, debug output)?

## Anti-patterns

### Bundled commits
One commit containing feature code, test fixes, and a dependency upgrade.
Each commit should be independently revertable.

### Branch improvisation
Creating a branch with a name not matching the spec `branch` field.
The spec is the contract — branch name deviation makes traceability impossible.

## References
- See `toolchain.md` for the exact `dotnet` commands used before committing
- See bootstrap file for the full implementation workflow that these conventions support
