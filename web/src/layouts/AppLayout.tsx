import { useState } from 'react'
import { NavLink, Outlet } from 'react-router-dom'
import { FolderKanban, Zap, Bot, History, ChevronLeft, ChevronRight } from 'lucide-react'
import { cn } from '@/lib/utils'
import { useAppStore } from '@/store/appStore'
import { ToastContainer } from '@/components/ui/ToastContainer'

const navItems = [
  { to: '/projects', icon: FolderKanban, label: 'Projects' },
  { to: '/sprint', icon: Zap, label: 'Active Sprint' },
  { to: '/agents', icon: Bot, label: 'Agent Monitor' },
  { to: '/history', icon: History, label: 'History' },
]

export default function AppLayout() {
  const [collapsed, setCollapsed] = useState(false)
  const connectionStatus = useAppStore((s) => s.status)

  return (
    <div className="flex h-screen bg-surface overflow-hidden">
      {/* Sidebar */}
      <aside
        className={cn(
          'flex flex-col border-r border-surface-border bg-surface-card transition-all duration-200',
          collapsed ? 'w-16' : 'w-60'
        )}
      >
        {/* Logo area */}
        <div className="flex items-center justify-between px-4 py-4 border-b border-surface-border">
          {!collapsed && (
            <span className="text-forge-500 font-bold text-lg tracking-tight">ChaosForge</span>
          )}
          <button
            onClick={() => setCollapsed((c) => !c)}
            className="text-gray-400 hover:text-white transition-colors ml-auto"
            aria-label={collapsed ? 'Expand sidebar' : 'Collapse sidebar'}
          >
            {collapsed ? <ChevronRight size={18} /> : <ChevronLeft size={18} />}
          </button>
        </div>

        {/* Navigation */}
        <nav className="flex-1 py-4 space-y-1 px-2">
          {navItems.map(({ to, icon: Icon, label }) => (
            <NavLink
              key={to}
              to={to}
              className={({ isActive }) =>
                cn(
                  'flex items-center gap-3 rounded-md px-2 py-2 text-sm font-medium transition-colors',
                  isActive
                    ? 'bg-forge-500/10 text-forge-500'
                    : 'text-gray-400 hover:bg-surface-hover hover:text-white'
                )
              }
            >
              <Icon size={18} className="shrink-0" />
              {!collapsed && <span>{label}</span>}
            </NavLink>
          ))}
        </nav>
      </aside>

      {/* Main area */}
      <div className="flex flex-col flex-1 min-w-0">
        {/* Top bar */}
        <header className="flex items-center justify-between px-6 py-3 border-b border-surface-border bg-surface-card shrink-0">
          <div className="text-gray-400 text-sm">ChaosForge</div>
          <div className="flex items-center gap-2 text-sm text-gray-400">
            <span
              className={cn('h-2 w-2 rounded-full', {
                'bg-status-done': connectionStatus === 'connected',
                'bg-status-idle': connectionStatus === 'disconnected',
                'bg-status-pending animate-pulse': connectionStatus === 'connecting',
              })}
            />
            <span className="capitalize">{connectionStatus}</span>
          </div>
        </header>

        {/* Content */}
        <main className="flex-1 overflow-auto p-6">
          <Outlet />
        </main>
      </div>
      <ToastContainer />
    </div>
  )
}
