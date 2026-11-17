# SportsTeamManagement - Azure Deployment Architecture Assessment

**Assessment date:** 2026-11-17  
**Scope:** Repository source, project and application configuration, and current Microsoft/GitHub service documentation.  
**Phase:** Assessment only. No application code, website, Azure resources, or deployment configuration has been changed or created.

## Executive summary

The solution is a small Clean Architecture-style .NET solution containing an ASP.NET Core API, application/domain/infrastructure libraries, and a .NET MAUI starter application. The API's persistence model is SQL Server-compatible and can use Azure SQL Database without a database-engine redesign.

The proposed Azure combination is technically viable for a low-use development/proof-of-concept environment, subject to very restrictive free quotas and no service-level agreement (SLA). It is not a reliable production configuration. Azure currently lists an App Service F1 Free plan and a separate, recurring Azure SQL Database free tier. The latter is not limited to the first 12 months, but is available only for new databases, has a small monthly compute allowance, and can pause when that allowance is exhausted. An App Configuration Free tier is also available, but its request limit and lack of SLA need to be considered.

**Do not deploy the current API publicly or load real student data yet.** The API has no configured authentication handler and no endpoint authorization attributes. In particular, user/student APIs return or accept personally identifying information and student medical/emergency-contact data. The API also lacks a database connection string and EF Core migrations. In addition, the solution targets .NET 9, which reached end of support on 2026-11-10. Move to a supported runtime before deployment.

The first Azure deployment should be a separate App Service with its generated `azurewebsites.net` hostname. Nothing in this assessment requires or changes `ikigai.mk`, its DNS, or its existing MKHost hosting.

## 1. Existing solution and application architecture

The repository root contains `SportsTeamManagement.sln` and `src/`. The solution has five projects:

| Project | Current target | Role observed |
| --- | --- | --- |
| `SportsTeamManagement.API` | `net9.0` | ASP.NET Core Web API, controllers, startup and Swagger |
| `SportsTeamManagement.Application` | `net9.0` | Application services, interfaces, DTOs, mapping and result types |
| `SportsTeamManagement.Domain` | `net9.0` | Entities, enums and shared entity base type |
| `SportsTeamManagement.Infrastructure` | `net9.0` | EF Core SQL Server context, entity configuration and repositories |
| `SportsTeamManagement.Mobile` | `net9.0-android`, `net9.0-ios`, `net9.0-maccatalyst`; adds `net9.0-windows10.0.19041.0` on Windows | .NET MAUI application |

The API references Application and Infrastructure; Infrastructure references Application and Domain; Application references Domain. Application DI registers AutoMapper and user, student, attendance, and training-session services. Infrastructure DI registers `ApplicationDbContext` and repositories/unit of work. The solution contains no separate test project.

### Domain and persistence

`ApplicationDbContext` has sets for `User`, `Student`, `Trainer`, `TrainingSession`, `Attendance`, `BeltPromotion`, `Event`, and `EventRegistration`. Entity configurations are applied from the Infrastructure assembly.

The configured relationships include:

- A user has an optional one-to-one student and an optional one-to-one trainer record. Deleting a user cascades to either dependent record.
- Trainers own training sessions and events and are referenced by belt promotions. Deletion of a referenced trainer is restricted.
- Students have attendance, belt promotion, and event registration records. Deletion cascades to these records.
- Attendance is unique by student and training session; event registration is unique by student and event.
- Several properties have explicit maximum lengths, enum-to-integer conversions, and event/payment decimal precision.

The model contains one-club operational records but no `Club`/tenant identifier or tenant-scoped access policy. This is not a blocker for a single-club POC; it is a prerequisite for safely separating multiple clubs before multi-tenant SaaS use. Do not treat the current model as tenant-isolated.

`SaveChangesAsync` sets `CreatedAt` and `UpdatedAt` for `BaseEntity` entries. No migration classes or migrations directory were found. There is no startup migration or database initialization/seeding code. Consequently, a newly created Azure SQL database will not have the application's schema until an initial migration is created and applied.

### NuGet/runtime dependencies

Observed package references include:

- API: `Microsoft.AspNetCore.OpenApi` 9.0.4, `Microsoft.EntityFrameworkCore.Design` 9.0.9, `Swashbuckle.AspNetCore` 9.0.6.
- Infrastructure: `Microsoft.EntityFrameworkCore` 9.0.9, `Microsoft.EntityFrameworkCore.SqlServer` 9.0.9, `Microsoft.EntityFrameworkCore.Tools` 9.0.9.
- Application: `AutoMapper` 15.0.1.
- MAUI: `Microsoft.Maui.Controls` using `$(MauiVersion)` and `Microsoft.Extensions.Logging.Debug` 9.0.0.

