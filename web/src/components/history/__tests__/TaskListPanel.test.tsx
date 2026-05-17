import { describe, it, expect } from 'vitest'
import type { WorkTaskDto, WorkTaskStatus } from '@/api/types'

function makeTask(id: string, title: string, status: WorkTaskStatus): WorkTaskDto {
  return {
    id,
    srsId: 'srs-1',
    sprintId: null,
    title,
    description: '',
    status,
    storyPoints: 3,
    createdAt: new Date().toISOString(),
  }
}

function filterTasks(
  tasks: WorkTaskDto[],
  search: string,
  statusFilter: WorkTaskStatus | null
): WorkTaskDto[] {
  return tasks.filter((t) => {
    const matchesSearch = t.title.toLowerCase().includes(search.toLowerCase())
    const matchesStatus = statusFilter === null || t.status === statusFilter
    return matchesSearch && matchesStatus
  })
}

const TASKS: WorkTaskDto[] = [
  makeTask('1', 'Implement login', 'InProgress'),
  makeTask('2', 'Write tests', 'Done'),
  makeTask('3', 'Fix login bug', 'Backlog'),
  makeTask('4', 'Deploy service', 'InReview'),
]

describe('TaskListPanel – status chip filter', () => {
  it('shows all tasks when filter is null', () => {
    expect(filterTasks(TASKS, '', null)).toHaveLength(4)
  })

  it('shows only InProgress tasks when InProgress chip is active', () => {
    const result = filterTasks(TASKS, '', 'InProgress')
    expect(result).toHaveLength(1)
    expect(result[0].id).toBe('1')
  })

  it('shows only Done tasks when Done chip is active', () => {
    const result = filterTasks(TASKS, '', 'Done')
    expect(result).toHaveLength(1)
    expect(result[0].id).toBe('2')
  })

  it('shows only Backlog tasks when Backlog chip is active', () => {
    const result = filterTasks(TASKS, '', 'Backlog')
    expect(result).toHaveLength(1)
    expect(result[0].id).toBe('3')
  })

  it('returns empty array when no tasks match selected status', () => {
    expect(filterTasks(TASKS, '', 'InTesting')).toHaveLength(0)
  })
})

describe('TaskListPanel – search filter', () => {
  it('filters tasks by search string (case-insensitive)', () => {
    const result = filterTasks(TASKS, 'login', null)
    expect(result).toHaveLength(2)
    expect(result.map((t) => t.id)).toEqual(expect.arrayContaining(['1', '3']))
  })

  it('matches partial strings', () => {
    const result = filterTasks(TASKS, 'dep', null)
    expect(result).toHaveLength(1)
    expect(result[0].id).toBe('4')
  })

  it('is case-insensitive', () => {
    const result = filterTasks(TASKS, 'WRITE', null)
    expect(result).toHaveLength(1)
    expect(result[0].id).toBe('2')
  })
})

describe('TaskListPanel – empty state', () => {
  it('returns empty array when search yields no results', () => {
    expect(filterTasks(TASKS, 'zzznomatch', null)).toHaveLength(0)
  })

  it('returns empty array when status and search combined yield no results', () => {
    expect(filterTasks(TASKS, 'login', 'Done')).toHaveLength(0)
  })
})
