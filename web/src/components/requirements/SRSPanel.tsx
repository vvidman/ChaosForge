import { useState } from 'react'
import { Skeleton } from '@/components/ui/skeleton'
import { Button } from '@/components/ui/button'
import { Badge } from '@/components/ui/badge'
import InlineEditor from './InlineEditor'
import { useWorkTasksBySRS } from '@/hooks/useWorkTasks'
import type { SRSDto, ProjectStatus, WorkTaskStatus } from '@/api/types'

const STATUS_LABELS: Record<WorkTaskStatus, string> = {
  Backlog: 'Backlog',
  InProgress: 'In Progress',
  InReview: 'In Review',
  InTesting: 'In Testing',
  InDocumentation: 'In Documentation',
  Done: 'Done',
}

interface SRSItemProps {
  srs: SRSDto
  canEdit: boolean
  onHumanEdit: (id: string, description: string, note: string) => void
  isEditPending: boolean
}

function SRSItem({ srs, canEdit, onHumanEdit, isEditPending }: SRSItemProps) {
  const [expanded, setExpanded] = useState(false)
  const [editing, setEditing] = useState(false)

  const { data: workTasks = [], isLoading: tasksLoading } = useWorkTasksBySRS(srs.id)

  return (
    <div className="rounded-md border border-surface-border bg-surface-card p-3">
      <p className="text-sm font-medium text-white mb-1">{srs.title}</p>

      <div className={expanded ? '' : 'line-clamp-3'}>
        <p className="text-xs text-gray-400">{srs.technicalDescription}</p>
      </div>

      {srs.technicalDescription.length > 0 && (
        <button
          className="text-xs text-forge-500 mt-1 hover:underline"
          onClick={() => setExpanded((v) => !v)}
        >
          {expanded ? 'Show less' : 'Show more'}
        </button>
      )}

      {srs.humanEditNote && (
        <p className="text-xs text-forge-500 mt-1 italic">Note: {srs.humanEditNote}</p>
      )}

      {canEdit && !editing && (
        <Button
          size="sm"
          variant="ghost"
          className="mt-2 text-xs"
          onClick={() => setEditing(true)}
        >
          Human edit
        </Button>
      )}

      {editing && (
        <InlineEditor
          initialDescription={srs.technicalDescription}
          isPending={isEditPending}
          onSave={(description, note) => {
            onHumanEdit(srs.id, description, note)
            setEditing(false)
          }}
          onCancel={() => setEditing(false)}
        />
      )}

      {tasksLoading && <Skeleton className="h-8 w-full mt-2" />}

      {!tasksLoading && workTasks.length > 0 && (
        <div className="mt-3 border-t border-surface-border pt-2">
          <p className="text-xs text-gray-500 mb-1">Work Tasks</p>
          <div className="flex flex-col gap-1">
            {workTasks.map((task) => (
              <div key={task.id} className="flex items-center justify-between gap-2">
                <span className="text-xs text-gray-300 truncate">{task.title}</span>
                <Badge
                  variant={task.status === 'Done' ? 'default' : 'secondary'}
                  className="text-xs shrink-0"
                >
                  {STATUS_LABELS[task.status]}
                </Badge>
              </div>
            ))}
          </div>
        </div>
      )}
    </div>
  )
}

interface SRSPanelProps {
  projectStatus: ProjectStatus
  srsList: SRSDto[]
  isLoading: boolean
  onHumanEdit: (id: string, description: string, note: string) => void
  isEditPending: boolean
}

export default function SRSPanel({
  projectStatus,
  srsList,
  isLoading,
  onHumanEdit,
  isEditPending,
}: SRSPanelProps) {
  const canEdit =
    projectStatus === 'ArchitecturePhase' ||
    projectStatus === 'SprintPlanning' ||
    projectStatus === 'Development' ||
    projectStatus === 'Completed'

  if (isLoading) {
    return (
      <div className="flex flex-col gap-2">
        {[1, 2].map((i) => (
          <Skeleton key={i} className="h-24 w-full" />
        ))}
      </div>
    )
  }

  if (srsList.length === 0) {
    return <p className="text-sm text-gray-500 p-3">No SRS items yet.</p>
  }

  return (
    <div className="flex flex-col gap-2">
      {srsList.map((srs) => (
        <SRSItem
          key={srs.id}
          srs={srs}
          canEdit={canEdit}
          onHumanEdit={onHumanEdit}
          isEditPending={isEditPending}
        />
      ))}
    </div>
  )
}
