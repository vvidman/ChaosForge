import { useState } from 'react'
import { cn } from '@/lib/utils'
import { ScrollArea } from '@/components/ui/scroll-area'
import { TaskListRow } from './TaskListRow'
import type { WorkTaskDto, WorkTaskStatus } from '@/api/types'

const ALL_STATUSES: WorkTaskStatus[] = [
  'Backlog',
  'InProgress',
  'InReview',
  'InTesting',
  'InDocumentation',
  'Done',
]

const STATUS_CHIP_LABELS: Record<WorkTaskStatus, string> = {
  Backlog: 'Backlog',
  InProgress: 'In Progress',
  InReview: 'In Review',
  InTesting: 'In Testing',
  InDocumentation: 'In Docs',
  Done: 'Done',
}

interface TaskListPanelProps {
  tasks: WorkTaskDto[]
  selectedId: string | null
  onSelect: (id: string) => void
}

export function TaskListPanel({ tasks, selectedId, onSelect }: TaskListPanelProps) {
  const [search, setSearch] = useState('')
  const [statusFilter, setStatusFilter] = useState<WorkTaskStatus | null>(null)

  const filtered = tasks.filter((t) => {
    const matchesSearch = t.title.toLowerCase().includes(search.toLowerCase())
    const matchesStatus = statusFilter === null || t.status === statusFilter
    return matchesSearch && matchesStatus
  })

  return (
    <div className="flex flex-col h-full bg-surface-card rounded-lg border border-surface-border overflow-hidden">
      <div className="p-3 border-b border-surface-border space-y-2">
        <input
          type="text"
          value={search}
          onChange={(e) => setSearch(e.target.value)}
          placeholder="Search tasks..."
          className={cn(
            'w-full px-3 py-1.5 text-sm rounded-md',
            'bg-surface-hover border border-surface-border',
            'text-white placeholder-gray-500',
            'focus:outline-none focus:ring-1 focus:ring-forge-500'
          )}
        />
        <div className="flex flex-wrap gap-1">
          <button
            onClick={() => setStatusFilter(null)}
            className={cn(
              'px-2 py-0.5 rounded-full text-xs font-medium transition-colors',
              statusFilter === null
                ? 'bg-forge-500 text-white'
                : 'bg-surface-hover text-gray-400 hover:text-white'
            )}
          >
            All
          </button>
          {ALL_STATUSES.map((s) => (
            <button
              key={s}
              onClick={() => setStatusFilter(statusFilter === s ? null : s)}
              className={cn(
                'px-2 py-0.5 rounded-full text-xs font-medium transition-colors',
                statusFilter === s
                  ? 'bg-forge-500 text-white'
                  : 'bg-surface-hover text-gray-400 hover:text-white'
              )}
            >
              {STATUS_CHIP_LABELS[s]}
            </button>
          ))}
        </div>
      </div>

      <ScrollArea className="flex-1">
        {filtered.length === 0 ? (
          <p className="text-sm text-gray-500 text-center py-8 px-4">No tasks match</p>
        ) : (
          filtered.map((task) => (
            <TaskListRow
              key={task.id}
              task={task}
              isSelected={selectedId === task.id}
              onClick={() => onSelect(task.id)}
            />
          ))
        )}
      </ScrollArea>
    </div>
  )
}
