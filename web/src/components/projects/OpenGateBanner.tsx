import { useNavigate } from 'react-router-dom'
import { AlertTriangle } from 'lucide-react'
import { useOpenRevisionGate } from '@/hooks/useRevisionGates'
import { Button } from '@/components/ui/button'

interface OpenGateBannerProps {
  projectId: string
}

export function OpenGateBanner({ projectId }: OpenGateBannerProps) {
  const { data: gate } = useOpenRevisionGate(projectId)
  const navigate = useNavigate()

  if (!gate) return null

  return (
    <div className="flex items-center justify-between gap-4 rounded-md border border-amber-500/40 bg-amber-500/10 px-4 py-3 text-amber-400">
      <div className="flex items-center gap-2 text-sm font-medium">
        <AlertTriangle size={16} />
        <span>Human review required — {gate.type}</span>
      </div>
      <Button
        size="sm"
        variant="outline"
        className="border-amber-500/50 text-amber-400 hover:bg-amber-500/20 hover:text-amber-300"
        onClick={() => navigate(`/projects/${projectId}/gate`)}
      >
        Review now →
      </Button>
    </div>
  )
}
