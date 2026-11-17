# API configuration and Azure App Service deployment

## Deployment model

Use the native .NET 10 Linux App Service runtime; Docker is not required. The API listens on the hosting-provided URL/port configuration, accepts `PORT` when explicitly provided, honors forwarded scheme headers, and enables HTTPS redirection/HSTS. App Service must be set to **HTTPS Only**.

The API does not automatically apply migrations at startup. Create the database, apply the reviewed migration, configure the App Service settings and SQL identity, and only then deploy the API.

## Required App Service application settings

In the portal open **App Services > <app> > Settings > Environment variables > App settings**. Add the values below and select **Apply**. App Service restarts the app after settings change.

| App setting | Required value |
| --- | --- |
| `ASPNETCORE_ENVIRONMENT` | `Production` |
| `ConnectionStrings__DefaultConnection` | Passwordless Azure SQL connection string using the web app's system-assigned identity. See format below. |
| `Authentication__Authority` | The real HTTPS issuer/authority URL for the configured OIDC provider. The project currently uses `Auth0Id`, but this value is not present in the repository and must be supplied by the owner. |
| `Authentication__Audience` | The exact API audience configured at the identity provider. Must not be a placeholder. |
| `Authentication__RoleClaimType` | `roles` if the validated JWT uses that claim; otherwise the provider's exact role-claim name. Role values must be `Admin`, `Trainer`, or `Student` with matching casing. |
| `Cors__AllowedOrigins__0` | Optional exact HTTPS browser origin. Add only known production frontend origins. No origin is preconfigured. |
| `ASPNETCORE_FORWARDEDHEADERS_ENABLED` | `true` for the App Service Linux reverse-proxy integration so original HTTPS scheme/client metadata is processed. |

Use the exact `ConnectionStrings__DefaultConnection` key under **App settings**; do not rely on the portal's typed connection-string section to translate it to a different provider key.

Passwordless SQL connection format (substitute only the actual server/database names):

```text
Server=tcp:<sql-server>.database.windows.net,1433;Database=<database>;Authentication=Active Directory Managed Identity;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;
```

This value contains no password. Never commit the actual server/database connection setting, provider secrets, access tokens, or client credentials. App Service app settings are encrypted at rest; use managed identity rather than a stored SQL password.

### Configure using Azure CLI

Set the required values without placing secrets in source control. The commands below contain only placeholders for owner-supplied settings:

```powershell
az webapp config appsettings set --name <app-name> --resource-group <resource-group> --settings ASPNETCORE_ENVIRONMENT=Production ASPNETCORE_FORWARDEDHEADERS_ENABLED=true Authentication__Authority="<actual-https-issuer>" Authentication__Audience="<actual-api-audience>" Authentication__RoleClaimType=roles
```

Build the managed-identity connection string in the current PowerShell process and set it as an App Service application setting:

```powershell
$connectionString = "Server=tcp:<sql-server>.database.windows.net,1433;Database=<database>;Authentication=Active Directory Managed Identity;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
az webapp config appsettings set --name <app-name> --resource-group <resource-group> --settings "ConnectionStrings__DefaultConnection=$connectionString"
Remove-Variable connectionString
```

For each approved web origin:

```powershell
az webapp config appsettings set --name <app-name> --resource-group <resource-group> --settings Cors__AllowedOrigins__0="https://<approved-frontend-host>"
```

For multiple origins use indexed keys `Cors__AllowedOrigins__1`, `Cors__AllowedOrigins__2`, and so on. Values must be origins only (scheme/host/port), not paths or wildcard patterns. No CORS origins means browser origins are denied.

## Authentication and authorization behavior

- The API validates JWT bearer tokens against the configured HTTPS `Authority` and `Audience`.
- The fallback authorization policy requires both an authenticated principal and the `Admin` role. This protects all controller actions by default, including student, attendance, user, and event-related APIs.
- `POST /api/students/register` explicitly requires authentication but not the `Admin` role. The API overwrites the submitted `Auth0Id` with the validated token's `sub` claim; a client cannot choose another identity by submitting a different ID.
- The registration endpoint still accepts student profile data, including medical/emergency fields. See the production blockers in [08-security-checklist.md](./08-security-checklist.md).
- The identity provider must issue a trusted role claim. Configure the provider's role assignment/mapping out of band. Do not infer the role from request body data or a frontend screen.

Swagger and Swagger UI are enabled only in the Development environment. They are intentionally not exposed in Production. Health endpoints are anonymous; `/health/live` does not access SQL and `/health/ready` returns only a generic overall status.

## HTTPS, reverse proxy, ports, logging, and errors

- Enable **HTTPS Only** on the App Service and retain `UseHttpsRedirection` and production HSTS.
- Set `ASPNETCORE_FORWARDEDHEADERS_ENABLED=true` for App Service's trusted proxy integration. Forwarded headers must be processed before HTTPS redirection.
- `WebApplication.CreateBuilder` uses ASP.NET Core's normal URL/port providers. The app additionally honors a valid `PORT` environment variable when supplied. Do not hard-code a development port in Azure.
- Production logs use JSON console output suitable for App Service log streaming. EF command logging is kept at Warning; do not enable SQL parameter logging or request-body logging for student data.
- The exception handler logs server-side details with a trace identifier and returns a generic `application/problem+json` response. Stack traces, SQL exceptions, and connection strings are not sent to callers.
- Rate limiting applies to API requests at 120 requests per authenticated subject per minute; health probes are exempt. Increase this only after measuring and evaluating abuse/capacity risk.

## Deploy the native published application

The checked-in [GitHub Actions workflow](../../.github/workflows/azure-deploy.yml) restores, builds, tests, publishes, and offers a **manual-only** Azure deployment after the build job succeeds. It does not create resources, modify App Service settings, or apply database migrations.

For the workflow setup, see [07-github-actions.md](./07-github-actions.md). To publish locally:

```powershell
dotnet restore .\src\SportsTeamManagement.API\SportsTeamManagement.API.csproj
dotnet test .\src\SportsTeamManagement.API.Tests\SportsTeamManagement.API.Tests.csproj --configuration Release
dotnet publish .\src\SportsTeamManagement.API\SportsTeamManagement.API.csproj --configuration Release --output .\artifacts\api
```

Deploy only after the database migration and App Service settings are verified. No deployment command has been run from this session.

## Deployment smoke checks

```powershell
Invoke-WebRequest -Method Get -Uri "https://<app-name>.azurewebsites.net/health/live"
Invoke-WebRequest -Method Get -Uri "https://<app-name>.azurewebsites.net/health/ready"
```

The liveness endpoint should return HTTP 200 without SQL access. Readiness should return HTTP 200 and `Healthy` after database configuration/migration, or an unhealthy response when SQL is unavailable. It never includes connection details.

Check `https://<app-name>.azurewebsites.net/swagger` in Production; it should not serve the development Swagger UI. Verify an anonymous student endpoint returns 401, an authenticated non-admin returns 403, and a valid Admin token can access an authorized route.

## MAUI client URL

The current MAUI project contains no API `HttpClient` base address or localhost API URL to change. Do not change the mobile project as part of this deployment. When API integration is added, configure its base address to `https://<app-name>.azurewebsites.net/` using environment-specific client configuration and a valid bearer token. A native MAUI client is not subject to browser CORS.
