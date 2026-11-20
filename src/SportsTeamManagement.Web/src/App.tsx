import { Navigate, Route, Routes } from 'react-router-dom'
import { useAuth } from './auth/useAuth'
import { AppShell } from './components/AppShell'
import { LoginPage } from './pages/LoginPage'
import { DashboardPage } from './pages/DashboardPage'
import { StudentsPage } from './pages/StudentsPage'
import { GroupsPage } from './pages/GroupsPage'
import { AttendancePage } from './pages/AttendancePage'
import { ClubsPage } from './pages/ClubsPage'
import { AuthCallbackPage } from './pages/AuthCallbackPage'

export default function App() {
  const { token } = useAuth()
  if (!token) {
    return (
      <Routes>
        <Route path="/login" element={<LoginPage />} />
        <Route path="/auth/callback/:provider" element={<AuthCallbackPage />} />
        <Route path="*" element={<Navigate to="/login" replace />} />
      </Routes>
    )
  }

  return (
    <Routes>
      <Route element={<AppShell />}>
        <Route path="/" element={<DashboardPage />} />
        <Route path="/students" element={<StudentsPage />} />
        <Route path="/groups" element={<GroupsPage />} />
        <Route path="/attendance" element={<AttendancePage />} />
        <Route path="/clubs" element={<ClubsPage />} />
      </Route>
      <Route path="*" element={<Navigate to="/" replace />} />
    </Routes>
  )
}
