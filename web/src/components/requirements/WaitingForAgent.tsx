import { Loader2 } from 'lucide-react'

interface WaitingForAgentProps {
  label: string
}

export default function WaitingForAgent({ label }: WaitingForAgentProps) {
  return (
    <div className="flex items-center gap-2 text-sm text-gray-400 p-3">
      <Loader2 className="w-4 h-4 animate-spin text-forge-500" />
      <span>{label}</span>
    </div>
  )
}
