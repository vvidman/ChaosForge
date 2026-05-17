import { type ReactNode } from 'react'
import { Plus, FolderOpen } from 'lucide-react'
import { useProjects } from '@/hooks/useProjects'
import { ProjectCard } from '@/components/projects/ProjectCard'
import { CreateProjectDialog } from '@/components/projects/CreateProjectDialog'
import { Button } from '@/components/ui/button'
import { Skeleton } from '@/components/ui/skeleton'

function SkeletonCard() {
  return (
    <div className="rounded-lg border border-surface-border bg-surface-card p-5 space-y-3">
      <div className="flex justify-between">
        <Skeleton className="h-5 w-24" />
        <Skeleton className="h-4 w-16" />
      </div>
      <Skeleton className="h-5 w-3/4" />
      <Skeleton className="h-4 w-full" />
      <Skeleton className="h-4 w-2/3" />
      <div className="flex justify-between pt-1">
        <Skeleton className="h-3 w-28" />
        <Skeleton className="h-4 w-4" />
      </div>
    </div>
  )
}

function EmptyState({ onNew }: { onNew: ReactNode }) {
  return (
    <div className="flex flex-col items-center justify-center py-24 text-center">
      <FolderOpen size={48} className="text-gray-600 mb-4" />
      <h2 className="text-lg font-semibold text-white mb-2">No projects yet</h2>
      <p className="text-sm text-gray-400 mb-6 max-w-xs">
        Create your first project and let the AI team get to work.
      </p>
      {onNew}
    </div>
  )
}

export default function ProjectListPage() {
  const { data: projects, isLoading, isError } = useProjects()

  const newProjectButton = (
    <Button>
      <Plus size={16} />
      New Project
    </Button>
  )

  const sorted = projects
    ? [...projects].sort((a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime())
    : []

  return (
    <div>
      <div className="flex items-center justify-between mb-6">
        <h1 className="text-2xl font-semibold text-white">Projects</h1>
        <CreateProjectDialog trigger={newProjectButton} />
      </div>

      {isError && (
        <p className="text-sm text-status-blocked">Failed to load projects. Please refresh.</p>
      )}

      {isLoading && (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
          <SkeletonCard />
          <SkeletonCard />
          <SkeletonCard />
        </div>
      )}

      {!isLoading && !isError && sorted.length === 0 && (
        <EmptyState
          onNew={
            <CreateProjectDialog
              trigger={
                <Button>
                  <Plus size={16} />
                  Create your first project
                </Button>
              }
            />
          }
        />
      )}

      {!isLoading && !isError && sorted.length > 0 && (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
          {sorted.map((project) => (
            <ProjectCard key={project.id} project={project} />
          ))}
        </div>
      )}
    </div>
  )
}
