import { Badge } from '@/components/ui/badge'
import { cn } from '@/lib/utils'
import { statusConfig } from '@/components/ui/statusConfig'

interface StatusBadgeProps {
  status: string
  className?: string
}

export function StatusBadge({ status, className }: StatusBadgeProps) {
  const config = statusConfig[status] ?? { label: status, className: 'border-surface-border text-gray-400' }

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
