import { Link } from 'react-router-dom'
import { Brain, Building, ClipboardList, Code2, FlaskConical, Search, FileText } from 'lucide-react'
import { cn } from '@/lib/utils'
import { useWorkTask } from '@/hooks/useWorkTasks'
import type { AgentInstanceDto, AgentInstanceStatus, AgentRole } from '@/api/types'

const ROLE_ICONS: Record<AgentRole, React.ComponentType<{ size?: number; className?: string }>> = {
  BusinessAnalyst: Brain,
  Architect: Building,
  ScrumMaster: ClipboardList,
  Developer: Code2,
  Tester: FlaskConical,
  Reviewer: Search,
  TechnicalWriter: FileText,
}

const ROLE_LABELS: Record<AgentRole, string> = {
  BusinessAnalyst: 'Business Analyst',
  Architect: 'Architect',
  ScrumMaster: 'Scrum Master',
  Developer: 'Developer',
  Tester: 'Tester',
  Reviewer: 'Reviewer',
  TechnicalWriter: 'Technical Writer',
}

const STATUS_DOT: Record<AgentInstanceStatus, string> = {
  Idle: 'bg-status-idle',
  Working: 'bg-status-working animate-pulse',
  Blocked: 'bg-status-blocked',
  Finished: 'bg-status-finished',
}

interface AgentCardProps {
  agent: AgentInstanceDto
  projectId: string
}

function TaskTitle({ taskId }: { taskId: string }) {
  const { data } = useWorkTask(taskId, true)
  return <span>{data?.title ?? '…'}</span>
}

export function AgentCard({ agent, projectId }: AgentCardProps) {
  const Icon = ROLE_ICONS[agent.role]

  return (
    <div className="rounded-lg border border-surface-border bg-surface-card p-4 flex flex-col gap-3">
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-2">
          <Icon size={18} className="text-gray-400" />
          <span className="font-medium text-white">{ROLE_LABELS[agent.role]}</span>
        </div>
        <div className="flex items-center gap-1.5">
          <div className={cn('w-2.5 h-2.5 rounded-full transition-colors', STATUS_DOT[agent.status])} />
          <span className="text-sm text-gray-400">{agent.status}</span>
        </div>
      </div>

      <div className="text-xs text-gray-500">Persona: {agent.personaName}</div>

      {agent.currentTaskId && (
        <div className="text-xs text-gray-400">
          Task: <TaskTitle taskId={agent.currentTaskId} />
        </div>
      )}

      {agent.currentTaskId && (
        <Link
          to={`/projects/${projectId}/history?task=${agent.currentTaskId}`}
          className="text-xs text-forge-500 hover:text-forge-500/80 transition-colors"
        >
          View task →
        </Link>
      )}
    </div>
  )
}