The API and EF Core are on .NET 9 major versions. Microsoft's live lifecycle page lists .NET 9 as out of support as of **2026-11-10**; the same page lists .NET 10 as an active LTS release through 2028-11-14. Upgrade the API and libraries to a supported LTS target (currently .NET 10), align EF Core/provider/tooling versions, and run build, migration, and integration checks before publishing. Do not assume the MAUI app can be upgraded independently without checking its installed workload/platform support.

## 2. API startup, configuration, and operations

The API's `Program.cs` registers controllers, endpoint exploration, Swagger, CORS, Application DI, and Infrastructure DI. It uses `UseHttpsRedirection`, CORS, authentication and authorization middleware, then maps controllers. Swagger and Swagger UI are enabled only when the environment is Development.

The exposed controller surface currently covers users, students, and attendance only. Domain entities also include trainers, training sessions, belt promotions, events, and event registrations; a training-session application service exists, but no API controllers for these modules were found. Treat those capabilities as not yet exposed through this API rather than assuming they are deploy-ready.

### Database configuration and migrations

`Infrastructure/DependencyInjection.cs` reads `ConnectionStrings:DefaultConnection` and passes it to `UseSqlServer`. Neither `appsettings.json` nor `appsettings.Development.json` defines that connection string. A deployed app therefore has no configured Azure database target.

`ApplicationDbContextFactory` hardcodes a SQL Server LocalDB connection (`(localdb)\\mssqllocaldb`). This is a design-time developer-machine dependency, not an Azure or Linux CI connection. There are no migrations in the solution. A cloud migration workflow must be deliberately added; EF tooling currently has no cloud-configurable design-time connection path.

**Required before deployment:** use environment-specific runtime configuration (App Service settings or the App Configuration provider), keep credentials out of source, provide a safe EF design-time configuration, create and review an initial migration, then apply migrations to an empty Azure SQL Free database. Prefer a controlled deployment migration step over an unreviewed migration on every API startup. Use managed identity for SQL authentication if the selected App Service/SQL configuration supports it; otherwise store a least-privilege credential in an appropriate secret store, not in source or App Configuration values.

### Authentication, authorization, CORS, and data sensitivity

The API invokes `UseAuthentication()` and `UseAuthorization()`, but there is no explicit `AddAuthentication`/`AddAuthorization` configuration, authentication scheme or handler, JWT validation configuration, or `[Authorize]` attribute on the inspected controllers. A `User.Auth0Id` field and routes named for Auth0 are present, but do not constitute token validation or trusted identity. `StudentRegistrationDto` accepts an `Auth0Id` from the request body; the API must not treat a caller-supplied identifier as proof of identity.

The current user, student, and attendance controllers have read/write endpoints without authorization attributes. Student records include date of birth, emergency contact details, and medical conditions. User records include email, name, phone, and profile-image URL. This is especially sensitive for a sports academy that may serve minors.

The API configures a CORS policy named `AllowAll` using any origin, method, and header. This is unnecessarily broad. Native MAUI clients are not protected by browser CORS; if a browser client is later introduced, permit only its exact trusted origins. `ikigai.mk` is not being integrated or modified as part of this deployment.

**Deployment gate:** implement and test a real identity/token-validation strategy (the existing Auth0 identifiers suggest an intended provider, but no integration is configured), protect every endpoint by default, add least-privilege role/policy checks for administrative operations, and verify registration and user lookup cannot be abused. Do not deploy real student data before this gate is closed. For an early POC, use synthetic data.

### Logging, exceptions, API documentation, and health

The application has default logging-level settings (`Information` and `Microsoft.AspNetCore: Warning`), but no explicit structured logging provider, global exception-handling middleware, or health-check endpoint was found. `StudentService.RegisterStudentAsync` catches all exceptions, rolls back, and returns a generic failure without logging; the controller maps failures to HTTP 400. Other unhandled exceptions have no application-level error mapping identified. This reduces diagnostic visibility and can misclassify server failures as client errors.

Swagger is development-only, which is a reasonable default; if it is enabled in a hosted POC, protect or restrict it. Add a non-sensitive liveness/readiness strategy and make operational errors visible in App Service logs without returning sensitive exception details to callers.

