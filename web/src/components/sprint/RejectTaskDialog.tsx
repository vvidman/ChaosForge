import { useState } from 'react'
import {
  Dialog,
  DialogContent,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog'
import { Button } from '@/components/ui/button'

interface RejectTaskDialogProps {
  open: boolean
  onClose: () => void
  onConfirm: () => void
  isPending: boolean
}

export function RejectTaskDialog({ open, onClose, onConfirm, isPending }: RejectTaskDialogProps) {
  const [reason, setReason] = useState('')

  const isValid = reason.trim().length >= 10

  function handleConfirm() {
    if (!isValid) return
    onConfirm()
  }

  function handleOpenChange(next: boolean) {
    if (!next) {
      setReason('')
      onClose()
    }
  }

  return (
    <Dialog open={open} onOpenChange={handleOpenChange}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>Reject Task</DialogTitle>
        </DialogHeader>
        <div className="flex flex-col gap-2">
          <label htmlFor="reject-reason" className="text-sm text-gray-300">
            Reason for rejection
          </label>
          <textarea
            id="reject-reason"
            value={reason}
            onChange={(e) => setReason(e.target.value)}
            rows={4}
            className="w-full rounded-md border border-surface-border bg-surface-hover px-3 py-2 text-sm text-white placeholder-gray-500 focus:outline-none focus:ring-2 focus:ring-forge-500 resize-none"
            placeholder="Describe why this task is being rejected (min 10 characters)..."
          />
          {reason.length > 0 && !isValid && (
            <p className="text-xs text-status-rejected">Reason must be at least 10 characters.</p>
          )}
        </div>
        <DialogFooter>
          <Button variant="ghost" onClick={onClose} disabled={isPending}>
            Cancel
          </Button>
          <Button
            variant="destructive"
            onClick={handleConfirm}
            disabled={!isValid || isPending}
          >
            {isPending ? 'Rejecting…' : 'Reject task'}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  )
}
