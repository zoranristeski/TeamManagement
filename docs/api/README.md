# API Development

## Projects

- `SportsTeamManagement.API`: ASP.NET Core .NET 10 REST API.
- `SportsTeamManagement.Application`: DTOs, mappings, application services, and ports.
- `SportsTeamManagement.Infrastructure`: EF Core, SQL Server, repositories, and migrations.
- `SportsTeamManagement.Domain`: entities and enums.

## Configuration

Set these environment variables or user secrets:

- `ConnectionStrings__DefaultConnection`
- `Authentication__Authority`
- `Authentication__Audience`
- `Cors__AllowedOrigins__0`

The API rejects missing database, issuer, audience, or invalid CORS configuration at startup.

## Local development

```powershell
dotnet restore
dotnet run --project src/SportsTeamManagement.API
```

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
