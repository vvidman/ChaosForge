import { describe, it, expect } from 'vitest'
import type { ProjectStatus } from '@/api/types'

const PHASES: ProjectStatus[] = [
  'Setup',
  'RequirementsPhase',
  'ArchitecturePhase',
  'SprintPlanning',
  'Development',
  'Completed',
]

const PHASE_ORDER: Record<ProjectStatus, number> = {
  Setup: 0,
  RequirementsPhase: 1,
  ArchitecturePhase: 2,
  SprintPlanning: 3,
  Development: 4,
  Completed: 5,
}

function getPhaseStates(currentStatus: ProjectStatus) {
  const currentIndex = PHASE_ORDER[currentStatus]
  return PHASES.map((phase, index) => ({
    phase,
    isCompleted: index < currentIndex,
    isActive: index === currentIndex,
    isFuture: index > currentIndex,
  }))
}

describe('PhaseStepper phase state logic', () => {
  it('Setup: first phase is active, all others are future', () => {
    const states = getPhaseStates('Setup')
    expect(states[0].isActive).toBe(true)
    expect(states[0].isCompleted).toBe(false)
    expect(states.slice(1).every((s) => s.isFuture)).toBe(true)
  })

  it('RequirementsPhase: first is completed, second is active', () => {
    const states = getPhaseStates('RequirementsPhase')
    expect(states[0].isCompleted).toBe(true)
    expect(states[1].isActive).toBe(true)
    expect(states.slice(2).every((s) => s.isFuture)).toBe(true)
  })

  it('ArchitecturePhase: first two completed, third active', () => {
    const states = getPhaseStates('ArchitecturePhase')
    expect(states[0].isCompleted).toBe(true)
    expect(states[1].isCompleted).toBe(true)
    expect(states[2].isActive).toBe(true)
    expect(states.slice(3).every((s) => s.isFuture)).toBe(true)
  })

  it('SprintPlanning: first three completed, fourth active', () => {
    const states = getPhaseStates('SprintPlanning')
    expect(states.slice(0, 3).every((s) => s.isCompleted)).toBe(true)
    expect(states[3].isActive).toBe(true)
    expect(states.slice(4).every((s) => s.isFuture)).toBe(true)
  })

  it('Development: first four completed, fifth active', () => {
    const states = getPhaseStates('Development')
    expect(states.slice(0, 4).every((s) => s.isCompleted)).toBe(true)
    expect(states[4].isActive).toBe(true)
    expect(states[5].isFuture).toBe(true)
  })

  it('Completed: all phases are completed (last is active, none future)', () => {
    const states = getPhaseStates('Completed')
    expect(states.slice(0, 5).every((s) => s.isCompleted)).toBe(true)
    expect(states[5].isActive).toBe(true)
    expect(states.every((s) => !s.isFuture)).toBe(true)
  })

  it('exactly one phase is active for each status', () => {
    for (const status of PHASES) {
      const states = getPhaseStates(status)
      const activeCount = states.filter((s) => s.isActive).length
      expect(activeCount).toBe(1)
    }
  })

  it('no phase is both active and completed', () => {
    for (const status of PHASES) {
      const states = getPhaseStates(status)
      states.forEach((s) => {
        expect(s.isActive && s.isCompleted).toBe(false)
      })
    }
  })
})
