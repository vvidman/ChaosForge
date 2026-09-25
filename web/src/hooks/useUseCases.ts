import { useMutation, useQuery } from '@tanstack/react-query'
import { queryClient } from '@/lib/queryClient'
import {
  createUseCase,
  getUseCase,
  getUseCasesByProject,
  updateUseCasePriority,
  type CreateUseCaseData,
} from '@/api/usecases'

const USE_CASES_KEY = ['use-cases'] as const
// Key factories must not start with "use" - react-hooks/rules-of-hooks would treat them as hooks.
const projectUseCasesKey = (projectId: string) => ['use-cases', 'by-project', projectId] as const
const singleUseCaseKey = (id: string) => ['use-cases', id] as const

export function useUseCasesByProject(projectId: string, enabled = true) {
  return useQuery({
    queryKey: projectUseCasesKey(projectId),
    queryFn: () => getUseCasesByProject(projectId),
    enabled,
  })
}

export function useUseCase(id: string, enabled = true) {
  return useQuery({
    queryKey: singleUseCaseKey(id),
    queryFn: () => getUseCase(id),
    enabled,
  })
}

export function useMutateCreateUseCase() {
  return useMutation({
    mutationFn: (data: CreateUseCaseData) => createUseCase(data),
    onSuccess: (_, { projectId }) => {
      queryClient.invalidateQueries({ queryKey: projectUseCasesKey(projectId) })
    },
  })
}

export function useMutateUpdateUseCasePriority() {
  return useMutation({
    mutationFn: ({ id, priority }: { id: string; priority: number }) =>
      updateUseCasePriority(id, priority),
    onSuccess: (_, { id }) => {
      queryClient.invalidateQueries({ queryKey: singleUseCaseKey(id) })
      queryClient.invalidateQueries({ queryKey: USE_CASES_KEY })
    },
  })
}
