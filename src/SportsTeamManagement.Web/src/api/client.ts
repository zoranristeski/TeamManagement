const API_BASE_URL = (import.meta.env.VITE_API_BASE_URL as string | undefined)?.replace(/\/$/, '') || 'https://localhost:7001'

export type ApiError = { status: number; message: string }

export async function apiRequest<T>(path: string, init: RequestInit = {}): Promise<T> {
  const token = localStorage.getItem('sports_access_token')
  const clubId = localStorage.getItem('sports_active_club')
  const headers = new Headers(init.headers)
  headers.set('Accept', 'application/json')
  if (init.body) headers.set('Content-Type', 'application/json')
  if (token) headers.set('Authorization', `Bearer ${token}`)
  if (clubId) headers.set('X-Club-Id', clubId)

  const response = await fetch(`${API_BASE_URL}${path}`, { ...init, headers })
  if (!response.ok) {
    const body = await response.text()
    throw { status: response.status, message: body || `Request failed (${response.status})` } satisfies ApiError
  }
  if (response.status === 204) return undefined as T
  return response.json() as Promise<T>
}
