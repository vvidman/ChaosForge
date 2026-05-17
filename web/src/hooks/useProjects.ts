import { useMutation, useQuery } from '@tanstack/react-query'
import { queryClient } from '@/lib/queryClient'
import {
  createProject,
  getProject,
  getProjects,
  transitionProject,
  updateProjectDescription,
  type CreateProjectData,
} from '@/api/projects'
import type { ProjectStatus } from '@/api/types'

const PROJECTS_KEY = ['projects'] as const
const projectKey = (id: string) => ['projects', id] as const

export function useProjects(enabled = true) {
  return useQuery({
    queryKey: PROJECTS_KEY,
    queryFn: getProjects,
    enabled,
  })
}

export function useProject(id: string, enabled = true) {
  return useQuery({
    queryKey: projectKey(id),
    queryFn: () => getProject(id),
    enabled,
  })
}

export function useMutateCreateProject() {
  return useMutation({
    mutationFn: (data: CreateProjectData) => createProject(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: PROJECTS_KEY })
    },
  })
}

export function useMutateTransitionProject() {
  return useMutation({
    mutationFn: ({ id, newStatus }: { id: string; newStatus: ProjectStatus }) =>
      transitionProject(id, newStatus),
    onSuccess: (_, { id }) => {
      queryClient.invalidateQueries({ queryKey: projectKey(id) })
      queryClient.invalidateQueries({ queryKey: PROJECTS_KEY })
    },
  })
}

export function useMutateUpdateProjectDescription() {
  return useMutation({
    mutationFn: ({ id, description }: { id: string; description: string }) =>
      updateProjectDescription(id, description),
    onSuccess: (_, { id }) => {
      queryClient.invalidateQueries({ queryKey: projectKey(id) })
      queryClient.invalidateQueries({ queryKey: PROJECTS_KEY })
    },
  })
}
