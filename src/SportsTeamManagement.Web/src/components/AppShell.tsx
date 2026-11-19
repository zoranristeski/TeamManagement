import { NavLink, Outlet } from 'react-router-dom'
import { CalendarCheck, LayoutDashboard, Settings, Shield, Users, LogOut, Dumbbell } from 'lucide-react'
import { useAuth } from '../auth/useAuth'

const links = [
  { to: '/', label: 'Overview', icon: LayoutDashboard },
  { to: '/students', label: 'Students', icon: Users },
  { to: '/groups', label: 'Training groups', icon: Dumbbell },
  { to: '/attendance', label: 'Attendance', icon: CalendarCheck },
  { to: '/clubs', label: 'Club settings', icon: Settings },
]

export function AppShell() {
  const { logout } = useAuth()
  return (
    <div className="app-shell">
      <aside className="sidebar">
        <div className="brand"><span className="brand-mark"><Shield size={18} /></span><span>TeamHub</span></div>
        <nav>
          {links.map(({ to, label, icon: Icon }) => (
            <NavLink key={to} to={to} end={to === '/'}><Icon size={18} /><span>{label}</span></NavLink>
          ))}
        </nav>
      </aside>
      <div className="workspace">
        <header className="topbar">
          <div>
            <div className="eyebrow">Management portal</div>
            <strong>Sports operations</strong>
          </div>
          <div className="topbar-actions">
            <select defaultValue={localStorage.getItem('sports_active_club') || ''} onChange={(event) => {
              localStorage.setItem('sports_active_club', event.target.value)
            }} aria-label="Active club">
              <option value="">Select club</option>
            </select>
            <button className="icon-button" onClick={logout} title="Sign out"><LogOut size={18} /></button>
          </div>
        </header>
        <main><Outlet /></main>
      </div>
    </div>
  )
}
