import { useState } from 'react'
import { useMutateUpdateProjectDescription } from '@/hooks/useProjects'
import { useAppStore } from '@/store/appStore'
import { Button } from '@/components/ui/button'

interface InlineEditDescriptionProps {
  projectId: string
  description: string
}

export function InlineEditDescription({ projectId, description }: InlineEditDescriptionProps) {
  const [editing, setEditing] = useState(false)
  const [draft, setDraft] = useState(description)
  const push = useAppStore((s) => s.push)
  const { mutate, isPending } = useMutateUpdateProjectDescription()

  function handleEdit() {
    setDraft(description)
    setEditing(true)
  }

  function handleCancel() {
    setEditing(false)
  }

  function handleSave() {
    mutate(
      { id: projectId, description: draft },
      {
        onSuccess: () => {
          push({ message: 'Description updated', variant: 'success' })
          setEditing(false)
        },
        onError: () => {
          push({ message: 'Failed to update description', variant: 'error' })
        },
      }
    )
  }

  if (!editing) {
    return (
      <p
        className="text-sm text-gray-400 cursor-pointer hover:text-gray-300 transition-colors"
        onClick={handleEdit}
        title="Click to edit"
      >
        {description || <span className="italic">No description — click to add</span>}
      </p>
    )
  }

  return (
    <div className="flex flex-col gap-2">
      <textarea
        className="w-full rounded-md border border-surface-border bg-surface-card px-3 py-2 text-sm text-white placeholder-gray-500 focus:outline-none focus:ring-1 focus:ring-forge-500 resize-none"
        rows={3}
        value={draft}
        onChange={(e) => setDraft(e.target.value)}
        autoFocus
      />
      <div className="flex gap-2">
        <Button size="sm" onClick={handleSave} disabled={isPending}>
          {isPending ? 'Saving…' : 'Save'}
        </Button>
        <Button size="sm" variant="outline" onClick={handleCancel} disabled={isPending}>
          Cancel
        </Button>
      </div>
    </div>
  )
}
