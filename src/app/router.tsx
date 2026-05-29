import { createBrowserRouter, Navigate } from 'react-router-dom';
import { AppShell } from '@/components/layout/AppShell';
import { ClaimShell } from '@/components/layout/ClaimShell';
import { RequireAuth } from '@/features/auth/RequireAuth';
import LoginPage from '@/pages/LoginPage';
import DashboardPage from '@/pages/DashboardPage';
import ClaimsListPage from '@/pages/ClaimsListPage';
import ClaimWorkspacePage from '@/pages/ClaimWorkspacePage';
import DocumentsPhotosPage from '@/pages/DocumentsPhotosPage';
import AiEvidencePage from '@/pages/AiEvidencePage';
import RisksChecksPage from '@/pages/RisksChecksPage';
import HumanApprovalPage from '@/pages/HumanApprovalPage';
import AuditCostPage from '@/pages/AuditCostPage';
import PolicyCoveragePage from '@/pages/PolicyCoveragePage';
import CustomerVehiclePage from '@/pages/CustomerVehiclePage';
import DemoScenarioPage from '@/pages/DemoScenarioPage';
import CustomersDirectoryPage from '@/pages/CustomersDirectoryPage';

export const router = createBrowserRouter([
  { path: '/login', element: <LoginPage /> },
  {
    path: '/',
    element: (
      <RequireAuth>
        <AppShell />
      </RequireAuth>
    ),
    children: [
      { index: true, element: <DashboardPage /> },
      { path: 'claims', element: <ClaimsListPage /> },
      {
        path: 'claims/:claimId',
        element: <ClaimShell />,
        children: [
          { index: true, element: <ClaimWorkspacePage /> },
          { path: 'documents', element: <DocumentsPhotosPage /> },
          { path: 'ai-evidence', element: <AiEvidencePage /> },
          { path: 'risks', element: <RisksChecksPage /> },
          { path: 'approval', element: <HumanApprovalPage /> },
          { path: 'audit', element: <AuditCostPage /> },
          { path: 'policy', element: <PolicyCoveragePage /> },
          { path: 'customer-vehicle', element: <CustomerVehiclePage /> },
        ],
      },
      { path: 'customers', element: <CustomersDirectoryPage /> },
      { path: 'demo', element: <DemoScenarioPage /> },
      { path: '*', element: <Navigate to="/" replace /> },
    ],
  },
]);
