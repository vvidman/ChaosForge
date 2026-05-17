import { useParams } from 'react-router-dom'
import { Loader2 } from 'lucide-react'
import { useProject } from '@/hooks/useProjects'
import { useAgentInstancesByProject } from '@/hooks/useAgentInstances'
import { AgentGrid } from '@/components/agents/AgentGrid'
import { EventLog } from '@/components/agents/EventLog'
import { PhaseIndicatorBanner } from '@/components/agents/PhaseIndicatorBanner'

export default function AgentsPage() {
  const { id } = useParams<{ id: string }>()
  const { data: project, isLoading: projectLoading } = useProject(id!)
  const { data: agents = [], isLoading: agentsLoading } = useAgentInstancesByProject(id!)

  if (projectLoading || agentsLoading) {
    return (
      <div className="flex items-center gap-2 text-gray-400 py-8">
        <Loader2 size={18} className="animate-spin" />
        <span>Loading agents…</span>
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
