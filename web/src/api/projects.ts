import api from '@/lib/api'
import type { ProjectDto, ProjectSummaryDto, ProjectStatus } from './types'

export interface CreateProjectData {
  name: string
  description: string
  deadline?: string
}

export async function getProjects(): Promise<ProjectSummaryDto[]> {
  const { data } = await api.get<ProjectSummaryDto[]>('/api/projects')
  return data
}

export async function getProject(id: string): Promise<ProjectDto> {
  const { data } = await api.get<ProjectDto>(`/api/projects/${id}`)
  return data
}

export async function createProject(input: CreateProjectData): Promise<ProjectDto> {
  const { data } = await api.post<ProjectDto>('/api/projects', input)
  return data
}

export async function transitionProject(id: string, newStatus: ProjectStatus): Promise<void> {
  await api.post(`/api/projects/${id}/transition`, { newStatus })
}

export async function updateProjectDescription(id: string, description: string): Promise<void> {
  await api.patch(`/api/projects/${id}/description`, { description })
}
