# .NET Framework Upgrade Assessment and Change Log

**Assessment date:** 2026-11-17  
**Decision:** Target the API and its backend class libraries on .NET 10 LTS. Keep the MAUI project unchanged in this backend upgrade and handle its framework/workload migration separately.

## Summary

The API and backend projects have been retargeted to `net10.0`. EF Core and Microsoft ASP.NET OpenAPI references were aligned to the latest .NET 10 servicing versions available at assessment time, **10.0.15**. Swashbuckle was upgraded to **10.3.1**, which provides a .NET 10 asset. AutoMapper is **15.1.4**: the original 15.0.1 dependency was updated after NuGet audit identified high-severity advisories.

The repository root `global.json` requests SDK 10.0.100 with `latestFeature` roll-forward, so an installed .NET 10 SDK feature band is selected. Install a supported .NET 10 SDK before running local builds; CI installs .NET 10 explicitly.

The MAUI project was deliberately not changed. Its current target is `net9.0-*`, and its resolved MAUI packages are 9.0.111; the machine has MAUI 7 workloads, not a supported current MAUI workload. A portable .NET 10 SDK was later extracted into session-local storage for backend validation, but no MAUI workload/platform toolchain was installed. .NET MAUI 9 is out of support, so leaving it unchanged is a short-term compatibility decision, not a recommended long-term mobile target. Upgrade and validate it in a separate mobile-focused change with an installed, supported MAUI workload and platform toolchains.

**Backend build validation:** the API and test project restored and built successfully with the portable .NET SDK 10.0.404. Four API integration tests passed. The system-wide `dotnet` installation remains on SDK 5/7; the .NET 10 SDK used for validation is not a machine-wide install. MAUI and full-solution builds have not been validated.

## Repository findings before the upgrade

| Project | Original TFM | Package references relevant to the upgrade |
| --- | --- | --- |
| `SportsTeamManagement.API` | `net9.0` | `Microsoft.AspNetCore.OpenApi` 9.0.4; `Microsoft.EntityFrameworkCore.Design` 9.0.9; `Swashbuckle.AspNetCore` 9.0.6 |
| `SportsTeamManagement.Application` | `net9.0` | `AutoMapper` 15.0.1 |
| `SportsTeamManagement.Domain` | `net9.0` | None |
| `SportsTeamManagement.Infrastructure` | `net9.0` | EF Core, SQL Server provider, and Tools 9.0.9 |
| `SportsTeamManagement.Mobile` | `net9.0-android`, `net9.0-ios`, `net9.0-maccatalyst`; adds `net9.0-windows10.0.19041.0` on Windows | `Microsoft.Maui.Controls` uses `$(MauiVersion)`; `Microsoft.Extensions.Logging.Debug` 9.0.0 |

The existing resolved assets confirm the package versions above, including MAUI 9.0.111. The backend projects are single-targeted and reference each other through project references. The MAUI project does not reference the backend projects, so keeping its TFM separate does not introduce a project-reference TFM incompatibility.

## Compatibility review and changes made

| Package/project | Change | Compatibility finding and reason |
| --- | --- | --- |
| API, Application, Domain, Infrastructure TFMs | `net9.0` -> `net10.0` | .NET 10 is the selected supported LTS target. EF Core 10 requires the .NET 10 SDK and runtime. |
| `Microsoft.AspNetCore.OpenApi` | 9.0.4 -> 10.0.15 | NuGet package 10.0.15 has a `net10.0` asset and is aligned with the API's target framework. |
| `Microsoft.EntityFrameworkCore.Design` | 9.0.9 -> 10.0.15 | Same EF Core major and servicing release as the other EF packages. |
| `Microsoft.EntityFrameworkCore` | 9.0.9 -> 10.0.15 | EF Core 10.0.15 is explicitly a .NET 10 package. |
| `Microsoft.EntityFrameworkCore.SqlServer` | 9.0.9 -> 10.0.15 | SQL Server provider version aligned with EF Core 10.0.15; NuGet metadata targets `net10.0`. |
| `Microsoft.EntityFrameworkCore.Tools` | 9.0.9 -> 10.0.15 | Tooling version aligned with EF Core 10.0.15 and depends on matching EF Design packages for `net10.0`. |
| `Swashbuckle.AspNetCore` | 9.0.6 -> 10.3.1 | Package metadata includes a `net10.0` dependency group. Version 10 is a major version and its breaking changes were reviewed; the current API uses only standard `AddSwaggerGen`, `UseSwagger`, and `UseSwaggerUI` calls. |
| `AutoMapper` | 15.0.1 -> 15.1.4 | NuGet audit reported three high-severity advisories affecting 15.0.1; the package was moved to the patched 15.1.4 release. The API still uses the existing mapping pattern. A fresh NuGet vulnerability check reported no vulnerable packages in the API graph. |
| MAUI packages and target frameworks | No change | MAUI is an independent mobile app, and the installed SDK/workloads cannot build or validate a supported MAUI target. See the separate strategy below. |

