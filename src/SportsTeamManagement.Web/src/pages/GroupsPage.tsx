import { useState } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { Plus } from 'lucide-react'
import { apiRequest } from '../api/client'

type Group = { id: string; name: string; sportOrDiscipline?: string; capacity: number; location?: string; isActive: boolean }

export function GroupsPage() {
  const clubId = localStorage.getItem('sports_active_club') || ''
  const [showForm, setShowForm] = useState(false)
  const [name, setName] = useState('')
  const [capacity, setCapacity] = useState(20)
  const queryClient = useQueryClient()
  const query = useQuery({
    queryKey: ['groups', clubId],
    enabled: Boolean(clubId),
    queryFn: () => apiRequest<Group[]>(`/api/traininggroups?clubId=${clubId}`),
  })
  const create = useMutation({
    mutationFn: () => apiRequest('/api/traininggroups', { method: 'POST', body: JSON.stringify({ clubId, name, capacity }) }),
    onSuccess: () => { setShowForm(false); setName(''); queryClient.invalidateQueries({ queryKey: ['groups', clubId] }) },
  })

  return (
    <div className="page">
      <div className="page-heading"><div><div className="eyebrow">Operations</div><h1>Training groups</h1></div><button className="primary-button" onClick={() => setShowForm(true)}><Plus size={16} /> New group</button></div>
      {!clubId && <div className="warning-state">Select an active club to manage training groups.</div>}
      {showForm && <form className="panel inline-form" onSubmit={(event) => { event.preventDefault(); create.mutate() }}>
        <label>Group name<input value={name} onChange={(event) => setName(event.target.value)} required /></label>
        <label>Capacity<input type="number" min="1" value={capacity} onChange={(event) => setCapacity(Number(event.target.value))} /></label>
        <button className="primary-button" type="submit" disabled={create.isPending}>Create</button>
      </form>}
      <section className="panel">
        {query.isLoading && <div className="loading-state">Loading groups…</div>}
        {query.isError && <div className="error-state">{(query.error as { message?: string }).message || 'Unable to load groups.'}</div>}
        {!clubId ? null : <div className="card-grid">{query.data?.map((group) => (
          <article className="data-card" key={group.id}><div className="panel-heading"><h2>{group.name}</h2><span className="status-pill">{group.isActive ? 'Active' : 'Inactive'}</span></div><p>{group.sportOrDiscipline || 'General training'}</p><dl><div><dt>Capacity</dt><dd>{group.capacity}</dd></div><div><dt>Location</dt><dd>{group.location || 'Not set'}</dd></div></dl></article>
        ))}</div>}
        {clubId && !query.isLoading && query.data?.length === 0 && <div className="empty-state">No training groups for this club.</div>}
      </section>
    </div>
  )
}
