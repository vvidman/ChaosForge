import { createBrowserRouter, Navigate } from 'react-router-dom'
import AppLayout from '@/layouts/AppLayout'
import ProjectDetailLayout from '@/layouts/ProjectDetailLayout'
import ProjectListPage from '@/pages/ProjectListPage'
import OverviewPage from '@/pages/project/OverviewPage'
import RequirementsPage from '@/pages/project/RequirementsPage'
import SprintPage from '@/pages/project/SprintPage'
import AgentsPage from '@/pages/project/AgentsPage'
import HistoryPage from '@/pages/project/HistoryPage'
import GatePage from '@/pages/project/GatePage'
import { ErrorBoundary } from '@/components/error/ErrorBoundary'

export const router = createBrowserRouter([
  {
    path: '/',
    element: <AppLayout />,
    children: [
      { index: true, element: <Navigate to="/projects" replace /> },
      {
        path: 'projects',
        element: <ErrorBoundary><ProjectListPage /></ErrorBoundary>,
      },
      {
        path: 'projects/:id',
        element: <ErrorBoundary><ProjectDetailLayout /></ErrorBoundary>,
        children: [
          { index: true, element: <Navigate to="overview" replace /> },
          { path: 'overview', element: <ErrorBoundary><OverviewPage /></ErrorBoundary> },
          { path: 'requirements', element: <ErrorBoundary><RequirementsPage /></ErrorBoundary> },
          { path: 'sprint', element: <ErrorBoundary><SprintPage /></ErrorBoundary> },
          { path: 'agents', element: <ErrorBoundary><AgentsPage /></ErrorBoundary> },
          { path: 'history', element: <ErrorBoundary><HistoryPage /></ErrorBoundary> },
        ],
      },
    ],
  },
  {
    path: '/projects/:id/gate',
    element: <ErrorBoundary><GatePage /></ErrorBoundary>,
  },
])
