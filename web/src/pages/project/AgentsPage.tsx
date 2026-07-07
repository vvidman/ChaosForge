import { useParams } from 'react-router-dom'
import { useProject } from '@/hooks/useProjects'
import { useAgentInstancesByProject } from '@/hooks/useAgentInstances'
import { AgentGrid } from '@/components/agents/AgentGrid'
import { EventLog } from '@/components/agents/EventLog'
import { PhaseIndicatorBanner } from '@/components/agents/PhaseIndicatorBanner'
import { SkeletonCard } from '@/components/ui/SkeletonCard'

export default function AgentsPage() {
  const { id } = useParams<{ id: string }>()
  const { data: project, isLoading: projectLoading } = useProject(id!)
  const { data: agents = [], isLoading: agentsLoading } = useAgentInstancesByProject(id!)

  if (projectLoading || agentsLoading) {
    return (
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
        <SkeletonCard />
        <SkeletonCard />
        <SkeletonCard />
      </div>
    )
  }

  if (!project) {
    return <div className="text-gray-400 py-8">Project not found.</div>
  }

  return (
    <div className="flex flex-col gap-6">
      <PhaseIndicatorBanner project={project} agents={agents} />
      {agents.length === 0 ? (
        <p className="text-gray-500 text-sm">No agents configured for this project.</p>
      ) : (
        <AgentGrid agents={agents} projectId={id!} />
      )}
      <EventLog />
    </div>
  )
}
