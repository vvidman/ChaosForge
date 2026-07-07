import { Minus, Plus } from 'lucide-react'
import { useAgentSlotsByProject, useMutateCreateAgentSlot, useMutateUpdateAgentSlotCount } from '@/hooks/useAgentSlots'
import { Button } from '@/components/ui/button'
import type { AgentRole, ProjectStatus } from '@/api/types'

const ALL_ROLES: AgentRole[] = [
  'BusinessAnalyst',
  'Architect',
  'ScrumMaster',
  'Developer',
  'Tester',
  'Reviewer',
  'TechnicalWriter',
]

const SINGLETON_ROLES = new Set<AgentRole>(['BusinessAnalyst', 'Architect', 'ScrumMaster'])

const ROLE_LABELS: Record<AgentRole, string> = {
  BusinessAnalyst: 'Business Analyst',
  Architect: 'Architect',
  ScrumMaster: 'Scrum Master',
  Developer: 'Developer',
  Tester: 'Tester',
  Reviewer: 'Reviewer',
  TechnicalWriter: 'Technical Writer',
}

interface AgentSlotTableProps {
  projectId: string
  projectStatus: ProjectStatus
}

export function AgentSlotTable({ projectId, projectStatus }: AgentSlotTableProps) {
  const { data: slots = [], isLoading } = useAgentSlotsByProject(projectId)
  const { mutate: createSlot, isPending: isCreating } = useMutateCreateAgentSlot()
  const { mutate: updateCount } = useMutateUpdateAgentSlotCount()

  const isSetup = projectStatus === 'Setup'
  const slotMap = new Map(slots.map((s) => [s.role, s]))
  const hasSlots = slots.length > 0

  if (isLoading) {
    return <div className="text-sm text-gray-400">Loading agent slots…</div>
  }

  if (!hasSlots) {
    return (
      <div className="flex flex-col gap-3">
        <p className="text-sm text-gray-400">No agent slots configured yet.</p>
        {isSetup && (
          <Button
            size="sm"
            disabled={isCreating}
            onClick={() => {
              ALL_ROLES.forEach((role) => {
                createSlot({ projectId, role, count: 1 })
              })
            }}
          >
            {isCreating ? 'Configuring…' : 'Configure Agent Slots'}
          </Button>
        )}
      </div>
    )
  }

  function handleDecrement(role: AgentRole) {
    const slot = slotMap.get(role)
    if (!slot || slot.count <= 1) return
    updateCount({ id: slot.id, count: slot.count - 1, projectId })
  }

  function handleIncrement(role: AgentRole) {
    const slot = slotMap.get(role)
    if (!slot) return
    updateCount({ id: slot.id, count: slot.count + 1, projectId })
  }

  return (
    <table className="w-full text-sm">
      <thead>
        <tr className="border-b border-surface-border text-left text-xs text-gray-500 uppercase tracking-wide">
          <th className="pb-2 pr-4 font-medium">Role</th>
          <th className="pb-2 font-medium">Count</th>
        </tr>
      </thead>
      <tbody>
        {ALL_ROLES.map((role) => {
          const slot = slotMap.get(role)
          if (!slot) return null
          const isSingleton = SINGLETON_ROLES.has(role)
          const controlsDisabled = !isSetup || isSingleton

          return (
            <tr key={role} className="border-b border-surface-border/50 last:border-0">
              <td className="py-2 pr-4 text-gray-300">{ROLE_LABELS[role]}</td>
              <td className="py-2">
                <div className="flex items-center gap-2">
                  <button
                    className="flex items-center justify-center w-6 h-6 rounded border border-surface-border text-gray-400 hover:text-white hover:border-gray-500 disabled:opacity-30 disabled:cursor-not-allowed transition-colors"
                    onClick={() => handleDecrement(role)}
                    disabled={controlsDisabled || slot.count <= 1}
                    aria-label={`Decrease ${role} count`}
                  >
                    <Minus size={12} />
                  </button>
                  <span className="w-6 text-center text-white font-medium">{slot.count}</span>
                  <button
                    className="flex items-center justify-center w-6 h-6 rounded border border-surface-border text-gray-400 hover:text-white hover:border-gray-500 disabled:opacity-30 disabled:cursor-not-allowed transition-colors"
                    onClick={() => handleIncrement(role)}
                    disabled={controlsDisabled}
                    aria-label={`Increase ${role} count`}
                  >
                    <Plus size={12} />
                  </button>
                  {isSingleton && (
                    <span className="text-xs text-gray-500 ml-1">singleton</span>
                  )}
                </div>
              </td>
            </tr>
          )
        })}
      </tbody>
    </table>
  )
}
