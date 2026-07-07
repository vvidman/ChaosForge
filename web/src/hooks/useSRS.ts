import { useMutation, useQuery } from '@tanstack/react-query'
import { queryClient } from '@/lib/queryClient'
import {
  applyHumanEditToSRS,
  createSRS,
  getSRS,
  getSRSsByURS,
  type ApplyHumanEditToSRSData,
  type CreateSRSData,
} from '@/api/srs'

const srssByURSKey = (ursId: string) => ['srs', 'by-urs', ursId] as const
const srsKey = (id: string) => ['srs', id] as const

export function useSRSsByURS(ursId: string, enabled = true) {
  return useQuery({
    queryKey: srssByURSKey(ursId),
    queryFn: () => getSRSsByURS(ursId),
    enabled,
  })
}

export function useSRS(id: string, enabled = true) {
  return useQuery({
    queryKey: srsKey(id),
    queryFn: () => getSRS(id),
    enabled,
  })
}

export function useMutateCreateSRS() {
  return useMutation({
    mutationFn: (data: CreateSRSData) => createSRS(data),
    onSuccess: (_, { ursId }) => {
      queryClient.invalidateQueries({ queryKey: srssByURSKey(ursId) })
    },
  })
}

export function useMutateApplyHumanEditToSRS() {
  return useMutation({
    mutationFn: ({ id, ...data }: { id: string } & ApplyHumanEditToSRSData) =>
      applyHumanEditToSRS(id, data),
    onSuccess: (_, { id }) => {
      queryClient.invalidateQueries({ queryKey: srsKey(id) })
    },
  })
}
