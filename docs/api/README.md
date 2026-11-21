# API Development

## Projects

- `SportsTeamManagement.API`: ASP.NET Core .NET 10 REST API.
- `SportsTeamManagement.Application`: DTOs, mappings, application services, and ports.
- `SportsTeamManagement.Infrastructure`: EF Core, SQL Server, repositories, and migrations.
- `SportsTeamManagement.Domain`: entities and enums.

## Configuration

Set these environment variables or user secrets:

- `ConnectionStrings__DefaultConnection`
- `Authentication__Audience`
- `Authentication__SigningKey` (local JWT signing key, at least 32 characters)
- `Authentication__Authority` when using an external JWT issuer instead of local token signing
- `Cors__AllowedOrigins__0` for deployed browser frontends

The API rejects missing database, audience, issuer/signing configuration, or invalid CORS
configuration at startup.

## Local development

Local web development runs on `http://localhost:5173`; that origin is allowed in the
Development configuration. The API HTTPS port is `7251`. Trust the local .NET HTTPS
certificate before using the web app:

```powershell
dotnet dev-certs https --trust
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=(localdb)\MSSQLLocalDB;Database=SportsTeamManagement;Trusted_Connection=True;TrustServerCertificate=True" --project src/SportsTeamManagement.API
dotnet user-secrets set "Authentication:Audience" "sports-team-management" --project src/SportsTeamManagement.API
dotnet user-secrets set "Authentication:SigningKey" "<a private random key of at least 32 characters>" --project src/SportsTeamManagement.API
```

Then apply the migrations:

```powershell
dotnet ef database update --project src/SportsTeamManagement.Infrastructure --startup-project src/SportsTeamManagement.API
```

Registration and login issue local API tokens and require the signing key. Do not use a
development signing key in production.

```powershell
dotnet restore
dotnet run --project src/SportsTeamManagement.API
```

For the React app, copy `.env.example` to `.env.local` in `src/SportsTeamManagement.Web`.
Google/Facebook buttons require public web client IDs and matching server-side provider
secrets; register `http://localhost:5173/auth/callback/google` and
`http://localhost:5173/auth/callback/facebook` with the respective provider consoles.
Never put provider secrets in a `VITE_*` variable.

Swagger is available only in the Development environment.

## Migrations

Set `ConnectionStrings__DefaultConnection` before EF design-time commands:

```powershell
dotnet ef database update `
  --project src/SportsTeamManagement.Infrastructure `
  --startup-project src/SportsTeamManagement.API
```

## Health

- `/health/live`: anonymous liveness probe.
- `/health/ready`: SQL readiness probe.

## Implemented route groups

- `/api/users`
- `/api/students`
- `/api/attendance`
- `/api/clubs`
- `/api/traininggroups`

Bulk attendance is available at `POST /api/attendance/bulk`.
