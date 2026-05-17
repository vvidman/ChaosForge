import { Activity, ArrowRightLeft } from 'lucide-react'
import { ScrollArea } from '@/components/ui/scroll-area'
import { useAppStore } from '@/store/appStore'
import type { AgentEvent } from '@/store/appStore'

function formatTime(iso: string): string {
  return new Date(iso).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit', second: '2-digit' })
}

function EventRow({ event }: { event: AgentEvent }) {
  const Icon = event.type === 'AgentStatusChanged' ? Activity : ArrowRightLeft
  return (
    <div className="flex items-start gap-2 py-1.5 border-b border-surface-border last:border-0">
      <Icon size={13} className="mt-0.5 shrink-0 text-gray-500" />
      <span className="text-xs text-gray-500 shrink-0">{formatTime(event.timestamp)}</span>
      <span className="text-xs text-gray-300">{event.description}</span>
    </div>
  )
}

export function EventLog() {
  const events = useAppStore((s) => s.agentEvents)
  const reversed = [...events].reverse()

  return (
    <div className="flex flex-col gap-2">
      <h3 className="text-sm font-semibold text-gray-400 uppercase tracking-wider">Recent Events</h3>
      <div className="rounded-lg border border-surface-border bg-surface-card">
        {reversed.length === 0 ? (
          <p className="px-4 py-3 text-sm text-gray-500">No events yet.</p>
        ) : (
          <ScrollArea className="h-64">
            <div className="px-4 py-2">
              {reversed.map((e) => (
                <EventRow key={e.id} event={e} />
              ))}
            </div>
          </ScrollArea>
        )}
      </div>
    </div>
  )
}
