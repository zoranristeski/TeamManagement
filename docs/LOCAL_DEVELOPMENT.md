# Local Development

## Prerequisites

- .NET 10 SDK
- Node.js and npm
- SQL Server LocalDB or another SQL Server reachable from the API
- PowerShell on Windows

The API is under `src/SportsTeamManagement.API`; the React/Vite app is under
`src/SportsTeamManagement.Web`.

## Initial setup

From the repository root, trust the .NET HTTPS development certificate. This is a
one-time per-user operation and may ask for confirmation:

```powershell
dotnet dev-certs https --trust
dotnet dev-certs https --check --trust
```

Configure local database and API audience in user-secrets. Do not put database
credentials or signing keys in appsettings files or commit them:

```powershell
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=(localdb)\MSSQLLocalDB;Database=SportsTeamManagement;Trusted_Connection=True;TrustServerCertificate=True" --project .\src\SportsTeamManagement.API\SportsTeamManagement.API.csproj
dotnet user-secrets set "Authentication:Audience" "sports-team-management" --project .\src\SportsTeamManagement.API\SportsTeamManagement.API.csproj
```

The API startup script generates an ephemeral signing key for its API process when
`Authentication:SigningKey` is not configured. This key is never written to disk or
printed; authentication sessions are invalidated when that API process stops. To preserve
sessions across API restarts, set a private development key in user-secrets:

```powershell
dotnet user-secrets set "Authentication:SigningKey" "<private random key with at least 32 characters>" --project .\src\SportsTeamManagement.API\SportsTeamManagement.API.csproj
```

When you start the API with `scripts\dev.ps1`, it applies pending migrations and
creates idempotent sample data in a local SQL Server or LocalDB database. To run the API
manually without sample data, create/update the database yourself:

```powershell
dotnet ef database update --project .\src\SportsTeamManagement.Infrastructure\SportsTeamManagement.Infrastructure.csproj --startup-project .\src\SportsTeamManagement.API\SportsTeamManagement.API.csproj
```

Install web dependencies and configure the local API URL once:

```powershell
Set-Location .\src\SportsTeamManagement.Web
npm ci
Copy-Item .env.example .env.local
```

From the repository root, build the API and web app:

```powershell
dotnet build .\src\SportsTeamManagement.API\SportsTeamManagement.API.csproj
Set-Location .\src\SportsTeamManagement.Web
npm run build
Set-Location ..\..
```

The template uses `VITE_API_BASE_URL=https://localhost:7251`; Google and Facebook client
IDs are optional for local email/password testing. Provider secrets belong only in API
user-secrets or a secrets manager, never in `VITE_*` variables.

## Start both applications

From the repository root:

```powershell
.\scripts\dev.ps1
```

The script checks tool and project locations, frontend dependencies, API configuration,
and the HTTPS certificate. If the certificate is not trusted, it offers to run the
standard .NET trust command. It then starts the API with the `https` launch profile,
enables development sample seeding, and starts the frontend with the existing npm `dev`
script. The seeder applies pending migrations, then inserts sample rows only when the
demo admin is not already present. It refuses to seed non-local SQL Server instances.
If an endpoint is already responding on the expected port, the script reuses it and
does not stop it on exit; an already-running API will not run the seeder.

The script reports API liveness, SQL connectivity, Swagger, and frontend availability
separately. Database readiness must pass for registration to succeed. Press Enter to stop
only the processes started by the script.

With the script running, sign in with the seeded demo administrator:

- Email: `demo.admin@example.invalid`
- Password: `DemoAdmin!123`

The sample records cover all 24 application tables, including a club, users and
memberships, a student and trainer, groups and sessions, attendance and grading,
events and registrations, plans and payments, guardians, enrollment, identity
relationships, external login, refresh token, role history, and audit log. The
development-only demo password should not be reused outside a local development
database.

To run the applications manually in two terminals (without automatic demo seeding):

```powershell
dotnet run --project .\src\SportsTeamManagement.API\SportsTeamManagement.API.csproj --launch-profile https
```

```powershell
Set-Location .\src\SportsTeamManagement.Web
npm run dev
```

## URLs and diagnostics

- Frontend login: <http://localhost:5173/login>
- API HTTPS: <https://localhost:7251>
- Swagger: <https://localhost:7251/swagger/index.html>
- API liveness: <https://localhost:7251/health/live>
- Database readiness: <https://localhost:7251/health/ready>

Run local diagnostics while both processes are running:

```powershell
.\scripts\diagnose-dev.ps1
```

The diagnostic exits non-zero if API HTTPS, database readiness, Swagger, frontend, API
base URL, or the development CORS preflight check fails. It does not display secret values.

## Troubleshooting

### API process or port

The HTTPS launch profile binds `https://localhost:7251` and `http://localhost:5068` in
Development. A cloud-provided `PORT` variable is applied only outside Development and
cannot override these local launch-profile URLs. If a port is already occupied, close
the app using it and rerun the startup script.

### HTTPS certificate

If the browser or PowerShell rejects the local certificate, run:

```powershell
dotnet dev-certs https --trust
dotnet dev-certs https --check --trust
```

Restart the browser after trusting the certificate if it still reports an error. The
application does not disable certificate validation.

### CORS

Development CORS explicitly permits `http://localhost:5173` and the Vite preview origin
`http://localhost:4173`, including `Authorization`, `Content-Type`, and `X-Club-Id`.
Use `localhost` consistently; `127.0.0.1` is a different browser origin and is not
implicitly allowed. Production origins must be explicitly configured with
`Cors__AllowedOrigins__0` and subsequent indexed settings.

### Registration

Registration and login are explicitly anonymous; protected API routes remain subject to
the fallback authentication policy. If the API is live but registration fails, check:

1. `/health/ready` returns HTTP 200 and the database schema has been migrated.
2. `Authentication:Audience` is configured.
3. The frontend is running at the allowed `http://localhost:5173` origin.
4. `VITE_API_BASE_URL` in `.env.local` is exactly `https://localhost:7251`.
5. Passwords meet the API requirement: at least 12 characters with uppercase, lowercase,
   digit, and symbol characters, and terms must be accepted.

Use the API response shown on the form for validation/configuration failures. The
diagnostic script reports network, certificate, CORS, and SQL readiness separately.
