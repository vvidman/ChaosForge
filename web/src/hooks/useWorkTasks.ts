import { useMutation, useQuery } from '@tanstack/react-query'
import { queryClient } from '@/lib/queryClient'
import {
  approveTask,
  assignToSprint,
  completeTask,
  createWorkTask,
  getWorkTask,
  getWorkTasksByProject,
  getWorkTasksBySRS,
  passTesting,
  rejectTask,
  sendToReview,
  startTask,
  type CreateWorkTaskData,
} from '@/api/worktasks'

const workTasksBySRSKey = (srsId: string) => ['work-tasks', 'by-srs', srsId] as const
const workTasksByProjectKey = (projectId: string) => ['work-tasks', 'by-project', projectId] as const
const workTaskKey = (id: string) => ['work-tasks', id] as const

export function useWorkTasksBySRS(srsId: string, enabled = true) {
  return useQuery({
    queryKey: workTasksBySRSKey(srsId),
    queryFn: () => getWorkTasksBySRS(srsId),
    enabled,
  })
}

export function useWorkTasksByProject(projectId: string, enabled = true) {
  return useQuery({
    queryKey: workTasksByProjectKey(projectId),
    queryFn: () => getWorkTasksByProject(projectId),
    enabled,
  })
}

export function useWorkTask(id: string, enabled = true) {
  return useQuery({
    queryKey: workTaskKey(id),
    queryFn: () => getWorkTask(id),
    enabled,
  })
}

export function useMutateCreateWorkTask() {
  return useMutation({
    mutationFn: (data: CreateWorkTaskData) => createWorkTask(data),
    onSuccess: (_, { srsId }) => {
      queryClient.invalidateQueries({ queryKey: workTasksBySRSKey(srsId) })
    },
  })
}

export function useMutateAssignToSprint() {
  return useMutation({
    mutationFn: ({ id, sprintId }: { id: string; sprintId: string }) =>
      assignToSprint(id, sprintId),
    onSuccess: (_, { id }) => {
      queryClient.invalidateQueries({ queryKey: workTaskKey(id) })
    },
  })
}

export function useMutateStartTask() {
  return useMutation({
    mutationFn: ({ id }: { id: string }) => startTask(id),
    onSuccess: (_, { id }) => {
      queryClient.invalidateQueries({ queryKey: workTaskKey(id) })
    },
  })
}

export function useMutateSendToReview() {
  return useMutation({
    mutationFn: ({ id }: { id: string }) => sendToReview(id),
    onSuccess: (_, { id }) => {
      queryClient.invalidateQueries({ queryKey: workTaskKey(id) })
    },
  })
}

export function useMutateApproveTask() {
  return useMutation({
    mutationFn: ({ id }: { id: string }) => approveTask(id),
    onSuccess: (_, { id }) => {
      queryClient.invalidateQueries({ queryKey: workTaskKey(id) })
    },
  })
}

export function useMutatePassTesting() {
  return useMutation({
    mutationFn: ({ id }: { id: string }) => passTesting(id),
    onSuccess: (_, { id }) => {
      queryClient.invalidateQueries({ queryKey: workTaskKey(id) })
    },
  })
}

export function useMutateCompleteTask() {
  return useMutation({
    mutationFn: ({ id }: { id: string }) => completeTask(id),
    onSuccess: (_, { id }) => {
      queryClient.invalidateQueries({ queryKey: workTaskKey(id) })
    },
  })
}

export function useMutateRejectTask() {
  return useMutation({
    mutationFn: ({ id }: { id: string }) => rejectTask(id),
    onSuccess: (_, { id }) => {
      queryClient.invalidateQueries({ queryKey: workTaskKey(id) })
    },
  })
}
