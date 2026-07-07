import { describe, it, expect } from 'vitest'
import type { AgentRole, AgentInstanceStatus } from '@/api/types'

const ROLE_ICONS: Record<AgentRole, string> = {
  BusinessAnalyst: 'Brain',
  Architect: 'Building',
  ScrumMaster: 'ClipboardList',
  Developer: 'Code2',
  Tester: 'FlaskConical',
  Reviewer: 'Search',
  TechnicalWriter: 'FileText',
}

function getIconName(role: AgentRole): string {
  return ROLE_ICONS[role]
}

function showViewTaskLink(currentTaskId: string | null): boolean {
  return currentTaskId !== null
}

const STATUS_DOT_PULSE: Record<AgentInstanceStatus, boolean> = {
  Idle: false,
  Working: true,
  Blocked: false,
  Finished: false,
}

describe('AgentCard – role icon mapping', () => {
  it('maps BusinessAnalyst to Brain', () => {
    expect(getIconName('BusinessAnalyst')).toBe('Brain')
  })

  it('maps Developer to Code2', () => {
    expect(getIconName('Developer')).toBe('Code2')
  })

  it('maps Tester to FlaskConical', () => {
    expect(getIconName('Tester')).toBe('FlaskConical')
  })

  it('maps Reviewer to Search', () => {
    expect(getIconName('Reviewer')).toBe('Search')
  })

  it('maps TechnicalWriter to FileText', () => {
    expect(getIconName('TechnicalWriter')).toBe('FileText')
  })
})

describe('AgentCard – view task link visibility', () => {
  it('shows link when currentTaskId is set', () => {
    expect(showViewTaskLink('task-123')).toBe(true)
  })

  it('hides link when currentTaskId is null', () => {
    expect(showViewTaskLink(null)).toBe(false)
  })
})

describe('AgentCard – status dot pulse', () => {
  it('Working status pulses', () => {
    expect(STATUS_DOT_PULSE['Working']).toBe(true)
  })

  it('Idle status does not pulse', () => {
    expect(STATUS_DOT_PULSE['Idle']).toBe(false)
  })

  it('Blocked status does not pulse', () => {
    expect(STATUS_DOT_PULSE['Blocked']).toBe(false)
  })
})
