import { useParams } from 'react-router-dom'
import { useProject } from '@/hooks/useProjects'
import { AgentSlotTable } from '@/components/projects/AgentSlotTable'
import { Card } from '@/components/ui/card'

export default function OverviewPage() {
  const { id } = useParams<{ id: string }>()
  const { data: project } = useProject(id!)

  if (!project) return null

  return (
    <div className="flex flex-col gap-6">
      <Card className="bg-surface-card border-surface-border p-4">
        <h2 className="text-sm font-semibold text-gray-300 mb-3">Project Details</h2>
        <dl className="grid grid-cols-2 gap-x-6 gap-y-2 text-sm">
          <dt className="text-gray-500">Name</dt>
          <dd className="text-white">{project.name}</dd>
          <dt className="text-gray-500">Status</dt>
          <dd className="text-white">{project.status}</dd>
          <dt className="text-gray-500">Created</dt>
          <dd className="text-white">{new Date(project.createdAt).toLocaleDateString()}</dd>
          {project.deadline && (
            <>
              <dt className="text-gray-500">Deadline</dt>
              <dd className="text-white">{new Date(project.deadline).toLocaleDateString()}</dd>
            </>
          )}
        </dl>
      </Card>

      <Card className="bg-surface-card border-surface-border p-4">
        <h2 className="text-sm font-semibold text-gray-300 mb-3">Agent Slots</h2>
        <AgentSlotTable projectId={project.id} projectStatus={project.status} />
      </Card>
    </div>
  )
}
