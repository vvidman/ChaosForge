import { useParams } from 'react-router-dom'
import { useWorkTasksByProject } from '@/hooks/useWorkTasks'
import { BoardHeader } from '@/components/sprint/BoardHeader'
import { KanbanBoard } from '@/components/sprint/KanbanBoard'

export default function SprintPage() {
  const { id } = useParams<{ id: string }>()
  const { data: tasks, isLoading } = useWorkTasksByProject(id!)

  if (isLoading) {
    return <p className="text-sm text-gray-500">Loading sprint board…</p>
  }

  const allTasks = tasks ?? []

  return (
    <div className="flex flex-col gap-4 h-full">
      <BoardHeader tasks={allTasks} />
      <KanbanBoard tasks={allTasks} />
    </div>
  )
}
