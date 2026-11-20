import { useMemo, useState, type ReactNode } from 'react'
import { AuthContext, type AuthContextValue } from './auth-context'

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
      if (refreshToken) {
        void fetch(`${(import.meta.env.VITE_API_BASE_URL as string | undefined)?.replace(/\/$/, '') || 'https://localhost:7001'}/api/auth/logout`, {
          method: 'POST',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify({ refreshToken }),
        })
      }
      localStorage.removeItem('sports_access_token')
      localStorage.removeItem('sports_refresh_token')
      localStorage.removeItem('sports_active_club')
      setToken(null)
    },
  }), [token])
  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}
