export type ProjectStatus =
  | 'Setup'
  | 'RequirementsPhase'
  | 'ArchitecturePhase'
  | 'SprintPlanning'
  | 'Development'
  | 'Completed'

export type WorkTaskStatus =
  | 'Backlog'
  | 'InProgress'
  | 'InReview'
  | 'InTesting'
  | 'InDocumentation'
  | 'Done'

export type AgentRole =
  | 'BusinessAnalyst'
  | 'Architect'
  | 'ScrumMaster'
  | 'Developer'
  | 'Tester'
  | 'Reviewer'
  | 'TechnicalWriter'

export type AgentInstanceStatus = 'Idle' | 'Working' | 'Blocked' | 'Finished'

export type RevisionGateType = 'Requirements' | 'Architecture' | 'SprintPlanning'

export type RevisionGateStatus = 'Open' | 'Resolved'

export type RevisionGateAction = 'Accept' | 'EditAndAccept' | 'Reject'

export type AttemptType = 'Implementation' | 'Review' | 'Testing' | 'Documentation'

export type AttemptResult = 'Pending' | 'Approved' | 'Rejected'

export interface ProjectSummaryDto {
  readonly id: string
  readonly name: string
  readonly description: string
  readonly status: ProjectStatus
  readonly deadline: string | null
  readonly createdAt: string
}

export interface ProjectDto {
  readonly id: string
  readonly name: string
  readonly description: string
  readonly status: ProjectStatus
  readonly deadline: string | null
  readonly createdAt: string
}

export interface UseCaseDto {
  readonly id: string
  readonly projectId: string
  readonly title: string
  readonly description: string
  readonly priority: number
  readonly createdAt: string
}

export interface URSDto {
  readonly id: string
  readonly useCaseId: string
  readonly title: string
  readonly description: string
  readonly humanEditNote: string | null
  readonly createdAt: string
}

export interface SRSDto {
  readonly id: string
  readonly ursId: string
  readonly title: string
  readonly technicalDescription: string
  readonly humanEditNote: string | null
  readonly createdAt: string
}

export interface WorkTaskDto {
  readonly id: string
  readonly srsId: string
  readonly sprintId: string | null
  readonly title: string
  readonly description: string
  readonly status: WorkTaskStatus
  readonly storyPoints: number
  readonly createdAt: string
}

export interface AgentInstanceDto {
  readonly id: string
  readonly projectId: string
  readonly role: AgentRole
  readonly personaName: string
  readonly status: AgentInstanceStatus
  readonly currentTaskId: string | null
  readonly createdAt: string
}

export interface AgentSlotDto {
  readonly id: string
  readonly projectId: string
  readonly role: AgentRole
  readonly count: number
  readonly createdAt: string
}

export interface RevisionGateDto {
  readonly id: string
  readonly projectId: string
  readonly type: RevisionGateType
  readonly status: RevisionGateStatus
  readonly agentOutput: string
  readonly humanEditedOutput: string | null
  readonly rejectionReason: string | null
  readonly action: RevisionGateAction | null
  readonly resolvedAt: string | null
  readonly createdAt: string
}

export interface TaskAttemptDto {
  readonly id: string
  readonly workTaskId: string
  readonly agentInstanceId: string
  readonly type: AttemptType
  readonly output: string
  readonly reviewNote: string | null
  readonly testNote: string | null
  readonly result: AttemptResult
  readonly startedAt: string
  readonly completedAt: string | null
  readonly createdAt: string
}
