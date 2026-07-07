import { create } from 'zustand'

export type ConnectionStatus = 'connecting' | 'connected' | 'disconnected'

export interface AppNotification {
  id: string
  message: string
  variant: 'info' | 'success' | 'error'
}

export interface AgentEvent {
  id: string
  timestamp: string
  type: 'AgentStatusChanged' | 'WorkTaskStatusChanged'
  description: string
}

const MAX_NOTIFICATIONS = 5
const MAX_AGENT_EVENTS = 50

interface AppState {
  status: ConnectionStatus
  notifications: AppNotification[]
  agentEvents: AgentEvent[]
  sidebarCollapsed: boolean
  setStatus: (status: ConnectionStatus) => void
  push: (n: Omit<AppNotification, 'id'>) => void
  dismiss: (id: string) => void
  pushAgentEvent: (e: Omit<AgentEvent, 'id'>) => void
  setSidebarCollapsed: (collapsed: boolean) => void
}

export const useAppStore = create<AppState>((set) => ({
  status: 'disconnected',
  notifications: [],
  agentEvents: [],
  sidebarCollapsed: false,
  setStatus: (status) => set({ status }),
  push: (n) =>
    set((state) => {
      const next = [...state.notifications, { ...n, id: crypto.randomUUID() }]
      return { notifications: next.length > MAX_NOTIFICATIONS ? next.slice(next.length - MAX_NOTIFICATIONS) : next }
    }),
  dismiss: (id) =>
    set((state) => ({
      notifications: state.notifications.filter((n) => n.id !== id),
    })),
  pushAgentEvent: (e) =>
    set((state) => {
      const next = [...state.agentEvents, { ...e, id: crypto.randomUUID() }]
      return { agentEvents: next.length > MAX_AGENT_EVENTS ? next.slice(next.length - MAX_AGENT_EVENTS) : next }
    }),
  setSidebarCollapsed: (collapsed) => set({ sidebarCollapsed: collapsed }),
}))
