import { useMutation, useQuery } from '@tanstack/react-query'
import { queryClient } from '@/lib/queryClient'
import {
  acceptGate,
  editAndAcceptGate,
  getOpenRevisionGate,
  getRevisionGate,
  getRevisionGatesByProject,
  openRevisionGate,
  rejectGate,
  type OpenRevisionGateData,
} from '@/api/revisiongates'

const revisionGatesByProjectKey = (projectId: string) =>
  ['revision-gates', 'by-project', projectId] as const
const openRevisionGateKey = (projectId: string) =>
  ['revision-gates', 'open', projectId] as const
const revisionGateKey = (id: string) => ['revision-gates', id] as const

export function useRevisionGatesByProject(projectId: string, enabled = true) {
  return useQuery({
    queryKey: revisionGatesByProjectKey(projectId),
    queryFn: () => getRevisionGatesByProject(projectId),
    enabled,
  })
}

export function useRevisionGate(id: string, enabled = true) {
  return useQuery({
    queryKey: revisionGateKey(id),
    queryFn: () => getRevisionGate(id),
    enabled,
  })
}

export function useOpenRevisionGate(projectId: string, enabled = true) {
  return useQuery({
    queryKey: openRevisionGateKey(projectId),
    queryFn: () => getOpenRevisionGate(projectId),
    enabled,
  })
}

export function useMutateOpenRevisionGate() {
  return useMutation({
    mutationFn: (data: OpenRevisionGateData) => openRevisionGate(data),
    onSuccess: (_, { projectId }) => {
      queryClient.invalidateQueries({ queryKey: revisionGatesByProjectKey(projectId) })
      queryClient.invalidateQueries({ queryKey: openRevisionGateKey(projectId) })
    },
  })
}

export function useMutateAcceptGate() {
  return useMutation({
    mutationFn: ({ id }: { id: string; projectId: string }) => acceptGate(id),
    onSuccess: (_, { id, projectId }) => {
      queryClient.invalidateQueries({ queryKey: revisionGateKey(id) })
      queryClient.invalidateQueries({ queryKey: revisionGatesByProjectKey(projectId) })
      queryClient.invalidateQueries({ queryKey: openRevisionGateKey(projectId) })
    },
  })
}

export function useMutateEditAndAcceptGate() {
  return useMutation({
    mutationFn: ({ id, editedOutput }: { id: string; projectId: string; editedOutput: string }) =>
      editAndAcceptGate(id, editedOutput),
    onSuccess: (_, { id, projectId }) => {
      queryClient.invalidateQueries({ queryKey: revisionGateKey(id) })
      queryClient.invalidateQueries({ queryKey: revisionGatesByProjectKey(projectId) })
      queryClient.invalidateQueries({ queryKey: openRevisionGateKey(projectId) })
    },
  })
}

export function useMutateRejectGate() {
  return useMutation({
    mutationFn: ({ id, reason }: { id: string; projectId: string; reason: string }) =>
      rejectGate(id, reason),
    onSuccess: (_, { id, projectId }) => {
      queryClient.invalidateQueries({ queryKey: revisionGateKey(id) })
      queryClient.invalidateQueries({ queryKey: revisionGatesByProjectKey(projectId) })
      queryClient.invalidateQueries({ queryKey: openRevisionGateKey(projectId) })
    },
  })
}