`UseHttpsRedirection()` is already present. The hosted app should additionally have HTTPS-only enforcement configured in Azure and be tested through its generated hostname and the App Service proxy. No custom domain or DNS change is required.

## 3. Mobile, local dependencies, tests, and source-control observations

The MAUI project currently contains the template counter screen. Its `MauiProgram` only configures the app, fonts, and debug logging. No HTTP client, API base URL, API authentication flow, or application API integration was found. There is no hardcoded localhost API endpoint to update. Treat the mobile application as a separate future client; it is not a web deployment target and does not provide API authentication.

No application runtime dependency on local files, a local filesystem database, or machine-specific services was found. The exception is the LocalDB connection in the EF design-time factory described above. No API image/file upload or storage integration was found.

No test project, test files, EF migration files, `.github` workflow, `global.json`, `Directory.Build.*`, or `.gitignore` appeared in the inspected repository inventory. The current workspace is not a Git repository, so Git history, tracked-file status, remote configuration, and previously committed secrets could not be verified. No credential literal was observed in the inspected application settings; the hardcoded LocalDB string contains no password. Before connecting a GitHub repository, establish secret scanning/protection and ensure local settings, publish profiles, database credentials, and deployment artifacts are ignored and never committed.

GitHub Actions is a suitable CI/CD route for building and validating the API and deploying it to App Service. Prefer GitHub-to-Azure OpenID Connect federation with least-privilege permissions over a long-lived publish-profile secret. There is no workflow today. GitHub Actions is billed by GitHub plan rather than Azure: current GitHub Free documentation lists included compute usage for private repositories and 500 MB artifact storage; public standard runners are free. Usage above the applicable plan allowance can incur GitHub charges, so confirm repository visibility, plan, and spending limits.

## 4. Current Azure Free-tier feasibility (verified 2026-11-17)

The following is based on current Microsoft pricing and Learn pages, not historical blog posts or the old 12-month Azure offer. Free allowances are not a guarantee of reliability and may change; verify the selected subscription and region in the Azure portal immediately before provisioning.

| Component | Current free offer and constraints | Assessment for this API |
| --- | --- | --- |
| **Azure App Service** | Microsoft's current App Service pricing page lists the F1 Free plan under its Free/Shared plans section: shared compute, **60 CPU minutes per app per day**, **1 GB RAM**, and **1 GB storage**. Free/Shared is intended for trials, experimentation, and learning; it has **no SLA** and is not supported for production. The hosting-plan documentation says Free/Shared apps cannot scale out. | Suitable only for a very low-traffic, non-production POC. CPU quota, shared capacity, storage, and no SLA make it unsuitable for a dependable user-facing service. Confirm the F1 SKU and required OS/runtime are offered in the target region/subscription before creating the app. |
| **Azure SQL Database Free tier** | A new General Purpose serverless database gets **100,000 vCore-seconds, 32 GB data, and 32 GB backup storage per database per calendar month**, recurring for the lifetime of the subscription; up to **10** free databases per subscription. This is separate from the Azure account's introductory 12-month offer. It is available on most Azure subscription types, but **Azure for Students Starter is unsupported**. A new database is required; an existing database cannot be restored/converted into this tier. Free-limit exhaustion can be set to **AutoPause** to prevent overage charges; then the database is unavailable until the next month. Free DBs auto-pause after 15 minutes of inactivity. | The ordinary relational EF Core model is compatible in principle. A low-use development database can fit, but the quota is small and pause/resume introduces cold-start latency. Use synthetic data, monitor remaining vCore seconds, and select AutoPause rather than paid overages for the POC. |
| **Azure App Configuration** | Current pricing lists a Free tier with **10 MB per store**, **1,000 requests per day per resource**, up to **3 resources per subscription per region**, no guaranteed throughput, and **no SLA**. | Available for low-volume POC configuration, but there is no provider integration today. Cache settings and avoid frequent refresh/polling. Use it for non-secret configuration; it complements rather than replaces a secret store. App Service settings are a simpler fallback if the quota or extra service is not acceptable. |
| **Generated hostname / HTTPS** | App Service assigns a default `azurewebsites.net` hostname. The deployment can use that hostname without buying or binding a custom domain. | Meets the no-domain objective. Enforce HTTPS in App Service and verify the default hostname over HTTPS. |
| **GitHub Actions** | GitHub Free currently includes an allowance for private-repository Actions compute and 500 MB artifact storage; usage conditions differ for public repositories and usage beyond included allowance can be billed by GitHub. | No Azure charge for GitHub itself, but not necessarily unlimited/free CI. Keep artifacts small and verify the GitHub account/repository allowance. |

