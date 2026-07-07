import { useState } from 'react'
import { useParams, useSearchParams } from 'react-router-dom'
import { ScrollArea } from '@/components/ui/scroll-area'
import { useWorkTasksByProject } from '@/hooks/useWorkTasks'
import { TaskListPanel } from '@/components/history/TaskListPanel'
import { AttemptTimeline } from '@/components/history/AttemptTimeline'

export default function HistoryPage() {
  const { id } = useParams<{ id: string }>()
  const [searchParams] = useSearchParams()
  const [selectedTaskId, setSelectedTaskId] = useState<string | null>(
    searchParams.get('task')
  )

  const { data: tasks = [] } = useWorkTasksByProject(id!)

  const selectedTask = tasks.find((t) => t.id === selectedTaskId) ?? null

  return (
    <div className="flex flex-col lg:flex-row gap-4 h-full min-h-0">
      <div className="lg:w-1/3 min-h-0 flex flex-col">
        <TaskListPanel
          tasks={tasks}
          selectedId={selectedTaskId}
          onSelect={setSelectedTaskId}
        />
      </div>
      <ScrollArea className="lg:w-2/3 flex-1">
        {selectedTaskId && selectedTask ? (
          <AttemptTimeline taskId={selectedTaskId} task={selectedTask} />
        ) : (
          <div className="flex items-center justify-center h-full min-h-[200px]">
            <p className="text-gray-400">Select a task to view its attempt history</p>
          </div>
        )}
      </ScrollArea>
    </div>
  )
}
