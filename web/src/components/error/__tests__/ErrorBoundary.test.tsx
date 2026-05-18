import { describe, it, expect } from 'vitest'
import { ErrorBoundary } from '../ErrorBoundary'

describe('ErrorBoundary – getDerivedStateFromError', () => {
  it('stores the thrown error in state', () => {
    const err = new Error('boom')
    const state = ErrorBoundary.getDerivedStateFromError(err)
    expect(state.error).toBe(err)
  })

  it('stores error with custom message', () => {
    const err = new Error('network timeout')
    const state = ErrorBoundary.getDerivedStateFromError(err)
    expect(state.error?.message).toBe('network timeout')
  })
})

describe('ErrorBoundary – reset', () => {
  it('calls setState with null error on reset', () => {
    const boundary = new ErrorBoundary({ children: null })
    boundary.state = { error: new Error('stale') }

    let captured: { error: Error | null } | null = null
    boundary.setState = ((s: { error: Error | null }) => { captured = s }) as unknown as typeof boundary.setState

    boundary.reset()
    expect(captured).toEqual({ error: null })
  })
})

describe('ErrorBoundary – nested boundary isolation', () => {
  it('inner boundary error does not propagate to outer state', () => {
    const inner = new ErrorBoundary({ children: null })
    const outer = new ErrorBoundary({ children: null })

    const err = new Error('inner only')
    inner.state = ErrorBoundary.getDerivedStateFromError(err)

    expect(inner.state.error).toBe(err)
    expect(outer.state.error).toBeNull()
  })
})
