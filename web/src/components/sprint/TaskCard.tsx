import { useState, useRef, useEffect } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { Badge } from '@/components/ui/badge'
import { cn } from '@/lib/utils'
import type { WorkTaskDto, AttemptType } from '@/api/types'
import { useTaskAttemptsByTask } from '@/hooks/useTaskAttempts'
import { useMutateRejectTask } from '@/hooks/useWorkTasks'
import { useAppStore } from '@/store/appStore'
import { RejectTaskDialog } from './RejectTaskDialog'

interface TaskCardProps {
  task: WorkTaskDto
}

const attemptTypeBadgeClass: Record<AttemptType, string> = {
  Implementation: 'bg-forge-500/20 text-forge-500 border-transparent',
  Review: 'bg-purple-500/20 text-purple-400 border-transparent',
  Testing: 'bg-amber-500/20 text-amber-400 border-transparent',
  Documentation: 'bg-teal-500/20 text-teal-400 border-transparent',
}

export function TaskCard({ task }: TaskCardProps) {
  const { id: projectId } = useParams<{ id: string }>()
  const navigate = useNavigate()
  const push = useAppStore((s) => s.push)

  const [menuOpen, setMenuOpen] = useState(false)
  const [rejectOpen, setRejectOpen] = useState(false)
  const menuRef = useRef<HTMLDivElement>(null)

  const { data: attempts } = useTaskAttemptsByTask(task.id)
  const latestAttempt = attempts && attempts.length > 0 ? attempts[attempts.length - 1] : null

  const { mutate: rejectTask, isPending } = useMutateRejectTask()

  const canReject = task.status === 'InReview' || task.status === 'InTesting'

  useEffect(() => {
    if (!menuOpen) return
    function handleClick(e: MouseEvent) {
      if (menuRef.current && !menuRef.current.contains(e.target as Node)) {
        setMenuOpen(false)
      }
    }
    document.addEventListener('mousedown', handleClick)
    return () => document.removeEventListener('mousedown', handleClick)
  }, [menuOpen])

  function handleViewAttempts() {
    setMenuOpen(false)
    navigate(`/projects/${projectId}/history?task=${task.id}`)
  }

  function handleRejectClick() {
    setMenuOpen(false)
    setRejectOpen(true)
  }

  function handleRejectConfirm() {
    rejectTask(
      { id: task.id },
      {
        onSuccess: () => {
          setRejectOpen(false)
          push({ message: 'Task rejected and moved to Backlog.', variant: 'info' })
        },
      }
    )
  }

  return (
    <>
      <div className="rounded-lg border border-surface-border bg-surface-card p-3 flex flex-col gap-2 hover:border-forge-500/50 transition-colors">
        <div className="flex items-start justify-between gap-2">
          <Badge className="text-xs shrink-0">{task.storyPoints} pt</Badge>
          <div className="relative" ref={menuRef}>
            <button
              onClick={() => setMenuOpen((v) => !v)}
              className="text-gray-500 hover:text-white transition-colors p-0.5 rounded"
              aria-label="Task menu"
            >
              ⋮
            </button>
            {menuOpen && (
              <div className="absolute right-0 top-6 z-20 min-w-[160px] rounded-md border border-surface-border bg-surface-card shadow-lg py-1">
                <button
                  onClick={handleViewAttempts}
                  className="w-full text-left px-3 py-1.5 text-sm text-gray-300 hover:bg-surface-hover hover:text-white transition-colors"
                >
                  View attempts →
                </button>
                {canReject && (
                  <button
                    onClick={handleRejectClick}
                    className="w-full text-left px-3 py-1.5 text-sm text-status-rejected hover:bg-surface-hover transition-colors"
                  >
                    Reject task
                  </button>
                )}
              </div>
            )}
          </div>
        </div>

        <p className="text-sm font-semibold text-white leading-snug">{task.title}</p>
        <p className="text-xs text-gray-400 line-clamp-2 leading-relaxed">{task.description}</p>

        {latestAttempt && (
          <Badge className={cn('text-xs self-start', attemptTypeBadgeClass[latestAttempt.type])}>
            {latestAttempt.type}
          </Badge>
        )}
      </div>

      <RejectTaskDialog
        open={rejectOpen}
        onClose={() => setRejectOpen(false)}
        onConfirm={handleRejectConfirm}
        isPending={isPending}
      />
    </>
  )
}
