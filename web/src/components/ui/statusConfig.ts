export const statusConfig: Record<string, { label: string; className: string }> = {
  // ProjectStatus
  Setup: {
    label: 'Setup',
    className: 'border-surface-border text-gray-400 bg-transparent',
  },
  RequirementsPhase: {
    label: 'Requirements',
    className: 'border-transparent bg-status-pending/20 text-status-pending',
  },
  ArchitecturePhase: {
    label: 'Architecture',
    className: 'border-transparent bg-status-pending/20 text-status-pending',
  },
  SprintPlanning: {
    label: 'Sprint Planning',
    className: 'border-transparent bg-status-working/20 text-status-working',
  },
  Development: {
    label: 'Development',
    className: 'border-transparent bg-status-working/20 text-status-working',
  },
  Completed: {
    label: 'Completed',
    className: 'border-transparent bg-status-done/20 text-status-done',
  },

  // WorkTaskStatus
  Backlog: {
    label: 'Backlog',
    className: 'border-surface-border text-gray-400 bg-transparent',
  },
  InProgress: {
    label: 'In Progress',
    className: 'border-transparent bg-status-working/20 text-status-working',
  },
  InReview: {
    label: 'In Review',
    className: 'border-surface-border text-gray-300 bg-transparent',
  },
  InTesting: {
    label: 'In Testing',
    className: 'border-surface-border text-gray-300 bg-transparent',
  },
  InDocumentation: {
    label: 'In Documentation',
    className: 'border-surface-border text-gray-300 bg-transparent',
  },
  Done: {
    label: 'Done',
    className: 'border-transparent bg-status-done/20 text-status-done',
  },

  // AgentInstanceStatus
  Idle: {
    label: 'Idle',
    className: 'border-surface-border text-status-idle bg-transparent',
  },
  Working: {
    label: 'Working',
    className: 'border-transparent bg-status-working/20 text-status-working',
  },
  Blocked: {
    label: 'Blocked',
    className: 'border-transparent bg-status-blocked/20 text-status-blocked',
  },
  Finished: {
    label: 'Finished',
    className: 'border-transparent bg-status-done/20 text-status-done',
  },

  // AttemptResult
  Pending: {
    label: 'Pending',
    className: 'border-transparent bg-status-pending/20 text-status-pending',
  },
  Approved: {
    label: 'Approved',
    className: 'border-transparent bg-status-done/20 text-status-done',
  },
  Rejected: {
    label: 'Rejected',
    className: 'border-transparent bg-status-blocked/20 text-status-blocked',
  },
}
