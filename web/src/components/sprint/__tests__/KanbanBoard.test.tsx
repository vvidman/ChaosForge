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
    const col = task.sprintId === null ? 'Backlog' : task.status
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

  it('places tasks with null sprintId into Backlog regardless of status', () => {
    const tasks: WorkTaskDto[] = [
      makeTask({ id: '1', sprintId: null, status: 'InProgress' }),
      makeTask({ id: '2', sprintId: null, status: 'Done' }),
    ]
    const grouped = groupTasksByStatus(tasks)

    expect(grouped.Backlog).toHaveLength(2)
    expect(grouped.InProgress).toHaveLength(0)
    expect(grouped.Done).toHaveLength(0)
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
