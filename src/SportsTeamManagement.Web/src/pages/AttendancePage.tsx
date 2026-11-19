import { useState } from 'react'
import { useMutation } from '@tanstack/react-query'
import { Check } from 'lucide-react'
import { apiRequest } from '../api/client'

export function AttendancePage() {
  const [sessionId, setSessionId] = useState('')
  const [date, setDate] = useState('')
  const [rows, setRows] = useState([{ studentId: '', isPresent: true, notes: '' }])
  const save = useMutation({
    mutationFn: () => apiRequest('/api/attendance/bulk', {
      method: 'POST',
      body: JSON.stringify({ trainingSessionId: sessionId, attendanceDate: date, students: rows.filter((row) => row.studentId) }),
    }),
  })

  return (
    <div className="page">
      <div className="page-heading"><div><div className="eyebrow">Operations</div><h1>Mark attendance</h1></div></div>
      <section className="panel">
        <div className="attendance-controls">
          <label>Training session ID<input value={sessionId} onChange={(e) => setSessionId(e.target.value)} placeholder="GUID" /></label>
          <label>Attendance date<input type="date" value={date} onChange={(e) => setDate(e.target.value)} /></label>
        </div>
        <div className="attendance-list">
          {rows.map((row, index) => (
            <div className="attendance-row" key={index}>
              <input value={row.studentId} onChange={(e) => setRows((current) => current.map((item, i) => i === index ? { ...item, studentId: e.target.value } : item))} placeholder="Student GUID" />
              <button type="button" className={row.isPresent ? 'presence-button active' : 'presence-button'} onClick={() => setRows((current) => current.map((item, i) => i === index ? { ...item, isPresent: !item.isPresent } : item))}><Check size={16} />{row.isPresent ? 'Present' : 'Absent'}</button>
            </div>
          ))}
        </div>
        <div className="form-actions">
          <button className="secondary-button" onClick={() => setRows((current) => [...current, { studentId: '', isPresent: true, notes: '' }])}>Add student</button>
          <button className="primary-button" disabled={!sessionId || save.isPending} onClick={() => save.mutate()}>Save attendance</button>
        </div>
        {save.isSuccess && <div className="success-state">Attendance saved.</div>}
        {save.isError && <div className="error-state">{(save.error as { message?: string }).message || 'Unable to save attendance.'}</div>}
      </section>
    </div>
  )
}
