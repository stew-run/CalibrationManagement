import { Navigate, Route, Routes } from 'react-router-dom';
import ProtectedRoute from '../auth/ProtectedRoute';
import RoleGuard from '../auth/RoleGuard';
import CertificatesPage from '../features/certificates/pages/CertificatesPage';
import EquipmentPage from '../features/equipment/pages/EquipmentPage';
import HistoryPage from '../features/history/pages/HistoryPage';
import MaintenancePage from '../features/maintenance/pages/MaintenancePage';
import NotificationsPage from '../features/notifications/pages/NotificationsPage';
import ReportsPage from '../features/reports/pages/ReportsPage';
import ResultsPage from '../features/results/pages/ResultsPage';
import SchedulesPage from '../features/schedules/pages/SchedulesPage';
import ProfilePage from '../features/users/pages/ProfilePage';
import UsersPage from '../features/users/pages/UsersPage';
import AuthLayout from '../layouts/AuthLayout';
import MainLayout from '../layouts/MainLayout';
import DashboardPage from '../pages/DashboardPage';
import LoginPage from '../pages/LoginPage';

export default function AppRoutes() {
  return (
    <Routes>
      <Route element={<AuthLayout />}>
        <Route path="/login" element={<LoginPage />} />
      </Route>
      <Route element={<ProtectedRoute><MainLayout /></ProtectedRoute>}>
        <Route index element={<DashboardPage />} />
        <Route path="equipment" element={<EquipmentPage />} />
        <Route path="schedules" element={<SchedulesPage />} />
        <Route path="results" element={<ResultsPage />} />
        <Route path="history" element={<HistoryPage />} />
        <Route path="history/:equipmentId" element={<HistoryPage />} />
        <Route path="certificates" element={<CertificatesPage />} />
        <Route path="maintenance" element={<MaintenancePage />} />
        <Route path="reports" element={<ReportsPage />} />
        <Route path="notifications" element={<NotificationsPage />} />
        <Route path="profile" element={<ProfilePage />} />
        <Route path="users" element={<RoleGuard permission="admin"><UsersPage /></RoleGuard>} />
        <Route path="*" element={<Navigate to="/" replace />} />
      </Route>
    </Routes>
  );
}
