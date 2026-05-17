import { useState, type ReactNode, type FormEvent } from 'react'
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogFooter,
  DialogTrigger,
} from '@/components/ui/dialog'
import { Button } from '@/components/ui/button'
import { useMutateCreateProject } from '@/hooks/useProjects'
import { useAppStore } from '@/store/appStore'
import { cn } from '@/lib/utils'

interface FormValues {
  name: string
  description: string
  deadline: string
}

interface CreateProjectDialogProps {
  trigger: ReactNode
}

export function CreateProjectDialog({ trigger }: CreateProjectDialogProps) {
  const [open, setOpen] = useState(false)
  const [values, setValues] = useState<FormValues>({ name: '', description: '', deadline: '' })
  const [errors, setErrors] = useState<Partial<FormValues>>({})
  const [apiError, setApiError] = useState<string | null>(null)
  const [isSubmitting, setIsSubmitting] = useState(false)
  const createProject = useMutateCreateProject()
  const push = useAppStore((s) => s.push)

  function handleOpenChange(next: boolean) {
    if (!next) {
      setValues({ name: '', description: '', deadline: '' })
      setErrors({})
      setApiError(null)
    }
    setOpen(next)
  }

  function validate(): boolean {
    const next: Partial<FormValues> = {}
    if (!values.name.trim()) next.name = 'Name is required'
    if (!values.description.trim()) next.description = 'Description is required'
    setErrors(next)
    return Object.keys(next).length === 0
  }

  async function onSubmit(e: FormEvent) {
    e.preventDefault()
    if (!validate()) return
    setApiError(null)
    setIsSubmitting(true)
    try {
      await createProject.mutateAsync({
        name: values.name.trim(),
        description: values.description.trim(),
        deadline: values.deadline || undefined,
      })
      push({ message: `Project "${values.name}" created`, variant: 'success' })
      handleOpenChange(false)
    } catch {
      setApiError('Failed to create project. Please try again.')
    } finally {
      setIsSubmitting(false)
    }
  }

  return (
    <Dialog open={open} onOpenChange={handleOpenChange}>
      <DialogTrigger asChild>{trigger}</DialogTrigger>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>New Project</DialogTitle>
        </DialogHeader>

        <form onSubmit={onSubmit} className="space-y-4">
          <div>
            <label className="block text-sm font-medium text-gray-300 mb-1">
              Name <span className="text-status-blocked">*</span>
            </label>
            <input
              value={values.name}
              onChange={(e) => setValues((v) => ({ ...v, name: e.target.value }))}
              className={cn(
                'w-full rounded-md border bg-surface px-3 py-2 text-sm text-white placeholder-gray-500 focus:outline-none focus:ring-1 focus:ring-forge-500',
                errors.name ? 'border-status-blocked' : 'border-surface-border'
              )}
              placeholder="My Awesome Project"
            />
            {errors.name && (
              <p className="mt-1 text-xs text-status-blocked">{errors.name}</p>
            )}
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-300 mb-1">
              Description <span className="text-status-blocked">*</span>
            </label>
            <textarea
              value={values.description}
              onChange={(e) => setValues((v) => ({ ...v, description: e.target.value }))}
              rows={3}
              className={cn(
                'w-full rounded-md border bg-surface px-3 py-2 text-sm text-white placeholder-gray-500 focus:outline-none focus:ring-1 focus:ring-forge-500 resize-none',
                errors.description ? 'border-status-blocked' : 'border-surface-border'
              )}
              placeholder="What will this project build?"
            />
            {errors.description && (
              <p className="mt-1 text-xs text-status-blocked">{errors.description}</p>
            )}
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-300 mb-1">
              Deadline <span className="text-gray-500">(optional)</span>
            </label>
            <input
              type="date"
              value={values.deadline}
              onChange={(e) => setValues((v) => ({ ...v, deadline: e.target.value }))}
              className="w-full rounded-md border border-surface-border bg-surface px-3 py-2 text-sm text-white focus:outline-none focus:ring-1 focus:ring-forge-500"
            />
          </div>

          {apiError && (
            <p className="text-sm text-status-blocked">{apiError}</p>
          )}

          <DialogFooter>
            <Button
              type="button"
              variant="outline"
              onClick={() => handleOpenChange(false)}
              disabled={isSubmitting}
            >
              Cancel
            </Button>
            <Button type="submit" disabled={isSubmitting}>
              {isSubmitting ? 'Creating…' : 'Create Project'}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  )
}
