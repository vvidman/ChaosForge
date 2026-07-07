import { useState } from 'react'
import ReactMarkdown from 'react-markdown'
import remarkGfm from 'remark-gfm'
import { Prism as SyntaxHighlighter } from 'react-syntax-highlighter'
import { oneDark } from 'react-syntax-highlighter/dist/esm/styles/prism'
import { Copy, Check } from 'lucide-react'
import { cn } from '@/lib/utils'

interface AgentOutputViewerProps {
  agentOutput: string
}

export default function AgentOutputViewer({ agentOutput }: AgentOutputViewerProps) {
  const [copied, setCopied] = useState(false)

  async function handleCopy() {
    await navigator.clipboard.writeText(agentOutput)
    setCopied(true)
    setTimeout(() => setCopied(false), 2000)
  }

  return (
    <div className="relative overflow-y-auto max-h-[70vh] rounded-lg border border-surface-border bg-surface-card p-4">
      <button
        onClick={handleCopy}
        className={cn(
          'absolute top-3 right-3 z-10 p-1.5 rounded-md transition-colors',
          'bg-surface-hover hover:bg-surface-border text-gray-400 hover:text-white'
        )}
        aria-label="Copy to clipboard"
      >
        {copied ? <Check className="h-4 w-4 text-green-400" /> : <Copy className="h-4 w-4" />}
      </button>
      <div className="prose prose-invert max-w-none pr-8">
        <ReactMarkdown
          remarkPlugins={[remarkGfm]}
          components={{
            code({ className, children, ...props }) {
              const match = /language-(\w+)/.exec(className ?? '')
              const isInline = !match
              return isInline ? (
                <code className={cn('bg-surface-hover px-1 rounded text-sm', className)} {...props}>
                  {children}
                </code>
              ) : (
                <SyntaxHighlighter
                  style={oneDark}
                  language={match[1]}
                  PreTag="div"
                >
                  {String(children).replace(/\n$/, '')}
                </SyntaxHighlighter>
              )
            },
          }}
        >
          {agentOutput}
        </ReactMarkdown>
      </div>
    </div>
  )
}
