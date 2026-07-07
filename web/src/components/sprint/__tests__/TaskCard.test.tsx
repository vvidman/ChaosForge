import { describe, it, expect } from 'vitest'
import type { WorkTaskStatus } from '@/api/types'

function canRejectTask(status: WorkTaskStatus): boolean {
  return status === 'InReview' || status === 'InTesting'
}

describe('TaskCard – reject menu item visibility', () => {
  it('shows Reject task for InReview', () => {
    expect(canRejectTask('InReview')).toBe(true)
  })

  it('shows Reject task for InTesting', () => {
    expect(canRejectTask('InTesting')).toBe(true)
  })

  it('does not show Reject task for Backlog', () => {
    expect(canRejectTask('Backlog')).toBe(false)
  })

  it('does not show Reject task for InProgress', () => {
    expect(canRejectTask('InProgress')).toBe(false)
  })

  it('does not show Reject task for InDocumentation', () => {
    expect(canRejectTask('InDocumentation')).toBe(false)
  })

  it('does not show Reject task for Done', () => {
    expect(canRejectTask('Done')).toBe(false)
  })
})