### Region, subscription, and database behavior

- Microsoft documents the Azure SQL free tier as available to most subscription types, with Azure for Students Starter explicitly excluded. An active Azure subscription, permissions to create the resources, and a supported SQL logical server/region are still required.
- The SQL free-tier documentation does not provide a single static region allow-list for all subscriptions and configurations. Check the actual free-tier creation flow in the intended subscription and region. With Advanced configuration, the region selected for the first free-tier database applies to the subscription's other free-tier databases and cannot be changed. Select the region deliberately.
- F1 availability is SKU-, OS-, and region-dependent; there is no subscription-independent promise in the pricing listing that every region/account can create the desired F1 app. Check the portal's App Service plan options for the same target region. The API and SQL database should be colocated where possible.
- AutoPause at the SQL monthly free limit prevents overage charges but intentionally sacrifices availability until the monthly allowance resets. The SQL Free tier also has no SLA. Open database sessions, frequent database traffic, and monitoring that queries the database can consume the allowance or inhibit idle pause; monitor actual metrics.
- The SQL free tier has feature/configuration limits: with AutoPause-at-limit, the documented maximum is 4 vCores and 32 GB; point-in-time restore retention is seven days, locally redundant backup is used, and long-term retention is unavailable. Data Sync, Elastic Jobs, DNS aliases, elastic pools, and failover groups are not available in the free configuration.
- Azure SQL's free allowance does not make the entire application free. App hosting, App Configuration (if its quotas are exceeded), bandwidth/other supporting services, monitoring choices, and GitHub usage have separate terms or possible charges. Use Azure Cost Management budgets/alerts and verify that the SQL database's exhaustion behavior is AutoPause.
- F1 is shared and has no private network isolation. SQL public-endpoint firewall rules must be narrow and explicit; avoid the broad “Allow Azure services” firewall exception where possible. Private networking or other advanced isolation can require a paid App Service plan. Determine a supported secure SQL connection approach before provisioning.

### Cheapest suitable alternative when free is not adequate

For a POC, the free App Service + SQL Free combination is the lowest-cost fit if its quota-driven downtime and shared compute are acceptable. If the API must remain available, use at least a paid dedicated App Service tier (for example, Basic B1) and a paid Azure SQL tier sized for the workload; a low-end DTU tier or serverless General Purpose may be candidates, subject to feature and performance validation. Check the current calculator for the actual region, OS, backup, egress, and usage. Keeping SQL Free with AutoPause is not a reliability substitute for paid database capacity.

## 5. Deployment blockers, risks, and recommendations

### Blocking before a public deployment

1. **Runtime lifecycle:** upgrade from unsupported .NET 9 to a currently supported LTS runtime and align .NET/EF Core/provider/tool packages.
2. **Identity and authorization:** register a real auth scheme/token validator and authorize every endpoint. Enforce roles/policies server-side; never trust an Auth0 ID supplied in a request body. Add tests for anonymous, unauthorized, and permitted access.
3. **Database setup:** provide the Azure connection through secure external configuration, add reviewed EF migrations, and establish an explicit first-deployment migration procedure. Remove LocalDB as the only design-time path for EF tooling in CI.
4. **Database credentials/network:** choose managed identity/Entra authentication if compatible with the selected plan; otherwise use a protected secret source and least-privilege SQL principal. Restrict SQL firewall access and use TLS.
5. **CORS:** replace `AllowAll` with the minimum required browser origins, or remove browser CORS exposure if no browser client is planned. Do not add the existing website as an origin without an explicit integration requirement.
6. **Operations and error handling:** add centralized exception handling/logging and a health strategy; preserve useful server-side diagnostics while returning safe public errors.
7. **Build/verification:** introduce API tests and CI checks for restore, build, tests, and migration validation. There are no tests or workflows in the current solution.
8. **Data protection and privacy:** deploy only synthetic POC data until access controls, retention, privacy, and region/data-residency expectations for student records (including minors and health information) have been reviewed.

### Important but not a first POC code blocker

