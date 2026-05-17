import { describe, it, expect } from 'vitest'
import { getRelativeTimeString } from './relativeTime'

describe('getRelativeTimeString', () => {
  function isoSecondsAgo(seconds: number): string {
    return new Date(Date.now() - seconds * 1000).toISOString()
  }

  it('returns "just now" for timestamps less than 1 minute ago', () => {
    expect(getRelativeTimeString(isoSecondsAgo(0))).toBe('just now')
    expect(getRelativeTimeString(isoSecondsAgo(30))).toBe('just now')
    expect(getRelativeTimeString(isoSecondsAgo(59))).toBe('just now')
  })

  it('returns minutes ago for timestamps 1–59 minutes ago', () => {
    expect(getRelativeTimeString(isoSecondsAgo(60))).toBe('1 minute ago')
    expect(getRelativeTimeString(isoSecondsAgo(120))).toBe('2 minutes ago')
    expect(getRelativeTimeString(isoSecondsAgo(59 * 60))).toBe('59 minutes ago')
  })

  it('returns hours ago for timestamps 1–23 hours ago', () => {
    expect(getRelativeTimeString(isoSecondsAgo(3600))).toBe('1 hour ago')
    expect(getRelativeTimeString(isoSecondsAgo(7200))).toBe('2 hours ago')
    expect(getRelativeTimeString(isoSecondsAgo(23 * 3600))).toBe('23 hours ago')
  })

  it('returns days ago for timestamps 1+ days ago', () => {
    expect(getRelativeTimeString(isoSecondsAgo(86400))).toBe('1 day ago')
    expect(getRelativeTimeString(isoSecondsAgo(3 * 86400))).toBe('3 days ago')
  })
})
