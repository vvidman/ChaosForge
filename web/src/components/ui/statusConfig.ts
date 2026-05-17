import type { ProjectStatus } from '@/api/types'

export const statusConfig: Record<ProjectStatus, { label: string; className: string }> = {
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
}
