import { create } from 'zustand'

export type ConnectionStatus = 'connected' | 'disconnected' | 'reconnecting'

export interface AppNotification {
  id: string
  message: string
  timestamp: number
}

interface AppState {
  connectionStatus: ConnectionStatus
  notificationQueue: AppNotification[]
  setConnectionStatus: (status: ConnectionStatus) => void
  pushNotification: (message: string) => void
  dismissNotification: (id: string) => void
}

export const useAppStore = create<AppState>((set) => ({
  connectionStatus: 'disconnected',
  notificationQueue: [],
  setConnectionStatus: (status) => set({ connectionStatus: status }),
  pushNotification: (message) =>
    set((state) => ({
      notificationQueue: [
        ...state.notificationQueue,
        { id: crypto.randomUUID(), message, timestamp: Date.now() },
      ],
    })),
  dismissNotification: (id) =>
    set((state) => ({
      notificationQueue: state.notificationQueue.filter((n) => n.id !== id),
    })),
}))