- Decide how the API will support future multi-club tenancy before adding additional clubs. Introduce tenant identity, tenant-scoped queries/authorization, and isolation tests as a coherent design rather than assuming a `ClubId` column alone is sufficient.
- Add the MAUI API client, environment-specific base URL, token acquisition/storage, and platform-specific release/signing pipeline when mobile integration is in scope. The current MAUI project is a template and is not part of API deployment.
- Establish GitHub repository protections and secret scanning before publishing source. Configure Azure OIDC trust for the deployment workflow and constrain its Azure permissions and branch/environment scope.
- Keep Swagger disabled or protected in hosted environments unless intentionally exposed.

## 6. Recommended deployment shape

For the first development deployment, preserve the current layered application and use:

1. A separate Azure App Service F1 app in a region where the selected OS/runtime and F1 plan are available, with only the generated Azure hostname and HTTPS-only enabled.
2. A new Azure SQL Database Free General Purpose/serverless database, in the same region if available, with the free limit enabled and exhaustion set to AutoPause.
3. Azure App Configuration Free for environment-specific non-secret settings after integrating its .NET provider. Keep any database credential in an App Service connection-string setting or a secret store, not in App Configuration plaintext values or source control. Use managed identity for database access where supported.
4. A GitHub Actions API-only workflow using OIDC, with restore/build/test/migration validation and controlled deployment.
5. Synthetic test data only until identity, authorization, and privacy gates are complete.

This deployment is independent of `ikigai.mk`; do not change that site's files, hosting, DNS, or domain configuration.

## 7. Evidence reviewed

Repository files reviewed include the solution and all project files; API startup, settings, launch profiles, and controllers; Application and Infrastructure DI, services, repositories, DbContext, design-time factory, and EF entity configurations; Domain entities; MAUI startup and main page; and DTOs relevant to API data exposure. The source/config scan found no deployment workflow, test project, migrations, API localhost URL, or application runtime filesystem dependency.

Current product terms and policy were checked on 2026-11-17:

- [Azure App Service pricing](https://azure.microsoft.com/en-us/pricing/details/app-service/windows/)
- [App Service plans and pricing tiers](https://learn.microsoft.com/en-us/azure/app-service/overview-hosting-plans)
- [Azure SQL Database Free tier](https://learn.microsoft.com/en-us/azure/azure-sql/database/free-tier?view=azuresql)
- [Azure SQL Database Free tier FAQ](https://learn.microsoft.com/en-us/azure/azure-sql/database/free-tier-faq?view=azuresql)
- [Azure App Configuration pricing and quotas](https://azure.microsoft.com/en-us/pricing/details/app-configuration/)
- [.NET and .NET Core support policy](https://dotnet.microsoft.com/en-us/platform/support/policy/dotnet-core)
- [Deploy to App Service with GitHub Actions](https://learn.microsoft.com/en-us/azure/app-service/deploy-github-actions)
- [GitHub Actions billing](https://docs.github.com/en/billing/concepts/product-billing/github-actions)

Azure limits, regional availability, and pricing are subject to change; reconfirm them in the target subscription and Azure pricing calculator before provisioning.

## Implementation update (2026-11-17)

This assessment records the repository state before implementation. Subsequent deployment phases have now:

- Retargeted the API/backend projects to .NET 10, aligned EF Core/OpenAPI packages, and updated AutoMapper from 15.0.1 to patched 15.1.4 after NuGet reported high-severity advisories.
- Added JWT bearer validation, an Admin-only fallback policy, authenticated student registration bound to the token `sub`, explicit CORS, production exception/logging behavior, rate limiting, and liveness/readiness checks.
- Added four API integration tests and generated the initial EF migration plus an idempotent SQL script.
- Added Azure setup/deployment/security/validation/cost documentation and a manual-only OIDC deployment workflow.

No Azure resource, deployment, SQL database operation, or change to `ikigai.mk` has been performed. The generated migration has not been applied to any database.

The implementation decision differs from the initial recommended shape above in one respect: **the POC uses App Service application settings/environment variables instead of provisioning Azure App Configuration**. That is the simplest built-in ASP.NET Core configuration-provider path for one app and avoids another Azure resource. Add App Configuration only if shared centralized settings become necessary. The current decision, verified free-tier constraints, and prerequisites are recorded in [03-azure-prerequisites.md](./03-azure-prerequisites.md).

The deployment remains **POC only and not production-ready for real student/child data**. The owner must provide real OIDC issuer/audience/role configuration, and the application still lacks guardian/consent checks, tenant ownership, and per-student/per-club authorization. A real Azure SQL compatibility/migration/CRUD test is also pending because no Azure resources or credentials have been provided.
