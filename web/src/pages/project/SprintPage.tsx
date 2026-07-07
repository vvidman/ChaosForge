import { useParams } from 'react-router-dom'
import { useWorkTasksByProject } from '@/hooks/useWorkTasks'
import { BoardHeader } from '@/components/sprint/BoardHeader'
import { KanbanBoard } from '@/components/sprint/KanbanBoard'
import { SkeletonKanbanColumn } from '@/components/ui/SkeletonKanbanColumn'

export default function SprintPage() {
  const { id } = useParams<{ id: string }>()
  const { data: tasks, isLoading } = useWorkTasksByProject(id!)

  if (isLoading) {
    return <SkeletonKanbanColumn columns={3} cardsPerColumn={3} />
  }

  const allTasks = tasks ?? []

  return (
    <div className="flex flex-col gap-4 h-full">
      <BoardHeader tasks={allTasks} />
      <KanbanBoard tasks={allTasks} />
    </div>
  )
}
