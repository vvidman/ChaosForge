import { describe, it, expect, beforeEach } from 'vitest'
import { useAppStore } from './appStore'

function getStore() {
  return useAppStore.getState()
}

function resetStore() {
  useAppStore.setState({ status: 'disconnected', notifications: [] })
}

describe('appStore – connection slice', () => {
  beforeEach(resetStore)

  it('setStatus updates status', () => {
    getStore().setStatus('connected')
    expect(getStore().status).toBe('connected')
  })

  it('initial status is disconnected', () => {
    expect(getStore().status).toBe('disconnected')
  })

  it('setStatus cycles through all valid values', () => {
    getStore().setStatus('connecting')
    expect(getStore().status).toBe('connecting')
    getStore().setStatus('connected')
    expect(getStore().status).toBe('connected')
    getStore().setStatus('disconnected')
    expect(getStore().status).toBe('disconnected')
  })
})

describe('appStore – notification slice', () => {
  beforeEach(resetStore)

  it('push adds notification with id', () => {
    getStore().push({ message: 'hello', variant: 'info' })
    const { notifications } = getStore()
    expect(notifications).toHaveLength(1)
    expect(notifications[0].message).toBe('hello')
    expect(notifications[0].variant).toBe('info')
    expect(notifications[0].id).toBeTruthy()
  })

  it('dismiss removes notification by id', () => {
    getStore().push({ message: 'a', variant: 'info' })
    getStore().push({ message: 'b', variant: 'success' })
    const { notifications } = getStore()
    const firstId = notifications[0].id
    getStore().dismiss(firstId)
    const updated = getStore().notifications
    expect(updated).toHaveLength(1)
    expect(updated[0].message).toBe('b')
  })

  it('dismiss with unknown id is no-op', () => {
    getStore().push({ message: 'x', variant: 'error' })
    getStore().dismiss('nonexistent-id')
    expect(getStore().notifications).toHaveLength(1)
  })

  it('push beyond max 5 drops oldest', () => {
    for (let i = 0; i < 6; i++) {
      getStore().push({ message: `msg-${i}`, variant: 'info' })
    }
    const { notifications } = getStore()
    expect(notifications).toHaveLength(5)
    expect(notifications[0].message).toBe('msg-1')
    expect(notifications[4].message).toBe('msg-5')
  })

  it('push exactly 5 keeps all', () => {
    for (let i = 0; i < 5; i++) {
      getStore().push({ message: `msg-${i}`, variant: 'info' })
    }
    expect(getStore().notifications).toHaveLength(5)
  })
})
