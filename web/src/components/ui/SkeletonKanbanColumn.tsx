import { Skeleton } from '@/components/ui/skeleton'

interface SkeletonKanbanColumnProps {
  columns?: number
  cardsPerColumn?: number
}

function SkeletonColumn({ cards }: { cards: number }) {
  return (
    <div className="flex flex-col gap-3 min-w-[240px] w-[240px]">
      <div className="flex items-center gap-2">
        <Skeleton className="h-4 w-24" />
        <Skeleton className="h-5 w-6 rounded-full" />
      </div>
      <div className="flex flex-col gap-2">
        {Array.from({ length: cards }).map((_, i) => (
          <div key={i} className="rounded-md border border-surface-border bg-surface-card p-3 space-y-2">
            <Skeleton className="h-4 w-3/4" />
            <Skeleton className="h-3 w-full" />
            <Skeleton className="h-3 w-1/2" />
          </div>
        ))}
      </div>
    </div>
  )
}

export function SkeletonKanbanColumn({ columns = 3, cardsPerColumn = 3 }: SkeletonKanbanColumnProps) {
  return (
    <div
      className="flex gap-4 overflow-x-auto"
      aria-busy="true"
      aria-label="Loading..."
    >
      {Array.from({ length: columns }).map((_, i) => (
        <SkeletonColumn key={i} cards={cardsPerColumn} />
      ))}
    </div>
  )
}
