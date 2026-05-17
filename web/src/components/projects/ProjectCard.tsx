import { useNavigate } from 'react-router-dom'
import { ChevronRight } from 'lucide-react'
import { Card, CardContent } from '@/components/ui/card'
import { StatusBadge } from '@/components/ui/StatusBadge'
import { getRelativeTimeString } from '@/components/ui/relativeTime'
import type { ProjectSummaryDto } from '@/api/types'

interface ProjectCardProps {
  project: ProjectSummaryDto
}

export function ProjectCard({ project }: ProjectCardProps) {
  const navigate = useNavigate()

  function handleClick() {
    navigate(`/projects/${project.id}/overview`)
  }

  return (
    <Card
      className="cursor-pointer transition-colors hover:border-forge-500/50 hover:bg-surface-hover"
      onClick={handleClick}
    >
      <CardContent className="p-5">
        <div className="flex items-start justify-between mb-3">
          <StatusBadge status={project.status} />
          {project.deadline && (
            <span className="text-xs text-gray-400">
              {new Date(project.deadline).toLocaleDateString()}
            </span>
          )}
        </div>

        <h3 className="font-semibold text-lg text-white mb-1 leading-snug">
          {project.name}
        </h3>
        <p className="text-sm text-gray-400 line-clamp-2 mb-4">
          {project.description}
        </p>

        <div className="flex items-center justify-between">
          <span className="text-xs text-gray-500">
            Created {getRelativeTimeString(project.createdAt)}
          </span>
          <ChevronRight size={16} className="text-gray-500" />
        </div>
      </CardContent>
    </Card>
  )
}
