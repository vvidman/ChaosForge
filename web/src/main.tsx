import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import { RouterProvider } from 'react-router-dom'
import { QueryClientProvider } from '@tanstack/react-query'
import { queryClient } from '@/lib/queryClient'
import { router } from '@/router'
import { SignalRProvider } from '@/context/SignalRContext'
import './index.css'

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <QueryClientProvider client={queryClient}>
      <SignalRProvider>
        <RouterProvider router={router} />
      </SignalRProvider>
    </QueryClientProvider>
  </StrictMode>
)
