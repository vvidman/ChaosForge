import { AlertTriangle, CheckCircle } from 'lucide-react'
import type { AgentInstanceDto, AgentInstanceStatus, AgentRole, ProjectDto, ProjectStatus } from '@/api/types'

const PHASE_ROLES: Partial<Record<ProjectStatus, AgentRole[]>> = {
  RequirementsPhase: ['BusinessAnalyst'],
  ArchitecturePhase: ['Architect'],
  SprintPlanning: ['ScrumMaster'],
  Development: ['Developer', 'Tester', 'Reviewer', 'TechnicalWriter'],
}

const PHASE_LABELS: Record<ProjectStatus, string> = {
  Setup: 'Setup',
  RequirementsPhase: 'Requirements Phase',
  ArchitecturePhase: 'Architecture Phase',
  SprintPlanning: 'Sprint Planning',
  Development: 'Development',
  Completed: 'Completed',
}

const ACTIVE_STATUSES: AgentInstanceStatus[] = ['Idle', 'Working']

interface PhaseIndicatorBannerProps {
  project: ProjectDto
  agents: AgentInstanceDto[]
}

export function PhaseIndicatorBanner({ project, agents }: PhaseIndicatorBannerProps) {
  const expectedRoles = PHASE_ROLES[project.status] ?? []
  const allFinished = agents.length > 0 && agents.every((a) => a.status === 'Finished')

  const missingRoles = allFinished
    ? []
    : expectedRoles.filter((role) => !agents.some((a) => a.role === role && ACTIVE_STATUSES.includes(a.status)))

  return (
    <div className="rounded-lg border border-surface-border bg-surface-card px-4 py-3 flex items-center gap-3">
      <div className="flex items-center gap-2">
        <span className="text-sm font-medium text-white">Phase:</span>
        <span className="text-sm text-gray-300">{PHASE_LABELS[project.status]}</span>
      </div>
      {missingRoles.length > 0 && (
        <div className="flex items-center gap-1.5 ml-auto">
          <AlertTriangle size={14} className="text-status-pending" />
          <span className="text-xs text-status-pending">
            Missing active agents: {missingRoles.join(', ')}
          </span>
        </div>
      )}
      {missingRoles.length === 0 && expectedRoles.length > 0 && (
        <div className="flex items-center gap-1.5 ml-auto">
          <CheckCircle size={14} className="text-status-finished" />
          <span className="text-xs text-status-finished">All expected agents active</span>
        </div>
      )}
    </div>
  )
}
