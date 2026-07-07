import api from '@/lib/api'
import type { AgentInstanceDto, AgentRole } from './types'

export interface CreateAgentInstanceData {
  projectId: string
  role: AgentRole
  personaName: string
}

export async function getAgentInstancesByProject(projectId: string): Promise<AgentInstanceDto[]> {
  const { data } = await api.get<AgentInstanceDto[]>(`/api/agent-instances/by-project/${projectId}`)
  return data
}

export async function getAgentInstance(id: string): Promise<AgentInstanceDto> {
  const { data } = await api.get<AgentInstanceDto>(`/api/agent-instances/${id}`)
  return data
}

export async function createAgentInstance(input: CreateAgentInstanceData): Promise<AgentInstanceDto> {
  const { data } = await api.post<AgentInstanceDto>('/api/agent-instances', input)
  return data
}
