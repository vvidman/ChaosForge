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
