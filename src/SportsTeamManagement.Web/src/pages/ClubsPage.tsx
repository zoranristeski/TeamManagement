import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useForm } from 'react-hook-form'
import { apiRequest } from '../api/client'

type Club = { id: string; name: string; slug: string; location?: string; beltGradingEnabled: boolean }
type ClubForm = { name: string; slug: string; location: string; beltGradingEnabled: boolean }

export function ClubsPage() {
  const queryClient = useQueryClient()
  const query = useQuery({ queryKey: ['clubs'], queryFn: () => apiRequest<Club[]>('/api/clubs') })
  const form = useForm<ClubForm>({ defaultValues: { name: '', slug: '', location: '', beltGradingEnabled: false } })
  const create = useMutation({
    mutationFn: (dto: ClubForm) => apiRequest('/api/clubs', { method: 'POST', body: JSON.stringify(dto) }),
    onSuccess: () => { form.reset(); queryClient.invalidateQueries({ queryKey: ['clubs'] }) },
  })

  return (
    <div className="page">
      <div className="page-heading"><div><div className="eyebrow">Administration</div><h1>Clubs</h1></div></div>
      <div className="settings-grid">
        <section className="panel">
          <div className="panel-heading"><h2>Configured clubs</h2></div>
          {query.isLoading && <div className="loading-state">Loading clubs…</div>}
          {query.isError && <div className="error-state">{(query.error as { message?: string }).message || 'Unable to load clubs.'}</div>}
          {query.data?.map((club) => (
            <button className="club-row" key={club.id} onClick={() => localStorage.setItem('sports_active_club', club.id)}>
              <span className="club-avatar">{club.name.slice(0, 2).toUpperCase()}</span>
              <span><strong>{club.name}</strong><small>{club.location || club.slug}</small></span>
              <span className="status-pill">Active</span>
            </button>
          ))}
        </section>
        <section className="panel">
          <div className="panel-heading"><h2>Create club</h2></div>
          <form className="settings-form" onSubmit={form.handleSubmit((dto) => create.mutate(dto))}>
            <label>Club name<input {...form.register('name', { required: true })} /></label>
            <label>Slug<input {...form.register('slug', { required: true })} /></label>
            <label>Location<input {...form.register('location')} /></label>
            <label className="checkbox-row"><input type="checkbox" {...form.register('beltGradingEnabled')} /> Martial arts grading enabled</label>
            <button className="primary-button" type="submit" disabled={create.isPending}>Create club</button>
            {create.isError && <div className="error-state">{(create.error as { message?: string }).message || 'Unable to create club.'}</div>}
            {create.isSuccess && <div className="success-state">Club created.</div>}
          </form>
        </section>
      </div>
    </div>
  )
}
