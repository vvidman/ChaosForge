import { QueryClient } from '@tanstack/react-query'
import { useAppStore } from '@/store/appStore'
import { ApiError } from '@/lib/api'

export const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      staleTime: 30_000,
      retry: 1,
    },
    mutations: {
      onError: (error) => {
        const message = error instanceof ApiError ? error.message : 'An unexpected error occurred'
        useAppStore.getState().pushNotification(message)
      },
    },
  },
})

queryClient.getQueryCache().config.onError = (error) => {
  const message = error instanceof ApiError ? error.message : 'An unexpected error occurred'
  useAppStore.getState().pushNotification(message)
}
