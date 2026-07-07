import api from '@/lib/api'
import type { URSDto } from './types'

export interface CreateURSData {
  useCaseId: string
  title: string
  description: string
}

export interface ApplyHumanEditToURSData {
  editedDescription: string
  note: string
}

export async function getURSsByUseCase(useCaseId: string): Promise<URSDto[]> {
  const { data } = await api.get<URSDto[]>(`/api/urs/by-usecase/${useCaseId}`)
  return data
}

export async function getURS(id: string): Promise<URSDto> {
  const { data } = await api.get<URSDto>(`/api/urs/${id}`)
  return data
}

export async function createURS(input: CreateURSData): Promise<URSDto> {
  const { data } = await api.post<URSDto>('/api/urs', input)
  return data
}

export async function applyHumanEditToURS(id: string, input: ApplyHumanEditToURSData): Promise<void> {
  await api.patch(`/api/urs/${id}/human-edit`, input)
}
