import { Skeleton } from '@/components/ui/skeleton'

export function SkeletonCard() {
  return (
    <div
      className="rounded-lg border border-surface-border bg-surface-card p-5 space-y-3"
      aria-busy="true"
      aria-label="Loading..."
    >
      <div className="flex justify-between">
        <Skeleton className="h-5 w-24" />
        <Skeleton className="h-4 w-16" />
      </div>
      <Skeleton className="h-5 w-3/4" />
      <Skeleton className="h-4 w-full" />
      <Skeleton className="h-4 w-2/3" />
      <div className="flex justify-between pt-1">
        <Skeleton className="h-3 w-28" />
        <Skeleton className="h-4 w-4" />
      </div>
    </div>
  )
}
