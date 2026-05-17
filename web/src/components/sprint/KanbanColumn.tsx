import type { WorkTaskDto, WorkTaskStatus } from '@/api/types'
import { TaskCard } from './TaskCard'

interface KanbanColumnProps {
  status: WorkTaskStatus
  tasks: WorkTaskDto[]
}

const COLUMN_LABEL: Record<WorkTaskStatus, string> = {
  Backlog: 'Backlog',
  InProgress: 'In Progress',
  InReview: 'In Review',
  InTesting: 'In Testing',
  InDocumentation: 'In Documentation',
  Done: 'Done',
}

export function KanbanColumn({ status, tasks }: KanbanColumnProps) {
  return (
    <div className="flex flex-col gap-3 min-w-[240px] w-[240px]">
      <div className="flex items-center gap-2">
        <span className="text-sm font-semibold text-gray-300">{COLUMN_LABEL[status]}</span>
        <span className="rounded-full bg-surface-border px-2 py-0.5 text-xs text-gray-400">
          {tasks.length}
        </span>
      </div>
      <div className="flex flex-col gap-2 min-h-[100px]">
        {tasks.length === 0 ? (
          <p className="text-xs text-gray-600 italic mt-2">No tasks</p>
        ) : (
          tasks.map((task) => <TaskCard key={task.id} task={task} />)
        )}
      </div>
    </div>
  )
}
