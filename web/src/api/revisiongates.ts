import api from '@/lib/api'
import type { RevisionGateDto, RevisionGateType } from './types'

export interface OpenRevisionGateData {
  projectId: string
  type: RevisionGateType
  agentOutput: string
}

export async function getRevisionGatesByProject(projectId: string): Promise<RevisionGateDto[]> {
  const { data } = await api.get<RevisionGateDto[]>(`/api/revision-gates/by-project/${projectId}`)
  return data
}

export async function getRevisionGate(id: string): Promise<RevisionGateDto> {
  const { data } = await api.get<RevisionGateDto>(`/api/revision-gates/${id}`)
  return data
}

export async function getOpenRevisionGate(projectId: string): Promise<RevisionGateDto> {
  const { data } = await api.get<RevisionGateDto>(`/api/revision-gates/open/by-project/${projectId}`)
  return data
}

export async function openRevisionGate(input: OpenRevisionGateData): Promise<RevisionGateDto> {
  const { data } = await api.post<RevisionGateDto>('/api/revision-gates', input)
  return data
}

export async function acceptGate(id: string): Promise<void> {
  await api.post(`/api/revision-gates/${id}/accept`)
}

export async function editAndAcceptGate(id: string, editedOutput: string): Promise<void> {
  await api.post(`/api/revision-gates/${id}/edit-and-accept`, { editedOutput })
}

export async function rejectGate(id: string, reason: string): Promise<void> {
  await api.post(`/api/revision-gates/${id}/reject`, { reason })
}
