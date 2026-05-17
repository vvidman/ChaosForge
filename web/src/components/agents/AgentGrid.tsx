import { AgentCard } from './AgentCard'
import type { AgentInstanceDto, AgentRole } from '@/api/types'

const SINGLETON_ROLES: AgentRole[] = ['BusinessAnalyst', 'Architect', 'ScrumMaster']
const DEVELOPMENT_ROLES: AgentRole[] = ['Developer', 'Tester', 'Reviewer', 'TechnicalWriter']

interface AgentGridProps {
  agents: AgentInstanceDto[]
  projectId: string
}

function RoleGroup({ title, roles, agents, projectId }: { title: string; roles: AgentRole[]; agents: AgentInstanceDto[]; projectId: string }) {
  const grouped = roles.flatMap((role) => agents.filter((a) => a.role === role))
  if (grouped.length === 0) return null

  return (
    <div className="flex flex-col gap-3">
      <h3 className="text-sm font-semibold text-gray-400 uppercase tracking-wider">{title}</h3>
      <div className="grid grid-cols-1 md:grid-cols-2 gap-3">
        {grouped.map((agent) => (
          <AgentCard key={agent.id} agent={agent} projectId={projectId} />
        ))}
      </div>
    </div>
  )
}

export function AgentGrid({ agents, projectId }: AgentGridProps) {
  return (
    <div className="flex flex-col gap-6">
      <RoleGroup title="Singleton Agents" roles={SINGLETON_ROLES} agents={agents} projectId={projectId} />
      <RoleGroup title="Development Agents" roles={DEVELOPMENT_ROLES} agents={agents} projectId={projectId} />
    </div>
  )
}