The selected Microsoft 10.0.15 packages and Swashbuckle 10.3.1 were checked against their NuGet package metadata as well as Microsoft release documentation. Versions should be reviewed again during future servicing; do not mechanically bump every dependency to its latest major.

## Breaking changes reviewed

### .NET and ASP.NET Core

- .NET 10 introduces runtime, SDK, library, ASP.NET Core, and C# 14 changes. The application uses ordinary ASP.NET Core controller/startup patterns and has no use of APIs identified in the reviewed breaking-change list that require an immediate source edit.
- The application has no custom container, reflection/trimming configuration, or platform-specific backend code to carry forward.
- The SDK/build environment must be upgraded as well as the project TFM. An older SDK cannot restore/build the `net10.0` projects.

### EF Core 9 to 10

- EF Core 10 is an LTS release, requires the .NET 10 SDK/runtime, and is supported through **2028-11-10**.
- EF tools now require `--framework` when the *target project* is multi-targeted. These backend projects use a single TFM, so the change does not currently apply. Keep that in mind if a later backend project multi-targets.
- EF Core 10's SQL Server provider depends on Microsoft.Data.SqlClient 6.1.7. EF's breaking-change notes call out a SqlClient broker dependency beginning in 6.1.6 and its potential effect on publish size. No direct SqlClient override was added. Revisit the final dependency graph after restore, especially when choosing Entra ID/managed-identity authentication; do not override the provider's dependency without validating that authentication path.
- EF Core 10 injects an `Application Name` into a connection string if one is not specified. This is generally benign; it can change SqlClient connection-pool identity in mixed EF/ADO.NET or `TransactionScope` scenarios. No separate non-EF data access path or `TransactionScope` was found. Set a stable application name if a later integration introduces either scenario.
- EF Core 10 can use SQL Server's native JSON type for JSON mappings when configured with `UseAzureSql` or a compatibility level of 170 or higher. This application currently uses `UseSqlServer`, and the reviewed entities/configurations contain no JSON/complex-property mappings. No current model change is indicated. Recheck this before enabling Azure SQL-specific configuration or adding JSON fields.
- EF Core 10 changes parameterized-collection translation. The reviewed repository queries do not use LINQ `Contains` against parameterized collections, so no affected query was identified. Recheck query behavior and generated SQL in integration tests.
- No EF migrations existed before this upgrade. A later deployment phase generated an initial migration from the current EF model and an idempotent SQL script; these have not been applied to a live SQL Server/Azure SQL database.

### Swashbuckle 9 to 10

- Swashbuckle 10 is a major release, depends on OpenAPI.NET 2.x, and changes some public OpenAPI types/APIs. Its migration guidance recommends moving from 9.0.6 before upgrading; the repository was already on 9.0.6.
- The current API only calls `AddSwaggerGen`, `UseSwagger`, and `UseSwaggerUI`; it has no custom schema/operation filters or `WithOpenApi()` calls. No source change was identified for those standard calls.
- OpenAPI 3.1 generation is opt-in in Swashbuckle 10, so the upgrade does not intentionally change the document version. Do not add a `WithOpenApi()` call; the Swashbuckle 10 release no longer supports that ASP.NET Core method.
- The package upgrade compiled successfully with the portable .NET 10 SDK. A standalone Swagger endpoint smoke test and real SQL Server/Azure SQL migration execution remain pending.

## MAUI strategy (separate from the backend upgrade)

The MAUI project is currently on .NET 9 and resolves MAUI 9.0.111, with `Microsoft.Extensions.Logging.Debug` 9.0.0. Microsoft's MAUI support policy lists MAUI 9 as out of support since **2026-05-12**. As of the assessment date, MAUI 10 is supported through **2027-05-11**, and MAUI 11 is the current supported release through **2028-05-09**.

