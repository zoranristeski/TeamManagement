import { useEffect, useState } from 'react'
import { useParams } from 'react-router-dom'
import { apiRequest } from '../api/client'
import { useAuth } from '../auth/useAuth'

type AuthResponse = {
  accessToken: string
  refreshToken: string
}

export function AuthCallbackPage() {
  const { provider } = useParams()
  const { login } = useAuth()
  const [error, setError] = useState('')

  useEffect(() => {
    const code = new URLSearchParams(window.location.search).get('code')
    if (!code || (provider !== 'google' && provider !== 'facebook')) {
      setError('The external login callback is invalid.')
      return
    }
    const redirectUri = `${window.location.origin}/auth/callback/${provider}`
    apiRequest<AuthResponse>(`/api/auth/external/${provider}`, {
      method: 'POST',
      body: JSON.stringify({ code, redirectUri }),
    }).then(response => {
      localStorage.setItem('sports_refresh_token', response.refreshToken)
      login(response.accessToken)
    }).catch(() => setError('External authentication failed.'))
  }, [login, provider])

  return <div className="login-page"><div className="login-card"><h1>Completing sign in</h1>{error && <span className="field-error">{error}</span>}</div></div>
}
