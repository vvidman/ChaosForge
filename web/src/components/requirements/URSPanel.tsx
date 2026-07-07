import { useState } from 'react'
import { Skeleton } from '@/components/ui/skeleton'
import { Button } from '@/components/ui/button'
import InlineEditor from './InlineEditor'
import WaitingForAgent from './WaitingForAgent'
import { cn } from '@/lib/utils'
import type { URSDto, ProjectStatus } from '@/api/types'

interface URSPanelProps {
  projectStatus: ProjectStatus
  ursList: URSDto[]
  isLoading: boolean
  selectedUrsId: string | null
  onSelect: (id: string) => void
  onHumanEdit: (id: string, description: string, note: string) => void
  isEditPending: boolean
}

export default function URSPanel({
  projectStatus,
  ursList,
  isLoading,
  selectedUrsId,
  onSelect,
  onHumanEdit,
  isEditPending,
}: URSPanelProps) {
  const [editingId, setEditingId] = useState<string | null>(null)

  const canEdit =
    projectStatus === 'RequirementsPhase' ||
    projectStatus === 'ArchitecturePhase' ||
    projectStatus === 'SprintPlanning' ||
    projectStatus === 'Development' ||
    projectStatus === 'Completed'

  const showWaiting = projectStatus === 'RequirementsPhase' && ursList.length === 0

  if (isLoading) {
    return (
      <div className="flex flex-col gap-2">
        {[1, 2].map((i) => (
          <Skeleton key={i} className="h-20 w-full" />
        ))}
      </div>
    )
  }

  return (
    <div className="flex flex-col gap-2">
      {showWaiting && <WaitingForAgent label="Waiting for Business Analyst…" />}

      {ursList.map((urs) => (
        <div
          key={urs.id}
          className={cn(
            'rounded-md border bg-surface-card p-3 cursor-pointer hover:bg-surface-hover transition-colors',
            selectedUrsId === urs.id
              ? 'border-l-4 border-l-forge-500 border-surface-border'
              : 'border-surface-border'
          )}
          onClick={() => onSelect(urs.id)}
        >
          <p className="text-sm font-medium text-white mb-1">{urs.title}</p>
          <p className="text-xs text-gray-400 line-clamp-3">{urs.description}</p>
          {urs.humanEditNote && (
            <p className="text-xs text-forge-500 mt-1 italic">Note: {urs.humanEditNote}</p>
          )}

          {canEdit && editingId !== urs.id && (
            <Button
              size="sm"
              variant="ghost"
              className="mt-2 text-xs"
              onClick={(e) => {
                e.stopPropagation()
                setEditingId(urs.id)
              }}
            >
              Human edit
            </Button>
          )}

          {editingId === urs.id && (
            <div onClick={(e) => e.stopPropagation()}>
              <InlineEditor
                initialDescription={urs.description}
                isPending={isEditPending}
                onSave={(description, note) => {
                  onHumanEdit(urs.id, description, note)
                  setEditingId(null)
                }}
                onCancel={() => setEditingId(null)}
              />
            </div>
          )}
        </div>
      ))}
    </div>
  )
}
