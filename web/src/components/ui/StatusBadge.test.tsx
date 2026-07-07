import { describe, it, expect } from 'vitest'
import { statusConfig } from './statusConfig'
import type { ProjectStatus } from '@/api/types'

describe('StatusBadge – statusConfig', () => {
  const allStatuses: ProjectStatus[] = [
    'Setup',
    'RequirementsPhase',
    'ArchitecturePhase',
    'SprintPlanning',
    'Development',
    'Completed',
  ]

  it('has a config entry for every ProjectStatus', () => {
    for (const status of allStatuses) {
      expect(statusConfig[status]).toBeDefined()
    }
  })

  it('Setup uses surface-border color class', () => {
    expect(statusConfig.Setup.className).toContain('surface-border')
  })

  it('RequirementsPhase uses pending color class', () => {
    expect(statusConfig.RequirementsPhase.className).toContain('status-pending')
  })

  it('ArchitecturePhase uses pending color class', () => {
    expect(statusConfig.ArchitecturePhase.className).toContain('status-pending')
  })

  it('SprintPlanning uses working color class', () => {
    expect(statusConfig.SprintPlanning.className).toContain('status-working')
  })

  it('Development uses working color class', () => {
    expect(statusConfig.Development.className).toContain('status-working')
  })

  it('Completed uses done color class', () => {
    expect(statusConfig.Completed.className).toContain('status-done')
  })

  it('each status has a non-empty label', () => {
    for (const status of allStatuses) {
      expect(statusConfig[status].label.length).toBeGreaterThan(0)
    }
  })
})

describe('StatusBadge – WorkTaskStatus entries', () => {
  it('Working uses working color class', () => {
    expect(statusConfig.Working.className).toContain('status-working')
    expect(statusConfig.Working.label).toBe('Working')
  })

  it('Blocked uses blocked color class', () => {
    expect(statusConfig.Blocked.className).toContain('status-blocked')
    expect(statusConfig.Blocked.label).toBe('Blocked')
  })

  it('Done uses done color class', () => {
    expect(statusConfig.Done.className).toContain('status-done')
    expect(statusConfig.Done.label).toBe('Done')
  })

  it('Pending uses pending color class', () => {
    expect(statusConfig.Pending.className).toContain('status-pending')
    expect(statusConfig.Pending.label).toBe('Pending')
  })
})

describe('StatusBadge – AgentInstanceStatus entries', () => {
  it('Idle uses idle color class', () => {
    expect(statusConfig.Idle.className).toContain('status-idle')
    expect(statusConfig.Idle.label).toBe('Idle')
  })

  it('Finished uses done color class', () => {
    expect(statusConfig.Finished.className).toContain('status-done')
    expect(statusConfig.Finished.label).toBe('Finished')
  })
})

describe('StatusBadge – fallback for unknown status', () => {
  it('returns undefined for unknown key (caller applies fallback)', () => {
    const config = statusConfig['SomeUnknownStatus']
    expect(config).toBeUndefined()
  })
})
