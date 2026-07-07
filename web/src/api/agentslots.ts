import api from '@/lib/api'
import type { AgentSlotDto, AgentRole } from './types'

export interface CreateAgentSlotData {
  projectId: string
  role: AgentRole
  count: number
}

export async function getAgentSlotsByProject(projectId: string): Promise<AgentSlotDto[]> {
  const { data } = await api.get<AgentSlotDto[]>(`/api/agent-slots/by-project/${projectId}`)
  return data
}

export async function createAgentSlot(input: CreateAgentSlotData): Promise<AgentSlotDto> {
  const { data } = await api.post<AgentSlotDto>('/api/agent-slots', input)
  return data
}

export async function updateAgentSlotCount(id: string, count: number): Promise<void> {
  await api.patch(`/api/agent-slots/${id}/count`, { count })
}
