import { describe, it, expect } from 'vitest'
import type { AgentInstanceDto, AgentInstanceStatus, AgentRole, ProjectStatus } from '@/api/types'

const PHASE_ROLES: Partial<Record<ProjectStatus, AgentRole[]>> = {
  RequirementsPhase: ['BusinessAnalyst'],
  ArchitecturePhase: ['Architect'],
  SprintPlanning: ['ScrumMaster'],
  Development: ['Developer', 'Tester', 'Reviewer', 'TechnicalWriter'],
}

const ACTIVE_STATUSES: AgentInstanceStatus[] = ['Idle', 'Working']

function getMissingRoles(
  projectStatus: ProjectStatus,
  agents: Pick<AgentInstanceDto, 'role' | 'status'>[]
): AgentRole[] {
  const expected = PHASE_ROLES[projectStatus] ?? []
  const allFinished = agents.length > 0 && agents.every((a) => a.status === 'Finished')
  if (allFinished) return []
  return expected.filter((role) => !agents.some((a) => a.role === role && ACTIVE_STATUSES.includes(a.status)))
}

describe('PhaseIndicatorBanner – missing role detection', () => {
  it('shows warning when expected role has no active instances', () => {
    const missing = getMissingRoles('RequirementsPhase', [])
    expect(missing).toContain('BusinessAnalyst')
  })

  it('no warning when expected role has an Idle instance', () => {
    const missing = getMissingRoles('RequirementsPhase', [
      { role: 'BusinessAnalyst', status: 'Idle' },
    ])
    expect(missing).toHaveLength(0)
  })

  it('no warning when expected role has a Working instance', () => {
    const missing = getMissingRoles('RequirementsPhase', [
      { role: 'BusinessAnalyst', status: 'Working' },
    ])
    expect(missing).toHaveLength(0)
  })

  it('shows warning when agent exists but is Blocked', () => {
    const missing = getMissingRoles('RequirementsPhase', [
      { role: 'BusinessAnalyst', status: 'Blocked' },
    ])
    expect(missing).toContain('BusinessAnalyst')
  })

  it('no warning when all agents Finished', () => {
    const missing = getMissingRoles('RequirementsPhase', [
      { role: 'BusinessAnalyst', status: 'Finished' },
    ])
    expect(missing).toHaveLength(0)
  })

  it('shows missing roles in Development phase', () => {
    const missing = getMissingRoles('Development', [
      { role: 'Developer', status: 'Working' },
    ])
    expect(missing).toContain('Tester')
    expect(missing).toContain('Reviewer')
    expect(missing).not.toContain('Developer')
  })

  it('no warnings for Setup phase (no expected roles)', () => {
    const missing = getMissingRoles('Setup', [])
    expect(missing).toHaveLength(0)
  })
})
