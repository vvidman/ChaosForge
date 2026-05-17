import { createContext, useContext, useEffect, type ReactNode } from 'react'
import { HubConnectionBuilder, LogLevel } from '@microsoft/signalr'
import { queryClient } from '@/lib/queryClient'
import { useAppStore, type ConnectionStatus } from '@/store/appStore'

const BASE_URL = import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5143'
const HUB_URL = `${BASE_URL}/hubs/chaosforge`

interface SignalREvent {
  type: string
  payload: Record<string, unknown>
}

interface SignalRContextValue {
  status: ConnectionStatus
}

const SignalRContext = createContext<SignalRContextValue | null>(null)

type PushFn = ReturnType<typeof useAppStore.getState>['push']
type PushAgentEventFn = ReturnType<typeof useAppStore.getState>['pushAgentEvent']

function dispatchEvent(event: SignalREvent, push: PushFn, pushAgentEvent: PushAgentEventFn) {
  const { type, payload } = event
  const projectId = payload['projectId'] as string | undefined
  const workTaskId = payload['workTaskId'] as string | undefined

  switch (type) {
    case 'ProjectStatusChanged':
      queryClient.invalidateQueries({ queryKey: ['projects'] })
      if (projectId) queryClient.invalidateQueries({ queryKey: ['projects', projectId] })
      break

    case 'WorkTaskStatusChanged':
      queryClient.invalidateQueries({ queryKey: ['work-tasks'] })
      push({ message: 'Task status changed', variant: 'info' })
      pushAgentEvent({
        timestamp: new Date().toISOString(),
        type: 'WorkTaskStatusChanged',
        description: `Task status → ${(payload['status'] as string) ?? 'unknown'}`,
      })
      break

    case 'AgentStatusChanged':
      if (projectId)
        queryClient.invalidateQueries({ queryKey: ['agent-instances', 'by-project', projectId] })
      push({ message: 'Agent status changed', variant: 'info' })
      pushAgentEvent({
        timestamp: new Date().toISOString(),
        type: 'AgentStatusChanged',
        description: `Agent ${(payload['role'] as string) ?? ''} ${(payload['personaName'] as string) ?? ''} → ${(payload['status'] as string) ?? ''}`,
      })
      break

    case 'RevisionGateResolved':
      if (projectId) {
        queryClient.invalidateQueries({ queryKey: ['revision-gates', 'by-project', projectId] })
        queryClient.invalidateQueries({ queryKey: ['revision-gates', 'open', projectId] })
      }
      break

    case 'TaskAttemptCompleted':
      if (workTaskId)
        queryClient.invalidateQueries({ queryKey: ['task-attempts', 'by-task', workTaskId] })
      break

    case 'TaskAttemptResolved':
      if (workTaskId)
        queryClient.invalidateQueries({ queryKey: ['task-attempts', 'by-task', workTaskId] })
      break

    default:
      break
  }
}

export function SignalRProvider({ children }: { children: ReactNode }) {
  const setStatus = useAppStore((s) => s.setStatus)
  const push = useAppStore((s) => s.push)
  const pushAgentEvent = useAppStore((s) => s.pushAgentEvent)

  useEffect(() => {
    const connection = new HubConnectionBuilder()
      .withUrl(HUB_URL)
      .withAutomaticReconnect()
      .configureLogging(import.meta.env.DEV ? LogLevel.Information : LogLevel.None)
      .build()

    const updateStatus = (s: ConnectionStatus) => setStatus(s)

    connection.onreconnecting(() => {
      updateStatus('connecting')
      if (import.meta.env.DEV) console.log('[SignalR] reconnecting')
    })
    connection.onreconnected(() => {
      updateStatus('connected')
      if (import.meta.env.DEV) console.log('[SignalR] reconnected')
    })
    connection.onclose(() => {
      updateStatus('disconnected')
      if (import.meta.env.DEV) console.log('[SignalR] disconnected')
    })

    connection.on('ReceiveEvent', (event: SignalREvent) => dispatchEvent(event, push, pushAgentEvent))

    updateStatus('connecting')
    connection
      .start()
      .then(() => {
        updateStatus('connected')
        if (import.meta.env.DEV) console.log('[SignalR] connected')
      })
      .catch(() => updateStatus('disconnected'))

    return () => {
      connection.stop()
    }
  }, [setStatus, push, pushAgentEvent])

  const status = useAppStore((s) => s.status)

  return <SignalRContext.Provider value={{ status }}>{children}</SignalRContext.Provider>
}

export function useSignalR(): SignalRContextValue {
  const ctx = useContext(SignalRContext)
  if (ctx === null) throw new Error('useSignalR must be used within SignalRProvider')
  return ctx
}
