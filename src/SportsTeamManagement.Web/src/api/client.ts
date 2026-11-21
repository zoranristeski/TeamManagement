const configuredApiBaseUrl = (import.meta.env.VITE_API_BASE_URL as string | undefined)?.trim() || 'https://localhost:7251'
const parsedApiBaseUrl = URL.canParse(configuredApiBaseUrl) ? new URL(configuredApiBaseUrl) : null
if (!parsedApiBaseUrl || !['https:', 'http:'].includes(parsedApiBaseUrl.protocol)) {
  throw new Error('VITE_API_BASE_URL must be an absolute HTTP or HTTPS API origin, such as https://localhost:7251.')
}
export const API_BASE_URL = configuredApiBaseUrl.replace(/\/$/, '')

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
    try {
      return await fetch(`${API_BASE_URL}${path}`, { ...init, headers })
    } catch (error) {
      if (error instanceof TypeError) {
        throw new Error(`The browser could not complete its request to ${API_BASE_URL}. Verify that the API is running, its HTTPS certificate is trusted, and CORS allows this frontend origin.`)
      }
      throw error
    }
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
    let message = body || `Request failed (${response.status})`
    try {
      const problem = JSON.parse(body) as { detail?: string; title?: string; errors?: Record<string, string[]> }
      message = problem.detail || problem.title || Object.values(problem.errors ?? {}).flat().join(' ') || message
    } catch {
      // Keep plain-text API error responses readable.
    }
    const error = new Error(message) as Error & ApiError
    error.status = response.status
    throw error
  }
  if (response.status === 204) return undefined as T
  const body = await response.text()
  return body ? JSON.parse(body) as T : undefined as T
}
