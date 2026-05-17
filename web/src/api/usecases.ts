import api from '@/lib/api'
import type { UseCaseDto } from './types'

export interface CreateUseCaseData {
  projectId: string
  title: string
  description: string
  priority: number
}

export async function getUseCasesByProject(projectId: string): Promise<UseCaseDto[]> {
  const { data } = await api.get<UseCaseDto[]>(`/api/usecases/by-project/${projectId}`)
  return data
}

export async function getUseCase(id: string): Promise<UseCaseDto> {
  const { data } = await api.get<UseCaseDto>(`/api/usecases/${id}`)
  return data
}

export async function createUseCase(input: CreateUseCaseData): Promise<UseCaseDto> {
  const { data } = await api.post<UseCaseDto>('/api/usecases', input)
  return data
}

export async function updateUseCasePriority(id: string, priority: number): Promise<void> {
  await api.patch(`/api/usecases/${id}/priority`, { priority })
}
