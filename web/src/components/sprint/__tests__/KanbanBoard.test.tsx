import { describe, it, expect } from 'vitest'
import type { WorkTaskDto, WorkTaskStatus } from '@/api/types'

const COLUMN_ORDER: WorkTaskStatus[] = [
  'Backlog',
  'InProgress',
  'InReview',
  'InTesting',
  'InDocumentation',
  'Done',
]

function groupTasksByStatus(tasks: WorkTaskDto[]): Record<WorkTaskStatus, WorkTaskDto[]> {
  const grouped = COLUMN_ORDER.reduce<Record<WorkTaskStatus, WorkTaskDto[]>>(
    (acc, status) => {
      acc[status] = []
      return acc
    },
    {} as Record<WorkTaskStatus, WorkTaskDto[]>
  )

  for (const task of tasks) {
    const col = task.status
    grouped[col].push(task)
  }

  return grouped
}

function makeTask(overrides: Partial<WorkTaskDto>): WorkTaskDto {
  return {
    id: 'task-1',
    srsId: 'srs-1',
    sprintId: 'sprint-1',
    title: 'Test Task',
    description: 'A test task',
    status: 'Backlog',
    storyPoints: 3,
    createdAt: '2026-01-01T00:00:00Z',
    ...overrides,
  }
}

describe('KanbanBoard – groupTasksByStatus', () => {
  it('groups tasks by their status', () => {
    const tasks: WorkTaskDto[] = [
      makeTask({ id: '1', status: 'Backlog' }),
      makeTask({ id: '2', status: 'InProgress' }),
      makeTask({ id: '3', status: 'InReview' }),
      makeTask({ id: '4', status: 'Done' }),
    ]
    const grouped = groupTasksByStatus(tasks)

    expect(grouped.Backlog).toHaveLength(1)
    expect(grouped.InProgress).toHaveLength(1)
    expect(grouped.InReview).toHaveLength(1)
    expect(grouped.Done).toHaveLength(1)
    expect(grouped.InTesting).toHaveLength(0)
    expect(grouped.InDocumentation).toHaveLength(0)
  })

  it('groups a task with sprintId null and status Backlog into the Backlog column', () => {
    const tasks: WorkTaskDto[] = [makeTask({ id: '1', sprintId: null, status: 'Backlog' })]
    const grouped = groupTasksByStatus(tasks)

    expect(grouped.Backlog).toHaveLength(1)
    expect(grouped.Backlog[0].id).toBe('1')
  })

  it('groups a task with sprintId set and status InReview into the InReview column', () => {
    const tasks: WorkTaskDto[] = [makeTask({ id: '2', sprintId: 'sprint-42', status: 'InReview' })]
    const grouped = groupTasksByStatus(tasks)

    expect(grouped.InReview).toHaveLength(1)
    expect(grouped.InReview[0].id).toBe('2')
  })

  it('returns empty arrays for all columns when no tasks', () => {
    const grouped = groupTasksByStatus([])
    for (const status of COLUMN_ORDER) {
      expect(grouped[status]).toHaveLength(0)
    }
  })

  it('groups multiple tasks into the same column', () => {
    const tasks: WorkTaskDto[] = [
      makeTask({ id: '1', status: 'InTesting' }),
      makeTask({ id: '2', status: 'InTesting' }),
      makeTask({ id: '3', status: 'InTesting' }),
    ]
    const grouped = groupTasksByStatus(tasks)
    expect(grouped.InTesting).toHaveLength(3)
  })

  it('preserves all six column keys', () => {
    const grouped = groupTasksByStatus([])
    expect(Object.keys(grouped)).toEqual(COLUMN_ORDER)
  })
})
