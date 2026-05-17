import type { RevisionGateType } from '@/api/types'

interface GateTypeHeaderProps {
  gateType: RevisionGateType
}

const gateInfo: Record<RevisionGateType, { title: string; description: string }> = {
  Requirements: {
    title: 'Requirements Review',
    description: "Review the business analyst's requirements document",
  },
  Architecture: {
    title: 'Architecture Review',
    description: "Review the architect's technical design",
  },
  SprintPlanning: {
    title: 'Sprint Planning Review',
    description: 'Review the sprint plan and task breakdown',
  },
}

export default function GateTypeHeader({ gateType }: GateTypeHeaderProps) {
  const { title, description } = gateInfo[gateType]

  return (
    <div className="mb-4">
      <h2 className="text-xl font-semibold text-white">{title}</h2>
      <p className="mt-1 text-sm text-gray-400">{description}</p>
      <p className="mt-2 text-xs font-medium text-status-working">Open — awaiting your decision</p>
    </div>
  )
}
