import { NavLink, Outlet, useNavigate, useParams } from 'react-router-dom'
import { Loader2 } from 'lucide-react'
import { cn } from '@/lib/utils'
import { useProject, useMutateTransitionProject } from '@/hooks/useProjects'
import { useOpenRevisionGate } from '@/hooks/useRevisionGates'
import { useAppStore } from '@/store/appStore'
import { Badge } from '@/components/ui/badge'
import { Button } from '@/components/ui/button'
import { PhaseStepper } from '@/components/projects/PhaseStepper'
import { OpenGateBanner } from '@/components/projects/OpenGateBanner'
import { InlineEditDescription } from '@/components/projects/InlineEditDescription'
import type { ProjectStatus } from '@/api/types'

const TAB_LINKS = [
  { to: 'overview', label: 'Overview' },
  { to: 'requirements', label: 'Requirements' },
  { to: 'sprint', label: 'Sprint' },
  { to: 'agents', label: 'Agents' },
  { to: 'history', label: 'History' },
]

const STATUS_VARIANT: Record<ProjectStatus, 'default' | 'secondary' | 'outline'> = {
  Setup: 'secondary',
  RequirementsPhase: 'default',
  ArchitecturePhase: 'default',
  SprintPlanning: 'default',
  Development: 'default',
  Completed: 'outline',
}

const STATUS_LABEL: Record<ProjectStatus, string> = {
  Setup: 'Setup',
  RequirementsPhase: 'Requirements',
  ArchitecturePhase: 'Architecture',
  SprintPlanning: 'Sprint Planning',
  Development: 'Development',
  Completed: 'Completed',
}

function PhaseAction({ projectId, status }: { projectId: string; status: ProjectStatus }) {
  const navigate = useNavigate()
  const push = useAppStore((s) => s.push)
  const { data: gate } = useOpenRevisionGate(projectId)
  const { mutate: transition, isPending } = useMutateTransitionProject()

  if (status === 'Setup') {
    return (
      <Button
        size="sm"
        disabled={isPending}
        onClick={() =>
          transition(
            { id: projectId, newStatus: 'RequirementsPhase' },
            {
              onError: () => push({ message: 'Failed to start requirements phase', variant: 'error' }),
            }
          )
        }
      >
        {isPending ? <Loader2 size={14} className="animate-spin" /> : null}
        Start Requirements Phase
      </Button>
    )
  }

  if (
    status === 'RequirementsPhase' ||
    status === 'ArchitecturePhase' ||
    status === 'SprintPlanning'
  ) {
    if (gate) {
      return (
        <Button size="sm" onClick={() => navigate(`/projects/${projectId}/gate`)}>
          Review gate →
        </Button>
      )
    }
    return (
      <div className="flex items-center gap-2 text-sm text-gray-400">
        <Loader2 size={14} className="animate-spin" />
        Waiting for agent…
      </div>
    )
  }

  if (status === 'Completed') {
    return (
      <div className="rounded-md bg-status-done/10 border border-status-done/30 px-3 py-2 text-sm text-status-done">
        Project complete
      </div>
    )
  }

  return null
}

export default function ProjectDetailLayout() {
  const { id } = useParams<{ id: string }>()
  const { data: project, isLoading } = useProject(id!)

  if (isLoading) {
    return (
      <div className="flex items-center gap-2 text-gray-400 py-8">
        <Loader2 size={18} className="animate-spin" />
        <span>Loading project…</span>
      </div>
    )
  }

  if (!project) {
    return <div className="text-gray-400 py-8">Project not found.</div>
  }

  return (
    <div className="flex flex-col gap-4">
      <OpenGateBanner projectId={project.id} />

      {/* Header */}
      <div className="flex flex-col gap-1">
        <div className="flex items-center gap-3 flex-wrap">
          <h1 className="text-2xl font-semibold text-white">{project.name}</h1>
          <Badge variant={STATUS_VARIANT[project.status]}>{STATUS_LABEL[project.status]}</Badge>
          {project.deadline && (
            <span className="text-xs text-gray-500">
              Deadline: {new Date(project.deadline).toLocaleDateString()}
            </span>
          )}
        </div>
        <InlineEditDescription projectId={project.id} description={project.description} />
      </div>

      {/* Phase stepper + action */}
      <div className="flex flex-col gap-3 rounded-md border border-surface-border bg-surface-card px-4 py-3">
        <PhaseStepper currentStatus={project.status} />
        <PhaseAction projectId={project.id} status={project.status} />
      </div>

      {/* Tab nav */}
      <nav className="flex gap-1 border-b border-surface-border">
        {TAB_LINKS.map(({ to, label }) => (
          <NavLink
            key={to}
            to={to}
            className={({ isActive }) =>
              cn(
                'px-3 py-2 text-sm font-medium border-b-2 -mb-px transition-colors',
                isActive
                  ? 'border-forge-500 text-forge-500'
                  : 'border-transparent text-gray-400 hover:text-white hover:border-gray-500'
              )
            }
          >
            {label}
          </NavLink>
        ))}
      </nav>

      {/* Tab content */}
      <Outlet />
    </div>
  )
}
