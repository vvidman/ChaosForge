import { CheckCircle } from 'lucide-react'
import { cn } from '@/lib/utils'
import type { ProjectStatus } from '@/api/types'

const PHASES: { status: ProjectStatus; label: string }[] = [
  { status: 'Setup', label: 'Setup' },
  { status: 'RequirementsPhase', label: 'Requirements' },
  { status: 'ArchitecturePhase', label: 'Architecture' },
  { status: 'SprintPlanning', label: 'Sprint Planning' },
  { status: 'Development', label: 'Development' },
  { status: 'Completed', label: 'Completed' },
]

const PHASE_ORDER: Record<ProjectStatus, number> = {
  Setup: 0,
  RequirementsPhase: 1,
  ArchitecturePhase: 2,
  SprintPlanning: 3,
  Development: 4,
  Completed: 5,
}

interface PhaseStepperProps {
  currentStatus: ProjectStatus
}

export function PhaseStepper({ currentStatus }: PhaseStepperProps) {
  const currentIndex = PHASE_ORDER[currentStatus]

  return (
    <div className="flex items-center gap-0" role="list" aria-label="Project phases">
      {PHASES.map((phase, index) => {
        const isCompleted = index < currentIndex
        const isActive = index === currentIndex
        const isFuture = index > currentIndex

        return (
          <div key={phase.status} className="flex items-center" role="listitem">
            <div className="flex flex-col items-center">
              <div
                className={cn(
                  'flex items-center justify-center w-7 h-7 rounded-full border-2 text-xs font-semibold',
                  isCompleted && 'bg-forge-600 border-forge-600 text-white',
                  isActive && 'bg-forge-500 border-forge-500 text-white',
                  isFuture && 'bg-transparent border-surface-border text-gray-500'
                )}
                aria-current={isActive ? 'step' : undefined}
              >
                {isCompleted ? (
                  <CheckCircle size={14} aria-label="completed" />
                ) : (
                  <span>{index + 1}</span>
                )}
              </div>
              <span
                className={cn(
                  'mt-1 text-xs whitespace-nowrap',
                  isCompleted && 'text-forge-600',
                  isActive && 'text-forge-500 font-semibold',
                  isFuture && 'text-gray-500'
                )}
              >
                {phase.label}
              </span>
            </div>
            {index < PHASES.length - 1 && (
              <div
                className={cn(
                  'h-0.5 w-8 mb-4 mx-1',
                  index < currentIndex ? 'bg-forge-600' : 'bg-surface-border'
                )}
              />
            )}
          </div>
        )
      })}
    </div>
  )
}
