import { Badge } from '@/components/ui/badge'
import { cn } from '@/lib/utils'
import type { ProjectStatus } from '@/api/types'
import { statusConfig } from '@/components/ui/statusConfig'

interface StatusBadgeProps {
  status: ProjectStatus
  className?: string
}

export function StatusBadge({ status, className }: StatusBadgeProps) {
  const config = statusConfig[status]

  return (
    <Badge
      role="status"
      aria-label={config.label}
      className={cn('transition-colors duration-300', config.className, className)}
    >
      {config.label}
    </Badge>
  )
}

export { statusConfig }
