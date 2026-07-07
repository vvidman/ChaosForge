import { Badge } from '@/components/ui/badge'
import { cn } from '@/lib/utils'
import type { WorkTaskDto, WorkTaskStatus } from '@/api/types'

const STATUS_STYLES: Record<WorkTaskStatus, string> = {
  Backlog: 'bg-gray-500/20 text-gray-400',
  InProgress: 'bg-forge-500/20 text-forge-500',
  InReview: 'bg-purple-500/20 text-purple-400',
  InTesting: 'bg-amber-500/20 text-amber-400',
  InDocumentation: 'bg-teal-500/20 text-teal-400',
  Done: 'bg-status-done/20 text-status-done',
}

const STATUS_LABELS: Record<WorkTaskStatus, string> = {
  Backlog: 'Backlog',
  InProgress: 'In Progress',
  InReview: 'In Review',
  InTesting: 'In Testing',
  InDocumentation: 'In Documentation',
  Done: 'Done',
}

interface TaskListRowProps {
  task: WorkTaskDto
  isSelected: boolean
  onClick: () => void
}

export function TaskListRow({ task, isSelected, onClick }: TaskListRowProps) {
  return (
    <button
      onClick={onClick}
      className={cn(
        'w-full text-left px-4 py-3 border-b border-surface-border transition-colors',
        'hover:bg-surface-hover focus:outline-none focus:bg-surface-hover',
        isSelected
          ? 'bg-surface-hover border-l-2 border-l-forge-500'
          : 'border-l-2 border-l-transparent'
      )}
    >
      <div className="flex items-start justify-between gap-2">
        <span className="text-sm text-white font-medium line-clamp-2 flex-1">{task.title}</span>
        <span className="text-xs text-gray-500 shrink-0 mt-0.5">{task.storyPoints}pt</span>
      </div>
      <div className="mt-1.5">
        <Badge className={cn('border-transparent text-xs', STATUS_STYLES[task.status])}>
          {STATUS_LABELS[task.status]}
        </Badge>
      </div>
    </button>
  )
}
