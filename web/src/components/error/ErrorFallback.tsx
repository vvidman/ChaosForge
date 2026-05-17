import { useNavigate } from 'react-router-dom'
import { Button } from '@/components/ui/button'

interface ErrorFallbackProps {
  error: Error
  onReset: () => void
}

export function ErrorFallback({ error, onReset }: ErrorFallbackProps) {
  const navigate = useNavigate()

  return (
    <div
      role="alert"
      className="flex flex-col items-center justify-center min-h-[300px] p-8 text-center"
    >
      <div className="rounded-lg border border-surface-border bg-surface-card p-8 max-w-md w-full space-y-4">
        <div className="text-2xl">⚠</div>
        <h2 className="text-lg font-semibold text-white">Something went wrong</h2>
        <p className="text-sm text-gray-400 break-words">{error.message}</p>
        <div className="flex gap-3 justify-center pt-2">
          <Button onClick={onReset}>Try again</Button>
          <Button variant="outline" onClick={() => navigate('/projects')}>
            Go to projects
          </Button>
        </div>
      </div>
    </div>
  )
}
