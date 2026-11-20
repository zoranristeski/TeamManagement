const API_BASE_URL = (import.meta.env.VITE_API_BASE_URL as string | undefined)?.replace(/\/$/, '') || 'https://localhost:7001'

export type ApiError = { status: number; message: string }

export async function apiRequest<T>(path: string, init: RequestInit = {}): Promise<T> {
  const execute = async (): Promise<Response> => {
    const token = localStorage.getItem('sports_access_token')
    const clubId = localStorage.getItem('sports_active_club')
    const headers = new Headers(init.headers)
    headers.set('Accept', 'application/json')
    if (init.body) headers.set('Content-Type', 'application/json')
    if (token) headers.set('Authorization', `Bearer ${token}`)
    if (clubId) headers.set('X-Club-Id', clubId)
    return fetch(`${API_BASE_URL}${path}`, { ...init, headers })
  }

  let response = await execute()
  if (response.status === 401) {
    const refreshToken = localStorage.getItem('sports_refresh_token')
    if (refreshToken) {
      const refreshResponse = await fetch(`${API_BASE_URL}/api/auth/refresh`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json', Accept: 'application/json' },
        body: JSON.stringify({ refreshToken }),
      })
      if (refreshResponse.ok) {
        const refreshed = await refreshResponse.json() as { accessToken: string; refreshToken: string }
        localStorage.setItem('sports_access_token', refreshed.accessToken)
        localStorage.setItem('sports_refresh_token', refreshed.refreshToken)
        response = await execute()
      } else {
        localStorage.removeItem('sports_access_token')
        localStorage.removeItem('sports_refresh_token')
      }
    }
  }
  if (!response.ok) {
    const body = await response.text()
    throw { status: response.status, message: body || `Request failed (${response.status})` } satisfies ApiError
  }
  if (response.status === 204) return undefined as T
  return response.json() as Promise<T>
}
