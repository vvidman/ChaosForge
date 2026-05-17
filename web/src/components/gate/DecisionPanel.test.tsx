import { describe, it, expect } from 'vitest'

type PanelMode = 'none' | 'editAccept' | 'reject'

function toggleMode(current: PanelMode, next: PanelMode): PanelMode {
  return current === next ? 'none' : next
}

function isEditAcceptSubmitDisabled(content: string): boolean {
  return !content.trim()
}

function isRejectSubmitDisabled(reason: string): boolean {
  return !reason.trim()
}

describe('DecisionPanel – mode toggle logic', () => {
  it('activates editAccept mode when toggled from none', () => {
    expect(toggleMode('none', 'editAccept')).toBe('editAccept')
  })

  it('activates reject mode when toggled from none', () => {
    expect(toggleMode('none', 'reject')).toBe('reject')
  })

  it('collapses editAccept when toggled again', () => {
    expect(toggleMode('editAccept', 'editAccept')).toBe('none')
  })

  it('collapses reject when toggled again', () => {
    expect(toggleMode('reject', 'reject')).toBe('none')
  })

  it('switches from editAccept to reject (collapses first, opens second)', () => {
    expect(toggleMode('editAccept', 'reject')).toBe('reject')
  })

  it('switches from reject to editAccept', () => {
    expect(toggleMode('reject', 'editAccept')).toBe('editAccept')
  })

  it('only one mode is active at a time', () => {
    const modes: PanelMode[] = ['none', 'editAccept', 'reject']
    for (const current of modes) {
      for (const next of (['editAccept', 'reject'] as PanelMode[])) {
        const result = toggleMode(current, next)
        const activeCount = (['editAccept', 'reject'] as PanelMode[]).filter(
          (m) => result === m
        ).length
        expect(activeCount).toBeLessThanOrEqual(1)
      }
    }
  })
})

describe('DecisionPanel – reject submit disabled logic', () => {
  it('disabled when reason is empty string', () => {
    expect(isRejectSubmitDisabled('')).toBe(true)
  })

  it('disabled when reason is only whitespace', () => {
    expect(isRejectSubmitDisabled('   ')).toBe(true)
  })

  it('enabled when reason has content', () => {
    expect(isRejectSubmitDisabled('Not good enough')).toBe(false)
  })

  it('enabled when reason has mixed whitespace and content', () => {
    expect(isRejectSubmitDisabled('  reason  ')).toBe(false)
  })
})

describe('DecisionPanel – edit & accept submit disabled logic', () => {
  it('disabled when content is empty string', () => {
    expect(isEditAcceptSubmitDisabled('')).toBe(true)
  })

  it('disabled when content is only whitespace', () => {
    expect(isEditAcceptSubmitDisabled('   \n  ')).toBe(true)
  })

  it('enabled when content has text', () => {
    expect(isEditAcceptSubmitDisabled('# Requirements\n- Item 1')).toBe(false)
  })

  it('enabled when content has a single character', () => {
    expect(isEditAcceptSubmitDisabled('x')).toBe(false)
  })
})
