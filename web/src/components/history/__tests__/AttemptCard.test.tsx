import { describe, it, expect } from 'vitest'
import type { TaskAttemptDto, AttemptResult } from '@/api/types'

function makeAttempt(result: AttemptResult, reviewNote?: string, testNote?: string): TaskAttemptDto {
  return {
    id: 'att-1',
    workTaskId: 'task-1',
    agentInstanceId: 'agent-1',
    type: 'Implementation',
    output: 'Some output',
    reviewNote: reviewNote ?? null,
    testNote: testNote ?? null,
    result,
    startedAt: new Date().toISOString(),
    completedAt: null,
    createdAt: new Date().toISOString(),
  }
}

function shouldShowRejectionNote(attempt: TaskAttemptDto): boolean {
  return attempt.result === 'Rejected'
}

function shouldShowPendingSpinner(attempt: TaskAttemptDto): boolean {
  return attempt.result === 'Pending'
}

function getRejectionMessage(attempt: TaskAttemptDto): string | null {
  if (attempt.result !== 'Rejected') return null
  return attempt.reviewNote ?? attempt.testNote ?? null
}

describe('AttemptCard – rejection note visibility', () => {
  it('shows rejection note section when result is Rejected', () => {
    const attempt = makeAttempt('Rejected', 'Not acceptable')
    expect(shouldShowRejectionNote(attempt)).toBe(true)
  })

  it('does not show rejection note when result is Approved', () => {
    expect(shouldShowRejectionNote(makeAttempt('Approved'))).toBe(false)
  })

  it('does not show rejection note when result is Pending', () => {
    expect(shouldShowRejectionNote(makeAttempt('Pending'))).toBe(false)
  })

  it('uses reviewNote for rejection message when present', () => {
    const attempt = makeAttempt('Rejected', 'Review failed', 'Test failed')
    expect(getRejectionMessage(attempt)).toBe('Review failed')
  })

  it('falls back to testNote when reviewNote is null', () => {
    const attempt = makeAttempt('Rejected', undefined, 'Test failed')
    expect(getRejectionMessage(attempt)).toBe('Test failed')
  })
})

describe('AttemptCard – pending spinner', () => {
  it('shows spinner when result is Pending', () => {
    expect(shouldShowPendingSpinner(makeAttempt('Pending'))).toBe(true)
  })

  it('does not show spinner when result is Approved', () => {
    expect(shouldShowPendingSpinner(makeAttempt('Approved'))).toBe(false)
  })

  it('does not show spinner when result is Rejected', () => {
    expect(shouldShowPendingSpinner(makeAttempt('Rejected'))).toBe(false)
  })
})
