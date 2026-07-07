import api from '@/lib/api'
import type { TaskAttemptDto } from './types'

export async function getTaskAttemptsByTask(workTaskId: string): Promise<TaskAttemptDto[]> {
  const { data } = await api.get<TaskAttemptDto[]>(`/api/task-attempts/by-task/${workTaskId}`)
  return data
}

export async function getTaskAttempt(id: string): Promise<TaskAttemptDto> {
  const { data } = await api.get<TaskAttemptDto>(`/api/task-attempts/${id}`)
  return data
}
