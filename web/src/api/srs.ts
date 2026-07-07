import api from '@/lib/api'
import type { SRSDto } from './types'

export interface CreateSRSData {
  ursId: string
  title: string
  technicalDescription: string
}

export interface ApplyHumanEditToSRSData {
  editedDescription: string
  note: string
}

export async function getSRSsByURS(ursId: string): Promise<SRSDto[]> {
  const { data } = await api.get<SRSDto[]>(`/api/srs/by-urs/${ursId}`)
  return data
}

export async function getSRS(id: string): Promise<SRSDto> {
  const { data } = await api.get<SRSDto>(`/api/srs/${id}`)
  return data
}

export async function createSRS(input: CreateSRSData): Promise<SRSDto> {
  const { data } = await api.post<SRSDto>('/api/srs', input)
  return data
}

export async function applyHumanEditToSRS(id: string, input: ApplyHumanEditToSRSData): Promise<void> {
  await api.patch(`/api/srs/${id}/human-edit`, input)
}
