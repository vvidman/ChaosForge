import { cn } from '@/lib/utils'
import { Badge } from '@/components/ui/badge'
import type { UseCaseDto } from '@/api/types'

interface UseCaseCardProps {
  useCase: UseCaseDto
  isSelected: boolean
  onClick: () => void
  dragHandle?: React.ReactNode
}

export default function UseCaseCard({
  useCase,
  isSelected,
  onClick,
  dragHandle,
}: UseCaseCardProps) {
  return (
    <div
      className={cn(
        'flex items-start gap-2 p-3 rounded-md border cursor-pointer bg-surface-card hover:bg-surface-hover transition-colors',
        isSelected
          ? 'border-l-4 border-l-forge-500 border-surface-border'
          : 'border-surface-border'
      )}
      onClick={onClick}
    >
      {dragHandle}
      <div className="flex-1 min-w-0">
        <div className="flex items-center gap-2 mb-1">
          <Badge variant="outline" className="text-xs shrink-0">
            P{useCase.priority}
          </Badge>
          <span className="text-sm font-medium text-white truncate">{useCase.title}</span>
        </div>
        <p className="text-xs text-gray-400 line-clamp-2">{useCase.description}</p>
      </div>
    </div>
  )
}
