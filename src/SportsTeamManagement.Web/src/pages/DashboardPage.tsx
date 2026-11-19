import { useQuery } from '@tanstack/react-query'
import { CalendarCheck, Dumbbell, TrendingUp, Users } from 'lucide-react'
import { apiRequest } from '../api/client'

type Club = { id: string; name: string; beltGradingEnabled: boolean }
type AttendanceStats = { studentId: string; totalSessions: number; attendedSessions: number; attendancePercentage: number }

export function DashboardPage() {
  const clubs = useQuery({ queryKey: ['clubs'], queryFn: () => apiRequest<Club[]>('/api/clubs') })
  const stats = useQuery({ queryKey: ['attendance-stats'], queryFn: () => apiRequest<AttendanceStats[]>('/api/attendance/stats') })

  const students = stats.data?.length ?? 0
  const average = stats.data?.length
    ? stats.data.reduce((sum, item) => sum + item.attendancePercentage, 0) / stats.data.length
    : 0

  return (
    <div className="page">
      <div className="page-heading">
        <div><div className="eyebrow">Today</div><h1>Good to see you</h1></div>
      </div>
      {clubs.isError && <div className="error-state">{(clubs.error as { message?: string }).message || 'Unable to load club data.'}</div>}
      <div className="metric-grid">
        <Metric icon={Users} label="Students tracked" value={String(students)} />
        <Metric icon={Dumbbell} label="Active clubs" value={String(clubs.data?.length ?? 0)} />
        <Metric icon={CalendarCheck} label="Average attendance" value={`${average.toFixed(1)}%`} />
        <Metric icon={TrendingUp} label="Training activity" value="Live" />
      </div>
      <section className="panel">
        <div className="panel-heading"><div><div className="eyebrow">Overview</div><h2>Clubs</h2></div></div>
        {clubs.isLoading ? <div className="loading-state">Loading clubs…</div> : (
          <div className="club-list">
            {clubs.data?.map((club) => (
              <button key={club.id} className="club-row" onClick={() => localStorage.setItem('sports_active_club', club.id)}>
                <span className="club-avatar">{club.name.slice(0, 2).toUpperCase()}</span>
                <span><strong>{club.name}</strong><small>{club.beltGradingEnabled ? 'Martial arts grading enabled' : 'General sports club'}</small></span>
                <span className="status-pill">Active</span>
              </button>
            ))}
            {!clubs.data?.length && <div className="empty-state">No clubs have been configured yet.</div>}
          </div>
        )}
      </section>
    </div>
  )
}

function Metric({ icon: Icon, label, value }: { icon: typeof Users; label: string; value: string }) {
  return <div className="metric-card"><div className="metric-icon"><Icon size={20} /></div><div><span>{label}</span><strong>{value}</strong></div></div>
}
