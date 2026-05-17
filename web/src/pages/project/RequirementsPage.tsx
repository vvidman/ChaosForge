import { useState } from 'react'
import { useParams } from 'react-router-dom'
import { useProject } from '@/hooks/useProjects'
import {
  useUseCasesByProject,
  useMutateCreateUseCase,
  useMutateUpdateUseCasePriority,
} from '@/hooks/useUseCases'
import { useURSsByUseCase, useMutateApplyHumanEditToURS } from '@/hooks/useURS'
import { useSRSsByURS, useMutateApplyHumanEditToSRS } from '@/hooks/useSRS'
import { ScrollArea } from '@/components/ui/scroll-area'
import UseCaseList from '@/components/requirements/UseCaseList'
import URSPanel from '@/components/requirements/URSPanel'
import SRSPanel from '@/components/requirements/SRSPanel'

export default function RequirementsPage() {
  const { id } = useParams<{ id: string }>()
  const { data: project } = useProject(id!)

  const [selectedUseCaseId, setSelectedUseCaseId] = useState<string | null>(null)
  const [selectedUrsId, setSelectedUrsId] = useState<string | null>(null)

  const { data: useCases = [], isLoading: useCasesLoading } = useUseCasesByProject(id!)

  const { data: ursList = [], isLoading: ursLoading } = useURSsByUseCase(
    selectedUseCaseId ?? '',
    !!selectedUseCaseId
  )

  const { data: srsList = [], isLoading: srsLoading } = useSRSsByURS(
    selectedUrsId ?? '',
    !!selectedUrsId
  )

  const createUseCaseMutation = useMutateCreateUseCase()
  const updatePriorityMutation = useMutateUpdateUseCasePriority()
  const editUrsMutation = useMutateApplyHumanEditToURS()
  const editSrsMutation = useMutateApplyHumanEditToSRS()

  if (!project) return null

  function handleSelectUseCase(ucId: string) {
    setSelectedUseCaseId(ucId)
    setSelectedUrsId(null)
  }

  function handleAddUseCase(title: string, description: string, priority: number) {
    createUseCaseMutation.mutate({ projectId: id!, title, description, priority })
  }

  function handleReorder(ucId: string, priority: number) {
    updatePriorityMutation.mutate({ id: ucId, priority })
  }

  function handleHumanEditURS(ursId: string, editedDescription: string, note: string) {
    editUrsMutation.mutate({ id: ursId, editedDescription, note })
  }

  function handleHumanEditSRS(srsId: string, editedDescription: string, note: string) {
    editSrsMutation.mutate({ id: srsId, editedDescription, note })
  }

  return (
    <div className="flex flex-col lg:flex-row gap-4 h-full">
      <div className="lg:w-1/3 flex flex-col">
        <h3 className="text-sm font-semibold text-gray-300 mb-2">Use Cases</h3>
        <ScrollArea className="flex-1">
          <UseCaseList
            projectId={id!}
            projectStatus={project.status}
            useCases={useCases}
            isLoading={useCasesLoading}
            selectedUseCaseId={selectedUseCaseId}
            onSelect={handleSelectUseCase}
            onAdd={handleAddUseCase}
            onReorder={handleReorder}
            isAddPending={createUseCaseMutation.isPending}
          />
        </ScrollArea>
      </div>

      {selectedUseCaseId && (
        <div className="lg:w-1/3 flex flex-col">
          <h3 className="text-sm font-semibold text-gray-300 mb-2">User Requirements (URS)</h3>
          <ScrollArea className="flex-1">
            <URSPanel
              projectStatus={project.status}
              ursList={ursList}
              isLoading={ursLoading}
              selectedUrsId={selectedUrsId}
              onSelect={setSelectedUrsId}
              onHumanEdit={handleHumanEditURS}
              isEditPending={editUrsMutation.isPending}
            />
          </ScrollArea>
        </div>
      )}

      {selectedUrsId && (
        <div className="lg:w-1/3 flex flex-col">
          <h3 className="text-sm font-semibold text-gray-300 mb-2">
            System Requirements (SRS)
          </h3>
          <ScrollArea className="flex-1">
            <SRSPanel
              projectStatus={project.status}
              srsList={srsList}
              isLoading={srsLoading}
              onHumanEdit={handleHumanEditSRS}
              isEditPending={editSrsMutation.isPending}
            />
          </ScrollArea>
        </div>
      )}
    </div>
  )
}
