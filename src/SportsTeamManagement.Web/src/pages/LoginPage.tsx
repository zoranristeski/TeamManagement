import { useState } from 'react'
import { Shield } from 'lucide-react'
import { useAuth } from '../auth/useAuth'
import { apiRequest } from '../api/client'

type AuthResponse = {
  accessToken: string
  refreshToken: string
  expiresAtUtc: string
  user: { firstName: string; lastName: string; email: string }
}

export function LoginPage() {
  const { login } = useAuth()
  const [mode, setMode] = useState<'login' | 'register'>('login')
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [firstName, setFirstName] = useState('')
  const [lastName, setLastName] = useState('')
  const [acceptTerms, setAcceptTerms] = useState(false)
  const [error, setError] = useState('')

  const submit = async (event: React.FormEvent) => {
    event.preventDefault()
    setError('')
    try {
      const response = mode === 'login'
        ? await apiRequest<AuthResponse>('/api/auth/login', {
            method: 'POST',
            body: JSON.stringify({ email, password }),
          })
        : await apiRequest<AuthResponse>('/api/auth/register', {
            method: 'POST',
            body: JSON.stringify({ firstName, lastName, email, password, acceptTerms }),
          })
      localStorage.setItem('sports_refresh_token', response.refreshToken)
      login(response.accessToken)
    } catch (requestError) {
      setError(requestError instanceof Error ? requestError.message : 'Authentication failed.')
    }
  }

  const startProvider = (provider: 'google' | 'facebook') => {
    const config = provider === 'google'
      ? { clientId: import.meta.env.VITE_GOOGLE_CLIENT_ID, auth: 'https://accounts.google.com/o/oauth2/v2/auth' }
      : { clientId: import.meta.env.VITE_FACEBOOK_APP_ID, auth: 'https://www.facebook.com/v20.0/dialog/oauth' }
    if (!config.clientId) {
      setError(`${provider} OAuth is not configured.`)
      return
    }
    const redirectUri = `${window.location.origin}/auth/callback/${provider}`
    const params = new URLSearchParams({
      client_id: config.clientId,
      redirect_uri: redirectUri,
      response_type: 'code',
      scope: provider === 'google' ? 'openid email profile' : 'email public_profile',
      state: crypto.randomUUID(),
    })
    window.location.assign(`${config.auth}?${params}`)
  }

  return (
    <div className="login-page">
      <form className="login-card" onSubmit={submit}>
        <div className="login-brand"><span className="brand-mark"><Shield size={20} /></span><strong>TeamHub</strong></div>
        <h1>{mode === 'login' ? 'Sign in' : 'Create account'}</h1>
        {mode === 'register' && (
          <>
            <label>First name</label>
            <input value={firstName} onChange={event => setFirstName(event.target.value)} required />
            <label>Last name</label>
            <input value={lastName} onChange={event => setLastName(event.target.value)} required />
          </>
        )}
        <label>Email</label>
        <input type="email" value={email} onChange={event => setEmail(event.target.value)} required />
        <label>Password</label>
        <input type="password" value={password} onChange={event => setPassword(event.target.value)} required />
        {mode === 'register' && (
          <label><input type="checkbox" checked={acceptTerms} onChange={event => setAcceptTerms(event.target.checked)} /> I accept the terms</label>
        )}
        {error && <span className="field-error">{error}</span>}
        <button className="primary-button" type="submit">{mode === 'login' ? 'Sign in' : 'Register'}</button>
        <button type="button" onClick={() => setMode(mode === 'login' ? 'register' : 'login')}>
          {mode === 'login' ? 'Create account' : 'Already have an account?'}
        </button>
        <button type="button" onClick={() => startProvider('google')}>Continue with Google</button>
        <button type="button" onClick={() => startProvider('facebook')}>Continue with Facebook</button>
      </form>
    </div>
  )
}
