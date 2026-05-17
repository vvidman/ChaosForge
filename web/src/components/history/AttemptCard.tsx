import { useState } from 'react'
import { Loader2 } from 'lucide-react'
import { cn } from '@/lib/utils'
import { Button } from '@/components/ui/button'
import AgentOutputViewer from '@/components/gate/AgentOutputViewer'
import { getRelativeTimeString } from '@/components/ui/relativeTime'
import { useAgentInstance } from '@/hooks/useAgentInstances'
import { AttemptTypeBadge } from './AttemptTypeBadge'
import { AttemptResultBadge } from './AttemptResultBadge'
import type { TaskAttemptDto } from '@/api/types'

interface AttemptCardProps {
  attempt: TaskAttemptDto
}

export function AttemptCard({ attempt }: AttemptCardProps) {
  const [expanded, setExpanded] = useState(false)
  const { data: agent } = useAgentInstance(attempt.agentInstanceId)
  const personaName = agent?.personaName ?? attempt.agentInstanceId.slice(0, 8)

  return (
    <div className="rounded-lg border border-surface-border bg-surface-card p-4 space-y-3">
      <div className="flex items-center justify-between flex-wrap gap-2">
        <div className="flex items-center gap-2">
          <AttemptTypeBadge type={attempt.type} />
          <AttemptResultBadge result={attempt.result} />
        </div>
        <div className="flex items-center gap-3 text-xs text-gray-400">
          <span>Agent: <span className="text-gray-300">{personaName}</span></span>
          <span>{getRelativeTimeString(attempt.startedAt)}</span>
        </div>
      </div>

      {attempt.result === 'Pending' ? (
        <div className="flex items-center gap-2 text-sm text-gray-400">
          <Loader2 className="animate-spin" size={14} />
          <span>Agent is working...</span>
        </div>
      ) : (
        <div>
          <Button
            variant="ghost"
            size="sm"
            onClick={() => setExpanded((v) => !v)}
            className="text-gray-400 hover:text-white px-0 h-auto"
          >
            {expanded ? 'Collapse ↑' : 'Expand ↓'}
          </Button>
          {expanded && (
            <div className="mt-2">
              <AgentOutputViewer agentOutput={attempt.output} />
            </div>
          )}
        </div>
      )}

      {attempt.result === 'Rejected' && (
        <div className={cn(
          'rounded-md border border-status-rejected/30 bg-status-rejected/10 px-3 py-2',
          'text-sm text-status-rejected'
        )}>
          ⚠ {attempt.reviewNote ?? attempt.testNote}
        </div>
      )}
    </div>
  )
}
