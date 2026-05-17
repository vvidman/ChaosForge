import { Badge } from '@/components/ui/badge'
import { cn } from '@/lib/utils'
import { useTaskAttemptsByTask } from '@/hooks/useTaskAttempts'
import { AttemptCard } from './AttemptCard'
import type { WorkTaskDto, WorkTaskStatus } from '@/api/types'
import { useState } from 'react'

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

interface AttemptTimelineProps {
  taskId: string
  task: WorkTaskDto
}

export function AttemptTimeline({ taskId, task }: AttemptTimelineProps) {
  const { data: attempts = [] } = useTaskAttemptsByTask(taskId)
  const [descExpanded, setDescExpanded] = useState(false)

  const sorted = [...attempts].sort(
    (a, b) => new Date(a.startedAt).getTime() - new Date(b.startedAt).getTime()
  )

  return (
    <div className="flex flex-col h-full space-y-4">
      <div className="rounded-lg border border-surface-border bg-surface-card p-4">
        <div className="flex items-center gap-2 mb-2">
          <h2 className="text-lg font-semibold text-white">{task.title}</h2>
          <Badge className={cn('border-transparent shrink-0', STATUS_STYLES[task.status])}>
            {STATUS_LABELS[task.status]}
          </Badge>
        </div>
        <p
          className={cn(
            'text-sm text-gray-400 cursor-pointer',
            descExpanded ? '' : 'line-clamp-3'
          )}
          onClick={() => setDescExpanded((v) => !v)}
        >
          {task.description}
        </p>
      </div>

      {sorted.length === 0 ? (
        <p className="text-sm text-gray-500 text-center py-8">No attempts recorded yet</p>
      ) : (
        <div className="relative pl-6">
          <div className="absolute left-2.5 top-0 bottom-0 w-px bg-surface-border" />
          <div className="space-y-4">
            {sorted.map((attempt) => (
              <div key={attempt.id} className="relative">
                <div className="absolute -left-[17px] top-4 w-2.5 h-2.5 rounded-full bg-surface-border border-2 border-surface-card" />
                <AttemptCard attempt={attempt} />
              </div>
            ))}
          </div>
        </div>
      )}
    </div>
  )
}
