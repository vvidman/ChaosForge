import { useNavigate, useParams, Link } from 'react-router-dom'
import { useOpenRevisionGate } from '@/hooks/useRevisionGates'
import { useProject } from '@/hooks/useProjects'
import { useAppStore } from '@/store/appStore'
import { Skeleton } from '@/components/ui/skeleton'
import AgentOutputViewer from '@/components/gate/AgentOutputViewer'
import GateTypeHeader from '@/components/gate/GateTypeHeader'
import DecisionPanel from '@/components/gate/DecisionPanel'
import { ArrowLeft } from 'lucide-react'
import { useEffect } from 'react'

export default function GatePage() {
  const { id: projectId = '' } = useParams<{ id: string }>()
  const navigate = useNavigate()
  const push = useAppStore((s) => s.push)

  const gateQuery = useOpenRevisionGate(projectId, !!projectId)
  const projectQuery = useProject(projectId, !!projectId)

  const gate = gateQuery.data
  const project = projectQuery.data

  useEffect(() => {
    if (!gateQuery.isLoading && !gateQuery.error && gate === null) {
      navigate(`/projects/${projectId}/overview`, { replace: true })
    }
  }, [gate, gateQuery.isLoading, gateQuery.error, navigate, projectId])

  function handleSuccess() {
    push({ variant: 'success', message: 'Decision submitted successfully.' })
    navigate(`/projects/${projectId}/overview`)
  }

  if (gateQuery.isLoading || projectQuery.isLoading) {
    return (
      <div className="min-h-screen bg-surface p-8 flex flex-col gap-4">
        <Skeleton className="h-12 w-full" />
        <Skeleton className="h-96 w-full" />
      </div>
    )
  }

  if (gateQuery.error) {
    return (
      <div className="min-h-screen bg-surface flex items-center justify-center">
        <div className="text-center space-y-4">
          <p className="text-red-400">Failed to load revision gate.</p>
          <button
            className="text-forge-500 underline"
            onClick={() => gateQuery.refetch()}
          >
            Retry
          </button>
        </div>
      </div>
    )
  }

  if (!gate) {
    return null
  }

  return (
    <div className="min-h-screen bg-surface flex flex-col">
      <header className="flex items-center justify-between px-6 py-4 border-b border-surface-border">
        <Link
          to={`/projects/${projectId}/overview`}
          className="flex items-center gap-2 text-sm text-gray-400 hover:text-white transition-colors"
        >
          <ArrowLeft className="h-4 w-4" />
          Back to project
        </Link>
        <span className="text-white font-medium">{project?.name ?? ''}</span>
      </header>

      <main className="flex-1 p-6">
        <div className="max-w-7xl mx-auto grid grid-cols-1 lg:grid-cols-5 gap-6">
          <div className="lg:col-span-3">
            <AgentOutputViewer agentOutput={gate.agentOutput} />
          </div>
          <div className="lg:col-span-2 flex flex-col gap-4">
            <GateTypeHeader gateType={gate.type} />
            <DecisionPanel gate={gate} onSuccess={handleSuccess} />
          </div>
        </div>
      </main>
    </div>
  )
}
