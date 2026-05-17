import { cn } from '@/lib/utils'
import { getRelativeTimeString } from '@/components/ui/relativeTime'

export { getRelativeTimeString }

interface RelativeTimeProps {
  iso: string
  className?: string
}

export function RelativeTime({ iso, className }: RelativeTimeProps) {
  return (
    <span className={cn('text-sm text-gray-400', className)}>
      {getRelativeTimeString(iso)}
    </span>
  )
}