The machine-wide installation has .NET SDKs 5.0.408 and 7.0.400 and MAUI 7.0.92 workloads (`maui-android`, `maui-ios`, `maui-maccatalyst`, and `maui-windows`). A .NET 10.0.404 SDK was made available only in session-local storage; it has no matching MAUI workload. Consequently, changing the MAUI project's `TargetFrameworks`, its workload-selected `$(MauiVersion)`, or its logging package without a supported toolchain and platform builds would risk breaking the existing mobile project.

**Recommended separate mobile work:** install a supported .NET SDK and matching MAUI workload through Visual Studio Installer (or the current documented .NET MAUI installation process), then choose the supported MAUI release based on platform and release cadence. MAUI 11 is the longer-supported current option as of this assessment; it is not the backend's .NET 10 LTS TFM, but the mobile project is an independent client and does not reference the backend assemblies. Upgrade the MAUI target frameworks, workload-managed MAUI packages, and Microsoft.Extensions package versions together, then build/test each intended platform. On Windows, Android and Windows targets can be validated locally; full iOS/Mac Catalyst build/sign/deploy requires a compatible Mac/Xcode toolchain. Check supported Android API, Xcode, Windows SDK, and minimum OS versions at that time.

The mobile project was not retargeted or package-upgraded in this change. Its current .NET 9/MAUI 9 target is unsupported, so it should not be considered a supported release target until the separate mobile upgrade is completed.

## Files changed

- Framework-upgrade changes: `global.json` and the API, Application, Domain, and Infrastructure project files.

Later deployment phases also added API configuration/security code, tests, a migration, SQL/deployment scripts, workflow, and deployment documentation. The MAUI project and existing website remain unchanged. See [01-architecture-assessment.md](./01-architecture-assessment.md) for the architecture and complete deployment status.

## Validation status and required next checks

## Subsequent deployment-phase validation

- Restore and Release build of the API and referenced backend projects: **passed** with SDK 10.0.404.
- API integration tests: **4 passed** (anonymous health, anonymous API denial, non-admin denial, explicit CORS allow/deny).
- NuGet audit for the API dependency graph after the AutoMapper patch: **no known vulnerable packages reported**.
- EF initial migration generation and idempotent SQL script generation: **passed** using the current EF Core 10.0.15 model and a design-time-only connection string; no database was contacted.
- Real JWT issuer/role claim validation, Swagger runtime smoke test, SQL Server/Azure SQL migration execution, database health, CRUD, and restore: **not tested** because no identity-provider configuration or database exists.
- MAUI and full-solution builds: **not run** because the machine lacks a supported MAUI workload; its project remains unchanged.

Do not treat the backend test-host authentication scheme as a real OIDC-provider test. Do not build the whole solution as the sole check until the separate MAUI workload upgrade is resolved.

## References checked (2026-11-17)

- [.NET and .NET Core support policy](https://dotnet.microsoft.com/en-us/platform/support/policy/dotnet-core)
- [.NET 10 overview](https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-10/overview)
- [Breaking changes in .NET 10](https://learn.microsoft.com/en-us/dotnet/core/compatibility/10)
- [EF Core 10 what's new](https://learn.microsoft.com/en-us/ef/core/what-is-new/ef-core-10.0/whatsnew)
- [EF Core 10 breaking changes](https://learn.microsoft.com/en-us/ef/core/what-is-new/ef-core-10.0/breaking-changes)
- [Swashbuckle v10 release notes](https://github.com/domaindrivendev/Swashbuckle.AspNetCore/releases/tag/v10.0.0)
- [Swashbuckle v10 migration guide](https://github.com/domaindrivendev/Swashbuckle.AspNetCore/blob/master/docs/migrating-to-v10.md)
- [MAUI support policy](https://dotnet.microsoft.com/en-us/platform/support/policy/maui)
- [MAUI .NET 10 release information](https://learn.microsoft.com/en-us/dotnet/maui/whats-new/dotnet-10?view=net-maui-10.0)
- [MAUI installation requirements](https://learn.microsoft.com/en-us/dotnet/maui/get-started/installation?view=net-maui-10.0)
- [NuGet: EF Core 10.0.15](https://www.nuget.org/packages/Microsoft.EntityFrameworkCore/10.0.15)
- [NuGet: SQL Server provider 10.0.15](https://www.nuget.org/packages/Microsoft.EntityFrameworkCore.SqlServer/10.0.15)
- [NuGet: ASP.NET Core OpenAPI 10.0.15](https://www.nuget.org/packages/Microsoft.AspNetCore.OpenApi/10.0.15)
- [NuGet: Swashbuckle 10.3.1](https://www.nuget.org/packages/Swashbuckle.AspNetCore/10.3.1)

