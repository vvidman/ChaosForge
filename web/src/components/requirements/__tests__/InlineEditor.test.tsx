import { describe, it, expect } from 'vitest'

function isSaveDisabled(description: string): boolean {
  return !description.trim()
}

describe('InlineEditor – save button disabled logic', () => {
  it('disabled when description is empty string', () => {
    expect(isSaveDisabled('')).toBe(true)
  })

  it('disabled when description is only whitespace', () => {
    expect(isSaveDisabled('   ')).toBe(true)
  })

  it('enabled when description has content', () => {
    expect(isSaveDisabled('Some description')).toBe(false)
  })

  it('enabled when description has a single character', () => {
    expect(isSaveDisabled('x')).toBe(false)
  })

  it('save works without a note (note is optional)', () => {
    const description = 'Valid description'
    const note = ''
    expect(isSaveDisabled(description)).toBe(false)
    expect(note).toBe('')
  })
})
