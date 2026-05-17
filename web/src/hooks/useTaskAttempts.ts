import { useQuery } from '@tanstack/react-query'
import { getTaskAttempt, getTaskAttemptsByTask } from '@/api/taskattempts'

const taskAttemptsByTaskKey = (workTaskId: string) =>
  ['task-attempts', 'by-task', workTaskId] as const
const taskAttemptKey = (id: string) => ['task-attempts', id] as const

export function useTaskAttemptsByTask(workTaskId: string, enabled = true) {
  return useQuery({
    queryKey: taskAttemptsByTaskKey(workTaskId),
    queryFn: () => getTaskAttemptsByTask(workTaskId),
    enabled,
  })
}

export function useTaskAttempt(id: string, enabled = true) {
  return useQuery({
    queryKey: taskAttemptKey(id),
    queryFn: () => getTaskAttempt(id),
    enabled,
  })
}
