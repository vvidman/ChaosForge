import { describe, it, expect } from 'vitest'
import type { ProjectStatus } from '@/api/types'

function shouldShowAddForm(projectStatus: ProjectStatus): boolean {
  return projectStatus === 'Setup'
}

describe('UseCaseList – add form visibility', () => {
  it('shows add form when projectStatus is Setup', () => {
    expect(shouldShowAddForm('Setup')).toBe(true)
  })

  it('hides add form when projectStatus is RequirementsPhase', () => {
    expect(shouldShowAddForm('RequirementsPhase')).toBe(false)
  })

  it('hides add form when projectStatus is ArchitecturePhase', () => {
    expect(shouldShowAddForm('ArchitecturePhase')).toBe(false)
  })

  it('hides add form when projectStatus is SprintPlanning', () => {
    expect(shouldShowAddForm('SprintPlanning')).toBe(false)
  })

  it('hides add form when projectStatus is Development', () => {
    expect(shouldShowAddForm('Development')).toBe(false)
  })

  it('hides add form when projectStatus is Completed', () => {
    expect(shouldShowAddForm('Completed')).toBe(false)
  })
})
