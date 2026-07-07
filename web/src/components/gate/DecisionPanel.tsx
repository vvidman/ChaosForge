import { useState } from 'react'
import { Button } from '@/components/ui/button'
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog'
import {
  useMutateAcceptGate,
  useMutateEditAndAcceptGate,
  useMutateRejectGate,
} from '@/hooks/useRevisionGates'
import type { RevisionGateDto } from '@/api/types'

type PanelMode = 'none' | 'editAccept' | 'reject'

interface DecisionPanelProps {
  gate: RevisionGateDto
  onSuccess: () => void
}

export default function DecisionPanel({ gate, onSuccess }: DecisionPanelProps) {
  const [mode, setMode] = useState<PanelMode>('none')
  const [editedContent, setEditedContent] = useState(gate.agentOutput)
  const [rejectReason, setRejectReason] = useState('')
  const [acceptDialogOpen, setAcceptDialogOpen] = useState(false)

  const acceptMutation = useMutateAcceptGate()
  const editAcceptMutation = useMutateEditAndAcceptGate()
  const rejectMutation = useMutateRejectGate()

  function toggleMode(next: PanelMode) {
    setMode((prev) => (prev === next ? 'none' : next))
  }

  function handleAcceptConfirm() {
    acceptMutation.mutate(
      { id: gate.id, projectId: gate.projectId },
      {
        onSuccess: () => {
          setAcceptDialogOpen(false)
          onSuccess()
        },
      }
    )
  }

  function handleEditAcceptSubmit() {
    editAcceptMutation.mutate(
      { id: gate.id, projectId: gate.projectId, editedOutput: editedContent },
      { onSuccess }
    )
  }

  function handleRejectSubmit() {
    rejectMutation.mutate(
      { id: gate.id, projectId: gate.projectId, reason: rejectReason },
      { onSuccess }
    )
  }

  return (
    <div className="flex flex-col gap-2">
      <Button
        variant="default"
        className="w-full bg-green-600 hover:bg-green-700 text-white"
        onClick={() => setAcceptDialogOpen(true)}
      >
        Accept
      </Button>

      <Button
        variant="default"
        className="w-full"
        onClick={() => toggleMode('editAccept')}
      >
        Edit &amp; Accept
      </Button>
      {mode === 'editAccept' && (
        <div className="flex flex-col gap-2 p-3 rounded-md border border-surface-border bg-surface-card">
          <textarea
            className="w-full min-h-[200px] rounded-md border border-surface-border bg-surface-hover text-gray-100 p-2 text-sm resize-y focus:outline-none focus:ring-1 focus:ring-forge-500"
            value={editedContent}
            onChange={(e) => setEditedContent(e.target.value)}
          />
          <Button
            variant="default"
            className="w-full"
            disabled={!editedContent.trim() || editAcceptMutation.isPending}
            onClick={handleEditAcceptSubmit}
          >
            {editAcceptMutation.isPending ? 'Submitting…' : 'Submit edit'}
          </Button>
        </div>
      )}

      <Button
        variant="destructive"
        className="w-full"
        onClick={() => toggleMode('reject')}
      >
        Reject
      </Button>
      {mode === 'reject' && (
        <div className="flex flex-col gap-2 p-3 rounded-md border border-surface-border bg-surface-card">
          <textarea
            className="w-full min-h-[120px] rounded-md border border-surface-border bg-surface-hover text-gray-100 p-2 text-sm resize-y focus:outline-none focus:ring-1 focus:ring-forge-500"
            placeholder="Rejection reason (required)"
            value={rejectReason}
            onChange={(e) => setRejectReason(e.target.value)}
          />
          <Button
            variant="destructive"
            className="w-full"
            disabled={!rejectReason.trim() || rejectMutation.isPending}
            onClick={handleRejectSubmit}
          >
            {rejectMutation.isPending ? 'Submitting…' : 'Submit rejection'}
          </Button>
        </div>
      )}

      <Dialog open={acceptDialogOpen} onOpenChange={setAcceptDialogOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Confirm Accept</DialogTitle>
            <DialogDescription>
              This will advance the project to the next phase. Continue?
            </DialogDescription>
          </DialogHeader>
          <DialogFooter>
            <Button variant="outline" onClick={() => setAcceptDialogOpen(false)}>
              Cancel
            </Button>
            <Button
              className="bg-green-600 hover:bg-green-700 text-white"
              disabled={acceptMutation.isPending}
              onClick={handleAcceptConfirm}
            >
              {acceptMutation.isPending ? 'Confirming…' : 'Confirm'}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  )
}
