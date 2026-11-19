import { useState } from 'react'
import { useQuery } from '@tanstack/react-query'
import { Search } from 'lucide-react'
import { apiRequest } from '../api/client'

type Student = { id: string; fullName?: string; user?: { fullName?: string }; status: number; currentBelt: number }

export function StudentsPage() {
  const [status, setStatus] = useState('Active')
  const [search, setSearch] = useState('')
  const query = useQuery({
    queryKey: ['students', status],
    queryFn: () => apiRequest<Student[]>(`/api/students/status/${status}`),
  })
  const students = query.data?.filter((student) => {
    const name = student.fullName || student.user?.fullName || ''
    return name.toLowerCase().includes(search.toLowerCase())
  }) ?? []

  return (
    <div className="page">
      <div className="page-heading"><div><div className="eyebrow">People</div><h1>Students</h1></div></div>
      <section className="panel">
        <div className="table-toolbar">
          <div className="search-field"><Search size={16} /><input value={search} onChange={(e) => setSearch(e.target.value)} placeholder="Search students" /></div>
          <select value={status} onChange={(e) => setStatus(e.target.value)}>
            <option>Active</option><option>PendingApproval</option><option>Suspended</option><option>Inactive</option>
          </select>
        </div>
        {query.isLoading && <div className="loading-state">Loading students…</div>}
        {query.isError && <div className="error-state">{(query.error as { message?: string }).message || 'Unable to load students.'}</div>}
        {!query.isLoading && !query.isError && (
          <table><thead><tr><th>Student</th><th>Status</th><th>Belt</th></tr></thead>
            <tbody>{students.map((student) => <tr key={student.id}><td><strong>{student.fullName || student.user?.fullName || 'Student'}</strong></td><td><span className="status-pill">{status}</span></td><td>{student.currentBelt}</td></tr>)}</tbody>
          </table>
        )}
      </section>
    </div>
  )
}
