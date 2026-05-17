import { Skeleton } from '@/components/ui/skeleton'

interface SkeletonTimelineProps {
  items?: number
}

export function SkeletonTimeline({ items = 3 }: SkeletonTimelineProps) {
  return (
    <div
      className="flex flex-col space-y-4"
      aria-busy="true"
      aria-label="Loading..."
    >
      <div className="rounded-lg border border-surface-border bg-surface-card p-4 space-y-3">
        <div className="flex items-center gap-3">
          <Skeleton className="h-6 w-48" />
          <Skeleton className="h-5 w-20 rounded-full" />
        </div>
        <Skeleton className="h-4 w-full" />
        <Skeleton className="h-4 w-3/4" />
      </div>
      <div className="relative pl-6">
        <div className="absolute left-2.5 top-0 bottom-0 w-px bg-surface-border" />
        <div className="space-y-4">
          {Array.from({ length: items }).map((_, i) => (
            <div key={i} className="relative">
              <div className="absolute -left-[17px] top-4 w-2.5 h-2.5 rounded-full bg-surface-border border-2 border-surface-card" />
              <div className="rounded-lg border border-surface-border bg-surface-card p-4 space-y-2">
                <div className="flex justify-between">
                  <Skeleton className="h-4 w-24" />
                  <Skeleton className="h-4 w-20" />
                </div>
                <Skeleton className="h-3 w-full" />
                <Skeleton className="h-3 w-2/3" />
              </div>
            </div>
          ))}
        </div>
      </div>
    </div>
  )
}
