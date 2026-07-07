import { Loader2 } from 'lucide-react'
import { Badge } from '@/components/ui/badge'
import { cn } from '@/lib/utils'
import type { AttemptResult } from '@/api/types'

interface AttemptResultBadgeProps {
  result: AttemptResult
  className?: string
}

export function AttemptResultBadge({ result, className }: AttemptResultBadgeProps) {
  if (result === 'Pending') {
    return (
      <Badge
        className={cn(
          'border-transparent bg-status-pending/20 text-status-pending flex items-center gap-1',
          className
        )}
      >
        <Loader2 className="animate-spin" size={12} />
        Pending
      </Badge>
    )
  }

  if (result === 'Approved') {
    return (
      <Badge
        className={cn('border-transparent bg-status-done/20 text-status-done', className)}
      >
        Approved
      </Badge>
    )
  }

  return (
    <Badge
      className={cn('border-transparent bg-status-rejected/20 text-status-rejected', className)}
    >
      Rejected
    </Badge>
  )
}
