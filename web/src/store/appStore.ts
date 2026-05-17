import { create } from 'zustand'

export type ConnectionStatus = 'connecting' | 'connected' | 'disconnected'

export interface AppNotification {
  id: string
  message: string
  variant: 'info' | 'success' | 'error'
}

const MAX_NOTIFICATIONS = 5

interface AppState {
  status: ConnectionStatus
  notifications: AppNotification[]
  setStatus: (status: ConnectionStatus) => void
  push: (n: Omit<AppNotification, 'id'>) => void
  dismiss: (id: string) => void
}

export const useAppStore = create<AppState>((set) => ({
  status: 'disconnected',
  notifications: [],
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
}))
