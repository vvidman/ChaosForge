import { useState } from 'react'
import {
  DndContext,
  closestCenter,
  PointerSensor,
  useSensor,
  useSensors,
  type DragEndEvent,
} from '@dnd-kit/core'
import {
  SortableContext,
  useSortable,
  verticalListSortingStrategy,
  arrayMove,
} from '@dnd-kit/sortable'
import { CSS } from '@dnd-kit/utilities'
import { GripVertical } from 'lucide-react'
import { Button } from '@/components/ui/button'
import { Skeleton } from '@/components/ui/skeleton'
import UseCaseCard from './UseCaseCard'
import type { UseCaseDto, ProjectStatus } from '@/api/types'

interface SortableUseCaseCardProps {
  useCase: UseCaseDto
  isSelected: boolean
  onClick: () => void
  showDragHandle: boolean
}

function SortableUseCaseCard({
  useCase,
  isSelected,
  onClick,
  showDragHandle,
}: SortableUseCaseCardProps) {
  const { attributes, listeners, setNodeRef, transform, transition } = useSortable({
    id: useCase.id,
  })

  const style = {
    transform: CSS.Transform.toString(transform),
    transition,
  }

  return (
    <div ref={setNodeRef} style={style}>
      <UseCaseCard
        useCase={useCase}
        isSelected={isSelected}
        onClick={onClick}
        dragHandle={
          showDragHandle ? (
            <button
              className="text-gray-500 hover:text-gray-300 cursor-grab active:cursor-grabbing mt-0.5 shrink-0"
              {...attributes}
              {...listeners}
              onClick={(e) => e.stopPropagation()}
            >
              <GripVertical className="w-4 h-4" />
            </button>
          ) : undefined
        }
      />
    </div>
  )
}

interface AddUseCaseFormProps {
  onAdd: (title: string, description: string, priority: number) => void
  isPending: boolean
}

function AddUseCaseForm({ onAdd, isPending }: AddUseCaseFormProps) {
  const [open, setOpen] = useState(false)
  const [title, setTitle] = useState('')
  const [description, setDescription] = useState('')
  const [priority, setPriority] = useState(0)

  function handleSubmit() {
    onAdd(title, description, priority)
    setTitle('')
    setDescription('')
    setPriority(0)
    setOpen(false)
  }

  if (!open) {
    return (
      <Button size="sm" variant="outline" className="w-full mt-2" onClick={() => setOpen(true)}>
        + Add Use Case
      </Button>
    )
  }

  return (
    <div className="flex flex-col gap-2 p-3 mt-2 rounded-md border border-surface-border bg-surface-card">
      <input
        className="w-full rounded-md border border-surface-border bg-surface-hover text-gray-100 px-2 py-1.5 text-sm focus:outline-none focus:ring-1 focus:ring-forge-500"
        placeholder="Title"
        value={title}
        onChange={(e) => setTitle(e.target.value)}
      />
      <textarea
        className="w-full min-h-[80px] rounded-md border border-surface-border bg-surface-hover text-gray-100 p-2 text-sm resize-y focus:outline-none focus:ring-1 focus:ring-forge-500"
        placeholder="Description"
        value={description}
        onChange={(e) => setDescription(e.target.value)}
      />
      <div className="flex items-center gap-2">
        <label className="text-xs text-gray-400 shrink-0">Priority</label>
        <input
          type="number"
          className="w-20 rounded-md border border-surface-border bg-surface-hover text-gray-100 px-2 py-1 text-sm focus:outline-none focus:ring-1 focus:ring-forge-500"
          value={priority}
          onChange={(e) => setPriority(Number(e.target.value))}
        />
      </div>
      <div className="flex gap-2">
        <Button
          size="sm"
          disabled={!title.trim() || isPending}
          onClick={handleSubmit}
        >
          {isPending ? 'Adding…' : 'Add'}
        </Button>
        <Button size="sm" variant="outline" onClick={() => setOpen(false)}>
          Cancel
        </Button>
      </div>
    </div>
  )
}

interface UseCaseListProps {
  projectId: string
  projectStatus: ProjectStatus
  useCases: UseCaseDto[]
  isLoading: boolean
  selectedUseCaseId: string | null
  onSelect: (id: string) => void
  onAdd: (title: string, description: string, priority: number) => void
  onReorder: (id: string, priority: number) => void
  isAddPending: boolean
}

export default function UseCaseList({
  projectId: _projectId,
  projectStatus,
  useCases,
  isLoading,
  selectedUseCaseId,
  onSelect,
  onAdd,
  onReorder,
  isAddPending,
}: UseCaseListProps) {
  const [localOrder, setLocalOrder] = useState<string[]>([])

  const isSetup = projectStatus === 'Setup'

  const sensors = useSensors(
    useSensor(PointerSensor, { activationConstraint: { distance: 5 } })
  )

  const sortedByPriority = [...useCases].sort((a, b) => a.priority - b.priority)

  const displayItems =
    isSetup && localOrder.length === sortedByPriority.length
      ? localOrder.map((id) => sortedByPriority.find((uc) => uc.id === id)!).filter(Boolean)
      : sortedByPriority

  function handleDragEnd(event: DragEndEvent) {
    const { active, over } = event
    if (!over || active.id === over.id) return

    const oldIndex = displayItems.findIndex((uc) => uc.id === active.id)
    const newIndex = displayItems.findIndex((uc) => uc.id === over.id)
    const reordered = arrayMove(displayItems, oldIndex, newIndex)

    setLocalOrder(reordered.map((uc) => uc.id))

    reordered.forEach((uc, idx) => {
      if (displayItems.indexOf(uc) !== idx) {
        onReorder(uc.id, idx)
      }
    })
  }

  if (isLoading) {
    return (
      <div className="flex flex-col gap-2">
        {[1, 2, 3].map((i) => (
          <Skeleton key={i} className="h-16 w-full" />
        ))}
      </div>
    )
  }

  return (
    <div className="flex flex-col">
      <DndContext
        sensors={sensors}
        collisionDetection={closestCenter}
        onDragEnd={handleDragEnd}
      >
        <SortableContext
          items={displayItems.map((uc) => uc.id)}
          strategy={verticalListSortingStrategy}
        >
          <div className="flex flex-col gap-2">
            {displayItems.map((uc) => (
              <SortableUseCaseCard
                key={uc.id}
                useCase={uc}
                isSelected={selectedUseCaseId === uc.id}
                onClick={() => onSelect(uc.id)}
                showDragHandle={isSetup}
              />
            ))}
          </div>
        </SortableContext>
      </DndContext>

      {displayItems.length === 0 && (
        <p className="text-sm text-gray-500 p-3">No use cases yet.</p>
      )}

      {isSetup && (
        <AddUseCaseForm
          onAdd={onAdd}
          isPending={isAddPending}
        />
      )}
    </div>
  )
}
