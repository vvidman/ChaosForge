import api from '@/lib/api'
import type { WorkTaskDto } from './types'

export interface CreateWorkTaskData {
  srsId: string
  title: string
  description: string
  storyPoints: number
}

export async function getWorkTasksBySRS(srsId: string): Promise<WorkTaskDto[]> {
  const { data } = await api.get<WorkTaskDto[]>(`/api/worktasks/by-srs/${srsId}`)
  return data
}

export async function getWorkTask(id: string): Promise<WorkTaskDto> {
  const { data } = await api.get<WorkTaskDto>(`/api/worktasks/${id}`)
  return data
}

export async function createWorkTask(input: CreateWorkTaskData): Promise<WorkTaskDto> {
  const { data } = await api.post<WorkTaskDto>('/api/worktasks', input)
  return data
}

export async function assignToSprint(id: string, sprintId: string): Promise<void> {
  await api.post(`/api/worktasks/${id}/assign-sprint`, { sprintId })
}

export async function startTask(id: string): Promise<void> {
  await api.post(`/api/worktasks/${id}/start`)
}

export async function sendToReview(id: string): Promise<void> {
  await api.post(`/api/worktasks/${id}/send-to-review`)
}

export async function approveTask(id: string): Promise<void> {
  await api.post(`/api/worktasks/${id}/approve`)
}

export async function passTesting(id: string): Promise<void> {
  await api.post(`/api/worktasks/${id}/pass-testing`)
}

export async function completeTask(id: string): Promise<void> {
  await api.post(`/api/worktasks/${id}/complete`)
}

export async function rejectTask(id: string): Promise<void> {
  await api.post(`/api/worktasks/${id}/reject`)
}
