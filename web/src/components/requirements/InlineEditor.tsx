import { useState } from 'react'
import { Button } from '@/components/ui/button'

interface InlineEditorProps {
  initialDescription: string
  onSave: (description: string, note: string) => void
  onCancel: () => void
  isPending?: boolean
}

export default function InlineEditor({
  initialDescription,
  onSave,
  onCancel,
  isPending = false,
}: InlineEditorProps) {
  const [description, setDescription] = useState(initialDescription)
  const [note, setNote] = useState('')

  function handleSave() {
    onSave(description, note)
  }

  function handleCancel() {
    setDescription(initialDescription)
    setNote('')
    onCancel()
  }

  return (
    <div className="flex flex-col gap-2 p-3 rounded-md border border-surface-border bg-surface-card">
      <textarea
        className="w-full min-h-[120px] rounded-md border border-surface-border bg-surface-hover text-gray-100 p-2 text-sm resize-y focus:outline-none focus:ring-1 focus:ring-forge-500"
        value={description}
        onChange={(e) => setDescription(e.target.value)}
        placeholder="Description"
      />
      <textarea
        className="w-full min-h-[60px] rounded-md border border-surface-border bg-surface-hover text-gray-100 p-2 text-sm resize-y focus:outline-none focus:ring-1 focus:ring-forge-500"
        value={note}
        onChange={(e) => setNote(e.target.value)}
        placeholder="Note (optional)"
      />
      <div className="flex gap-2">
        <Button
          size="sm"
          disabled={!description.trim() || isPending}
          onClick={handleSave}
        >
          {isPending ? 'Saving…' : 'Save'}
        </Button>
        <Button size="sm" variant="outline" onClick={handleCancel}>
          Cancel
        </Button>
      </div>
    </div>
  )
}
