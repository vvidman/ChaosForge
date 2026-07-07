import { useMutation, useQuery } from '@tanstack/react-query'
import { queryClient } from '@/lib/queryClient'
import {
  createAgentSlot,
  getAgentSlotsByProject,
  updateAgentSlotCount,
  type CreateAgentSlotData,
} from '@/api/agentslots'

const agentSlotsByProjectKey = (projectId: string) =>
  ['agent-slots', 'by-project', projectId] as const

export function useAgentSlotsByProject(projectId: string, enabled = true) {
  return useQuery({
    queryKey: agentSlotsByProjectKey(projectId),
    queryFn: () => getAgentSlotsByProject(projectId),
    enabled,
  })
}

export function useMutateCreateAgentSlot() {
  return useMutation({
    mutationFn: (data: CreateAgentSlotData) => createAgentSlot(data),
    onSuccess: (_, { projectId }) => {
      queryClient.invalidateQueries({ queryKey: agentSlotsByProjectKey(projectId) })
    },
  })
}

export function useMutateUpdateAgentSlotCount() {
  return useMutation({
    mutationFn: ({ id, count, projectId: _ }: { id: string; count: number; projectId: string }) =>
      updateAgentSlotCount(id, count),
    onSuccess: (_, { projectId }) => {
      queryClient.invalidateQueries({ queryKey: agentSlotsByProjectKey(projectId) })
    },
  })
}
