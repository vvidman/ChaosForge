import { Skeleton } from '@/components/ui/skeleton'

interface SkeletonTableProps {
  rows?: number
}

export function SkeletonTable({ rows = 7 }: SkeletonTableProps) {
  return (
    <div
      className="space-y-3"
      aria-busy="true"
      aria-label="Loading..."
    >
      <div className="flex gap-4 pb-2 border-b border-surface-border">
        <Skeleton className="h-3 w-24" />
        <Skeleton className="h-3 w-16" />
      </div>
      {Array.from({ length: rows }).map((_, i) => (
        <div key={i} className="flex items-center gap-4 py-1">
          <Skeleton className="h-4 w-32" />
          <Skeleton className="h-6 w-20" />
        </div>
      ))}
    </div>
  )
}
