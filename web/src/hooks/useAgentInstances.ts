import { useMutation, useQuery } from '@tanstack/react-query'
import { queryClient } from '@/lib/queryClient'
import {
  createAgentInstance,
  getAgentInstance,
  getAgentInstancesByProject,
  type CreateAgentInstanceData,
} from '@/api/agentinstances'

const agentInstancesByProjectKey = (projectId: string) =>
  ['agent-instances', 'by-project', projectId] as const
const agentInstanceKey = (id: string) => ['agent-instances', id] as const

export function useAgentInstancesByProject(projectId: string, enabled = true) {
  return useQuery({
    queryKey: agentInstancesByProjectKey(projectId),
    queryFn: () => getAgentInstancesByProject(projectId),
    enabled,
  })
}

export function useAgentInstance(id: string, enabled = true) {
  return useQuery({
    queryKey: agentInstanceKey(id),
    queryFn: () => getAgentInstance(id),
    enabled,
  })
}

export function useMutateCreateAgentInstance() {
  return useMutation({
    mutationFn: (data: CreateAgentInstanceData) => createAgentInstance(data),
    onSuccess: (_, { projectId }) => {
      queryClient.invalidateQueries({ queryKey: agentInstancesByProjectKey(projectId) })
    },
  })
}
