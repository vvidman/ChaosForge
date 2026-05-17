import type { WorkTaskDto } from '@/api/types'

interface BoardHeaderProps {
  tasks: WorkTaskDto[]
}

export function BoardHeader({ tasks }: BoardHeaderProps) {
  const sprintTask = tasks.find((t) => t.sprintId !== null)
  const sprintDisplay = sprintTask ? sprintTask.sprintId!.slice(0, 8) : '—'

  const totalPoints = tasks.reduce((sum, t) => sum + t.storyPoints, 0)
  const completedPoints = tasks
    .filter((t) => t.status === 'Done')
    .reduce((sum, t) => sum + t.storyPoints, 0)

  return (
    <div className="flex items-center gap-6 px-1 pb-4">
      <div>
        <span className="text-xs text-gray-500 uppercase tracking-wide">Sprint</span>
        <p className="text-sm font-mono text-white">{sprintDisplay}</p>
      </div>
      <div className="h-8 w-px bg-surface-border" />
      <div>
        <span className="text-xs text-gray-500 uppercase tracking-wide">Total Points</span>
        <p className="text-sm font-semibold text-white">{totalPoints}</p>
      </div>
      <div>
        <span className="text-xs text-gray-500 uppercase tracking-wide">Completed</span>
        <p className="text-sm font-semibold text-status-done">{completedPoints}</p>
      </div>
    </div>
  )
}
