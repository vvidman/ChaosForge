import { Badge } from '@/components/ui/badge'
import { cn } from '@/lib/utils'
import type { AttemptType } from '@/api/types'

const TYPE_STYLES: Record<AttemptType, string> = {
  Implementation: 'bg-forge-500/20 text-forge-500',
  Review: 'bg-purple-500/20 text-purple-400',
  Testing: 'bg-amber-500/20 text-amber-400',
  Documentation: 'bg-teal-500/20 text-teal-400',
}

interface AttemptTypeBadgeProps {
  type: AttemptType
  className?: string
}

export function AttemptTypeBadge({ type, className }: AttemptTypeBadgeProps) {
  return (
    <Badge className={cn('border-transparent', TYPE_STYLES[type], className)}>
      {type}
    </Badge>
  )
}
