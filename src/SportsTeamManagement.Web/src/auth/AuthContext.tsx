import { useMemo, useState, type ReactNode } from 'react'
import { AuthContext, type AuthContextValue } from './auth-context'
import { apiRequest } from '../api/client'

export function AuthProvider({ children }: { children: ReactNode }) {
  const [token, setToken] = useState(() => localStorage.getItem('sports_access_token'))
  const value = useMemo<AuthContextValue>(() => ({
    token,
    login: (nextToken: string) => {
      localStorage.setItem('sports_access_token', nextToken)
      setToken(nextToken)
    },
    logout: () => {
      const refreshToken = localStorage.getItem('sports_refresh_token')
      if (refreshToken) void apiRequest<void>('/api/auth/logout', {
        method: 'POST',
        body: JSON.stringify({ refreshToken }),
      }).catch(error => console.error('Unable to revoke the current session.', error))
      localStorage.removeItem('sports_access_token')
      localStorage.removeItem('sports_refresh_token')
      localStorage.removeItem('sports_active_club')
      setToken(null)
    },
  }), [token])
  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}
