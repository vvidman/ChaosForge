---
category: specs
title: "API Client Layer and React Query Hooks"
branch: "fe-api-hooks"
status: done
date: "2026-04-21"
related_domain: []
related_adr: []
---

# Feature Spec — API Client Layer and React Query Hooks

<!-- Reference this file in the implementation agent with: implement @docs/specs/33-fe-api-hooks.md -->

---

## Context

The frontend needs typed access to every backend endpoint. Rather than scattering `axios`
calls across components, this spec centralises all API calls into a typed client layer
and exposes them through React Query hooks. Subsequent specs import hooks, not raw API
calls. Depends on: spec 32 (Axios instance, `queryClient`).

---

## Domain Impact

- New or modified entity: none
- New domain event: none
- New interface: none

---

## Architecture Decisions

- **File structure:**
  ```
  src/
    api/
      types.ts          — all DTO types mirroring backend DTOs
      projects.ts       — project API functions
      usecases.ts
      urs.ts
      srs.ts
      worktasks.ts
      revisiongates.ts
      agentslots.ts
      agentinstances.ts
      taskattempts.ts
    hooks/
      useProjects.ts
      useUseCases.ts
      useURS.ts
      useSRS.ts
      useWorkTasks.ts
      useRevisionGates.ts
      useAgentSlots.ts
      useAgentInstances.ts
      useTaskAttempts.ts
  ```
- All types in `types.ts` are `readonly` interfaces matching backend DTOs exactly.
  Enums are string literal unions (not TypeScript enums) to match JSON serialization.
- API functions return `Promise<T>` — they throw `ApiError` on failure.
- React Query hooks: each file exports `useXxx` (query) and `useMutateXxx` (mutation)
  hooks. Mutations call `queryClient.invalidateQueries` on success to keep data fresh.
- Query keys are string arrays defined as constants in each hook file.
- All mutations return the underlying React Query `UseMutationResult` — callers handle
  loading/error state.
- `optimistic updates` are NOT used in this spec — invalidate-on-success is sufficient.

### Key types (subset — implement all backend DTOs)

```typescript
// Enums as string literal unions
type ProjectStatus = 'Setup' | 'RequirementsPhase' | 'ArchitecturePhase' |
                     'SprintPlanning' | 'Development' | 'Completed'
type WorkTaskStatus = 'Backlog' | 'InProgress' | 'InReview' |
                      'InTesting' | 'InDocumentation' | 'Done'
type AgentRole = 'BusinessAnalyst' | 'Architect' | 'ScrumMaster' |
                 'Developer' | 'Tester' | 'Reviewer' | 'TechnicalWriter'
type AgentInstanceStatus = 'Idle' | 'Working' | 'Blocked' | 'Finished'
type RevisionGateType = 'Requirements' | 'Architecture' | 'SprintPlanning'
type RevisionGateStatus = 'Open' | 'Resolved'
type RevisionGateAction = 'Accept' | 'EditAndAccept' | 'Reject'
type AttemptType = 'Implementation' | 'Review' | 'Testing' | 'Documentation'
type AttemptResult = 'Pending' | 'Approved' | 'Rejected'

interface ProjectDto { id: string; name: string; description: string;
  status: ProjectStatus; deadline: string | null; createdAt: string }
interface WorkTaskDto { id: string; srsId: string; sprintId: string | null;
  title: string; description: string; status: WorkTaskStatus; storyPoints: number;
  createdAt: string }
interface AgentInstanceDto { id: string; projectId: string; role: AgentRole;
  personaName: string; status: AgentInstanceStatus;
  currentTaskId: string | null; createdAt: string }
interface RevisionGateDto { id: string; projectId: string; type: RevisionGateType;
  status: RevisionGateStatus; agentOutput: string;
  humanEditedOutput: string | null; rejectionReason: string | null;
  action: RevisionGateAction | null; resolvedAt: string | null; createdAt: string }
interface TaskAttemptDto { id: string; workTaskId: string; agentInstanceId: string;
  type: AttemptType; output: string; reviewNote: string | null;
  testNote: string | null; result: AttemptResult;
  startedAt: string; completedAt: string | null; createdAt: string }
```

---

## Implementation Scope — What must be done

- [x] Create `src/api/types.ts` with all DTO interfaces and string literal union types
  for all enums (derive from backend DTO definitions)

- [x] Create API function files (one per aggregate), each exporting pure async functions:
  - `projects.ts`: `getProjects`, `getProject`, `createProject`,
    `transitionProject`, `updateProjectDescription`
  - `usecases.ts`: `getUseCasesByProject`, `getUseCase`,
    `createUseCase`, `updateUseCasePriority`
  - `urs.ts`: `getURSsByUseCase`, `getURS`, `createURS`, `applyHumanEditToURS`
  - `srs.ts`: `getSRSsByURS`, `getSRS`, `createSRS`, `applyHumanEditToSRS`
  - `worktasks.ts`: `getWorkTasksByProject` (via `/by-srs` chained — or use the
    project-level query once available), `getWorkTask`,
    `createWorkTask`, `assignToSprint`, `startTask`, `sendToReview`,
    `approveTask`, `passTesting`, `completeTask`, `rejectTask`
  - `revisiongates.ts`: `getRevisionGatesByProject`, `getRevisionGate`,
    `getOpenRevisionGate`, `openRevisionGate`, `acceptGate`,
    `editAndAcceptGate`, `rejectGate`
  - `agentslots.ts`: `getAgentSlotsByProject`, `createAgentSlot`,
    `updateAgentSlotCount`
  - `agentinstances.ts`: `getAgentInstancesByProject`, `getAgentInstance`,
    `createAgentInstance`
  - `taskattempts.ts`: `getTaskAttemptsByTask`, `getTaskAttempt`

- [x] Create React Query hook files in `src/hooks/` — one per aggregate:
  - Each exports: `useXxxQuery` (GET), `useXxxMutation` (POST/PATCH) hooks
  - All query hooks accept an optional `enabled` parameter
  - All mutations invalidate relevant queries on success

- [x] Export everything from `src/api/index.ts` and `src/hooks/index.ts`

- [x] `npm run build` — zero TypeScript errors

---

## Out of Scope — What must NOT be done

- Do not implement any UI components here
- Do not add authentication headers — no auth in this project

---

## Test Expectations

- Unit tests required for: none (API functions are thin wrappers; tested via integration)
- Edge cases to cover: n/a

---

## Open Questions

- None.
