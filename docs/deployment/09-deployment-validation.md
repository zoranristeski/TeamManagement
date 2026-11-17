# Deployment validation checklist

This checklist is for a manually approved POC deployment. No Azure resource or live database has been created as part of the implementation.

## Local API build and automated tests

From the repository root, with the .NET 10 SDK:

```powershell
dotnet restore .\src\SportsTeamManagement.API.Tests\SportsTeamManagement.API.Tests.csproj
dotnet build .\src\SportsTeamManagement.API\SportsTeamManagement.API.csproj --configuration Release --no-restore
dotnet test .\src\SportsTeamManagement.API.Tests\SportsTeamManagement.API.Tests.csproj --configuration Release --no-restore
dotnet publish .\src\SportsTeamManagement.API\SportsTeamManagement.API.csproj --configuration Release --no-restore --output .\artifacts\api
```

The automated integration tests verify:

- Anonymous liveness requests are allowed.
- Anonymous student API requests receive 401.
- A non-admin authenticated role receives 403 on a protected student endpoint.
- CORS allows the explicitly configured test origin and does not return an allow-origin header for an untrusted origin.

These tests do not contact SQL, validate a real JWT issuer, exercise CRUD, or build the MAUI targets.

## Required local configuration and API startup

The API deliberately exits at startup if either `ConnectionStrings:DefaultConnection`, `Authentication:Authority`, or `Authentication:Audience` is missing. For local operation, use User Secrets for the connection and provider settings; do not put credentials in checked-in JSON or launch settings.

```powershell
dotnet user-secrets set "Authentication:Authority" "<actual-https-issuer>" --project .\src\SportsTeamManagement.API\SportsTeamManagement.API.csproj
dotnet user-secrets set "Authentication:Audience" "<actual-api-audience>" --project .\src\SportsTeamManagement.API\SportsTeamManagement.API.csproj
dotnet user-secrets set "Authentication:RoleClaimType" "roles" --project .\src\SportsTeamManagement.API\SportsTeamManagement.API.csproj
dotnet run --project .\src\SportsTeamManagement.API\SportsTeamManagement.API.csproj
```

Set a local development connection string through User Secrets using an interactive prompt; avoid typing it directly into a command stored in shell history. Development Swagger should be available only after valid required configuration is present.

## Database migration and connectivity

- [ ] Confirm the target database is new and empty.
- [ ] Review `scripts/deployment/001-initial-create.sql` and the generated EF migration.
- [ ] Apply it through the separate migration operator using `scripts/deployment/apply-migrations.ps1` or an approved SQL deployment mechanism.
- [ ] Verify `__EFMigrationsHistory` includes `20261117014023_InitialCreate`.
- [ ] Verify the API runtime identity has `db_datareader`/`db_datawriter`, but no DDL role.
- [ ] Confirm `/health/ready` can connect after deployment and fails safely when SQL is temporarily unavailable.
- [ ] Run approved user/student/attendance CRUD smoke tests with synthetic data; verify expected constraints, uniqueness, foreign keys, and soft-delete behavior.
- [ ] Do not run these tests with real student data until the security blockers in [08-security-checklist.md](./08-security-checklist.md) are closed.

No live database connection/migration/CRUD verification is claimed by this implementation.

## Production endpoint checks

Replace `<app-name>` with the actual App Service hostname:

```powershell
$baseUrl = "https://<app-name>.azurewebsites.net"
Invoke-WebRequest -Method Get -Uri "$baseUrl/health/live"
Invoke-WebRequest -Method Get -Uri "$baseUrl/health/ready"
```

Expected:

- `/health/live`: HTTP 200 and only a generic status.
- `/health/ready`: HTTP 200 and `Healthy` only after SQL is reachable; otherwise a generic unhealthy status/503, no connection or exception details.
- `/swagger` and `/swagger/index.html`: not exposed in Production.
- Anonymous `GET /api/students/eligible-for-grading`: 401.
- Authenticated token without `Admin`: 403.
- Valid token with the configured Admin claim: authorized. Test with a real, scoped access token and never paste it into source control or chat.
- HTTPS-only redirect/HSTS: verify browser/network response and no HTTP API data is served.
- CORS preflight: exact configured origin receives the corresponding CORS headers; an unconfigured origin does not.

Example PowerShell preflight (set a real approved origin):

```powershell
$origin = "https://<approved-frontend-origin>"
Invoke-WebRequest -Method Options -Uri "$baseUrl/api/students/eligible-for-grading" -Headers @{
    Origin = $origin
    "Access-Control-Request-Method" = "GET"
}
```

## Swagger/OpenAPI

- [ ] In Development only, visit `/swagger` and confirm the generated API document loads.
- [ ] Verify Production does not map Swagger.
- [ ] Do not publish production OpenAPI docs publicly unless the owner explicitly approves a secure, authenticated documentation design.

## GitHub Actions

- [ ] A push/PR build job restores, builds, tests, and publishes without an Azure login.
- [ ] A failing test prevents the deployment job.
- [ ] A push to `main` does not deploy.
- [ ] Manual `workflow_dispatch` on `main` reaches the protected `azure-poc` environment and authenticates with OIDC.
- [ ] A manual dispatch from another branch is skipped for deployment.
- [ ] The deployment identity has Website Contributor only on the target app.
- [ ] The workflow contains no Azure client secret, SQL password, connection setting, JWT, or migration step.

## MAUI client validation

The current MAUI project has no configured API client/base URL. When integration is added, set its `HttpClient.BaseAddress` to the Azure `https://<app-name>.azurewebsites.net/` endpoint through mobile environment configuration, implement token acquisition/refresh, and test the actual supported mobile platforms. Native mobile requests do not require CORS, but the API still requires valid bearer tokens. Do not hard-code a production token or change the MAUI target framework as part of API deployment.

## Sign-off record

Record the date, approver, app hostname, subscription/resource-group names, SQL migration ID, auth issuer/audience identifiers (not credentials), test results, backup/restore test result, and any accepted risks in the team's approved operations system. Do not store secrets or student data in the record.
