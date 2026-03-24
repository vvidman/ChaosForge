# Git Workflow Rules

## IMPORTANT — Git Is Managed by the Human

Claude Code never runs any Git commands.

All version control operations are performed by the human:
- `git add`, `git commit`, `git push`, `git pull`, `git merge`, `git branch` — human only
- Claude Code works exclusively on the local working copy
- When work is complete, Claude Code reports which files were created or modified
- The human reviews the diff and decides what and when to commit

The only exception is if the human explicitly asks Claude Code to run a specific
git command for a specific one-off reason.

---

## Branch Strategy (for human reference)

- `main` — stable, always buildable
- `dev` — integration branch
- `feature/<short-description>` — feature work branched from dev
- `fix/<short-description>` — bug fixes

## Commit Message Format (Conventional Commits)

```
feat(domain): add RevisionGate entity with butterfly effect logic
fix(infra): handle LlamaSharp timeout on long completions
chore(api): add missing CancellationToken to ProjectController
refactor(app): extract prompt building into PromptTemplateService
test(domain): add WorkTask state transition unit tests
docs: update architecture decisions for LLM provider strategy
```

## What Must Never Be Committed

- API keys or secrets of any kind
- `appsettings.Development.json` or `appsettings.Local.json` with real values
- SQLite database files (`*.db`, `*.db-shm`, `*.db-wal`)
- `node_modules/`
- `.claude/settings.local.json`
- `CLAUDE.local.md`
- Auto memory files under `.claude/memory/`

## What Must Always Be Committed

- `CLAUDE.md`
- `.claude/rules/*.md`
- `.claude/commands/*.md`
- `docs/architecture.md`, `docs/decisions/*.md`, `docs/specs/*.md`
- `.claude/settings.json` (shared hooks and permissions, no personal data)
