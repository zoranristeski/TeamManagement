import { useForm } from 'react-hook-form'
import { z } from 'zod'
import { zodResolver } from '@hookform/resolvers/zod'
import { Shield } from 'lucide-react'
import { useAuth } from '../auth/useAuth'

const schema = z.object({
  token: z.string().trim().min(20, 'Enter a valid access token'),
})

export function LoginPage() {
  const { login } = useAuth()
  const { register, handleSubmit, formState: { errors } } = useForm<{ token: string }>({
    resolver: zodResolver(schema),
  })
  return (
    <div className="login-page">
      <form className="login-card" onSubmit={handleSubmit(({ token }) => login(token))}>
        <div className="login-brand"><span className="brand-mark"><Shield size={20} /></span><strong>TeamHub</strong></div>
        <h1>Sign in</h1>
        <p>Use an access token issued by the configured authentication provider.</p>
        <label>Access token</label>
        <textarea rows={5} {...register('token')} placeholder="Paste bearer access token" />
        {errors.token && <span className="field-error">{errors.token.message}</span>}
        <button className="primary-button" type="submit">Continue</button>
      </form>
    </div>
  )
}
