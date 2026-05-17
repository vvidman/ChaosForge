import { useEffect } from 'react'
import { useAppStore } from '@/store/appStore'
import { cn } from '@/lib/utils'

const VARIANT_CLASSES = {
  info: 'bg-surface-card border-surface-border text-gray-200',
  success: 'bg-surface-card border-status-done/40 text-gray-200',
  error: 'bg-surface-card border-red-500/40 text-gray-200',
}

function Toast({ id, message, variant }: { id: string; message: string; variant: 'info' | 'success' | 'error' }) {
  const dismiss = useAppStore((s) => s.dismiss)

  useEffect(() => {
    const timer = setTimeout(() => dismiss(id), 4000)
    return () => clearTimeout(timer)
  }, [id, dismiss])

  return (
    <div
      className={cn(
        'flex items-start justify-between gap-3 rounded-md border px-4 py-3 shadow-lg text-sm',
        'animate-in slide-in-from-bottom-2 fade-in duration-200',
        VARIANT_CLASSES[variant]
      )}
    >
      <span>{message}</span>
      <button
        onClick={() => dismiss(id)}
        className="shrink-0 text-gray-500 hover:text-white transition-colors leading-none"
        aria-label="Dismiss"
      >
        ✕
      </button>
    </div>
  )
}

export function ToastContainer() {
  const notifications = useAppStore((s) => s.notifications)

  if (notifications.length === 0) return null

  return (
    <div className="fixed bottom-4 right-4 z-50 flex flex-col gap-2 w-80">
      {notifications.map((n) => (
        <Toast key={n.id} {...n} />
      ))}
    </div>
  )
}
