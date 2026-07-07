import type { WorkTaskDto, WorkTaskStatus } from '@/api/types'
import { ScrollArea, ScrollBar } from '@/components/ui/scroll-area'
import { KanbanColumn } from './KanbanColumn'

const COLUMN_ORDER: WorkTaskStatus[] = [
  'Backlog',
  'InProgress',
  'InReview',
  'InTesting',
  'InDocumentation',
  'Done',
]

interface KanbanBoardProps {
  tasks: WorkTaskDto[]
}

export function KanbanBoard({ tasks }: KanbanBoardProps) {
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

  return (
    <ScrollArea className="w-full">
      <div className="flex gap-4 pb-4">
        {COLUMN_ORDER.map((status) => (
          <KanbanColumn key={status} status={status} tasks={grouped[status]} />
        ))}
      </div>
      <ScrollBar orientation="horizontal" />
    </ScrollArea>
  )
}
