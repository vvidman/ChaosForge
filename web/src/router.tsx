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

export const router = createBrowserRouter([
  {
    path: '/',
    element: <AppLayout />,
    children: [
      { index: true, element: <Navigate to="/projects" replace /> },
      { path: 'projects', element: <ProjectListPage /> },
      {
        path: 'projects/:id',
        element: <ProjectDetailLayout />,
        children: [
          { index: true, element: <Navigate to="overview" replace /> },
          { path: 'overview', element: <OverviewPage /> },
          { path: 'requirements', element: <RequirementsPage /> },
          { path: 'sprint', element: <SprintPage /> },
          { path: 'agents', element: <AgentsPage /> },
          { path: 'history', element: <HistoryPage /> },
        ],
      },
    ],
  },
  {
    path: '/projects/:id/gate',
    element: <GatePage />,
  },
])
