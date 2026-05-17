import { useMutation, useQuery } from '@tanstack/react-query'
import { queryClient } from '@/lib/queryClient'
import {
  applyHumanEditToURS,
  createURS,
  getURS,
  getURSsByUseCase,
  type ApplyHumanEditToURSData,
  type CreateURSData,
} from '@/api/urs'

const urssByUseCaseKey = (useCaseId: string) => ['urs', 'by-usecase', useCaseId] as const
const ursKey = (id: string) => ['urs', id] as const

export function useURSsByUseCase(useCaseId: string, enabled = true) {
  return useQuery({
    queryKey: urssByUseCaseKey(useCaseId),
    queryFn: () => getURSsByUseCase(useCaseId),
    enabled,
  })
}

export function useURS(id: string, enabled = true) {
  return useQuery({
    queryKey: ursKey(id),
    queryFn: () => getURS(id),
    enabled,
  })
}

export function useMutateCreateURS() {
  return useMutation({
    mutationFn: (data: CreateURSData) => createURS(data),
    onSuccess: (_, { useCaseId }) => {
      queryClient.invalidateQueries({ queryKey: urssByUseCaseKey(useCaseId) })
    },
  })
}

export function useMutateApplyHumanEditToURS() {
  return useMutation({
    mutationFn: ({ id, ...data }: { id: string } & ApplyHumanEditToURSData) =>
      applyHumanEditToURS(id, data),
    onSuccess: (_, { id }) => {
      queryClient.invalidateQueries({ queryKey: ursKey(id) })
    },
  })
}
