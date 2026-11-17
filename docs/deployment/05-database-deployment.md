# Database deployment and recovery

## Model and migration status

The application uses `ApplicationDbContext` and EF Core 10 with the SQL Server provider. There were no migrations or snapshots before this deployment work. The initial migration was generated from the existing EF model, not hand-authored:

- Migration: `20261117014023_InitialCreate`
- Migration source: `src/SportsTeamManagement.Infrastructure/Data/Migrations/20261117014023_InitialCreate.cs`
- Model snapshot: `src/SportsTeamManagement.Infrastructure/Data/Migrations/ApplicationDbContextModelSnapshot.cs`
- Idempotent SQL: `scripts/deployment/001-initial-create.sql`

It creates the existing users, students, trainers, attendance, training session, event, registration, and belt promotion tables, their configured foreign keys, and unique/query indexes. The generated `Up` migration only creates schema objects. Its `Down` method drops the initial tables and is not to be run against data that must be retained.

**Apply this initial migration only to a new, empty POC database.** It is not a data-conversion or baseline migration for an existing database. Compare the existing schema and make a separately reviewed baseline/migration plan before pointing it at any database with tables or data.

No Azure database was created or modified. The migration and SQL script were generated without opening a database connection. SQL Server/Azure SQL compatibility, execution, CRUD, and restore are still deployment-time checks.

## Required tools and connection settings

1. Install/use the .NET 10 SDK.
2. Install EF tooling matching EF Core:

   ```powershell
   dotnet tool install --global dotnet-ef --version 10.0.15
   ```

3. The design-time context factory reads only the standard environment variable `ConnectionStrings__DefaultConnection`. It does not contain a LocalDB fallback or hard-coded password.
4. For local app development, use .NET User Secrets (already enabled for the API project):

   ```powershell
   dotnet user-secrets set "ConnectionStrings:DefaultConnection" "<local-development-connection-string>" --project .\src\SportsTeamManagement.API\SportsTeamManagement.API.csproj
   ```

   Do not place a real connection string in appsettings, launch settings, checked-in scripts, or chat. For Azure SQL local operator access, use a Microsoft Entra interactive connection string; the local machine cannot use the App Service's managed identity.
5. For design-time EF commands, enter the connection string at a hidden PowerShell prompt or set `ConnectionStrings__DefaultConnection` only in the current process. The application runtime on App Service uses its system-assigned identity and a passwordless connection string. Never print the variable.

The repeatable operator script prompts for the connection string without echoing it and applies the current migration:

```powershell
.\scripts\deployment\apply-migrations.ps1
```

Use an Entra administrator or a separate migration principal with DDL permissions. Do not grant the runtime app identity DDL permissions. The script runs `dotnet ef database update`; review the target server/database in the connection dialog/provider before confirming.

To inspect or regenerate a reviewed script:

```powershell
$secure = Read-Host "Connection string for design-time EF" -AsSecureString
$pointer = [IntPtr]::Zero
$previousConnectionString = [Environment]::GetEnvironmentVariable("ConnectionStrings__DefaultConnection", "Process")
try {
    $pointer = [Runtime.InteropServices.Marshal]::SecureStringToBSTR($secure)
    $env:ConnectionStrings__DefaultConnection = [Runtime.InteropServices.Marshal]::PtrToStringBSTR($pointer)
    dotnet ef migrations list --project .\src\SportsTeamManagement.Infrastructure\SportsTeamManagement.Infrastructure.csproj --startup-project .\src\SportsTeamManagement.API\SportsTeamManagement.API.csproj
    dotnet ef migrations script --idempotent --project .\src\SportsTeamManagement.Infrastructure\SportsTeamManagement.Infrastructure.csproj --startup-project .\src\SportsTeamManagement.API\SportsTeamManagement.API.csproj --output .\scripts\deployment\reviewed-migrations.sql
}
finally {
    if ($null -eq $previousConnectionString) {
        Remove-Item Env:\ConnectionStrings__DefaultConnection -ErrorAction SilentlyContinue
    }
    else {
        $env:ConnectionStrings__DefaultConnection = $previousConnectionString
    }
    if ($pointer -ne [IntPtr]::Zero) {
        [Runtime.InteropServices.Marshal]::ZeroFreeBSTR($pointer)
    }
    $secure.Dispose()
}
```

Review the generated SQL before applying. Do not use `EnsureCreated`, do not call `Database.Migrate()` automatically at API startup, and do not run `dotnet ef database update` as part of the production web app startup.

## Azure database creation and network access

Follow [04-azure-resource-setup.md](./04-azure-resource-setup.md) to create a **new** SQL Free database with `useFreeLimit=true` and `AutoPause`. Create the database in the same region as the API if eligible, set the Entra SQL administrator, create the App Service system identity, and configure narrow firewall rules. A managed identity does not bypass the SQL firewall.

The production app connection key is `ConnectionStrings__DefaultConnection`. Its value uses:

- `Server=tcp:<sql-server>.database.windows.net,1433`
- `Database=<new-database>`
- `Authentication=Active Directory Managed Identity`
- `Encrypt=True;TrustServerCertificate=False`
- A bounded connection timeout

There is no SQL username/password. The App Service identity receives only `db_datareader` and `db_datawriter` for runtime access. Keep schema changes under a separate migration operator.

## Connectivity verification

After migration and deployment:

1. Check the App Service log stream for a startup failure, without enabling request-body or EF parameter-value logging.
2. Request `https://<app-name>.azurewebsites.net/health/live`; it should return HTTP 200 without touching SQL.
3. Request `/health/ready`; it should return HTTP 200 and `{"status":"Healthy"}` only when the SQL server/database is reachable. Failure returns an unhealthy status without database details or connection strings.
4. In Azure SQL metrics, verify compute usage and free amount remaining. SQL free databases can pause at allowance exhaustion; the next request may be delayed or fail while it resumes.
5. Run a small, approved CRUD smoke test using non-sensitive test data. Remove that test data when complete.

## Backup, export, and restore

Azure SQL Database takes automated backups. Point-in-time restore creates a **new** database; it never overwrites the source. Retention and backup redundancy are configurable and must be checked on the actual database. Free-tier eligibility does not imply a production recovery SLA or long-term retention.

### Verify automatic backup configuration

In the Azure portal, open the database's **Data management > Backups** settings and verify the current point-in-time retention and backup storage redundancy. Review the current [automated backup documentation](https://learn.microsoft.com/azure/azure-sql/database/automated-backups-overview). Retention beyond defaults, long-term retention, or greater redundancy can incur charges.

### Create a portable export

For an independent export, install Microsoft's [SqlPackage](https://learn.microsoft.com/sql/tools/sqlpackage/sqlpackage-download). Run an export using an interactive Entra connection (no SQL password); replace every placeholder and save the BACPAC to an approved encrypted location:

```powershell
SqlPackage /Action:Export /SourceConnectionString:"Server=tcp:<sql-server>.database.windows.net,1433;Initial Catalog=<database>;Authentication=Active Directory Interactive;Encrypt=True;TrustServerCertificate=False;" /TargetFile:"<protected-backup-path>\SportsTeamManagement.bacpac"
```

BACPAC contains student data. Encrypt it at rest, restrict access, record the export date, and do not commit it. An Azure Blob Storage destination is optional and may add storage/transaction charges; it is not part of the Free-tier estimate.

### Point-in-time restore

Use the portal's **Restore** page or current Azure CLI `az sql db restore` command to restore to a new database at a selected timestamp. Confirm the target SKU and pricing before submitting. Restore creates a new database and can be billed at normal rates; do not assume the restore remains in the Free offer.

```powershell
az sql db restore --resource-group <resource-group> --server <sql-server-name> --name <source-database> --dest-name <restored-database-name> --time "<UTC-time-in-ISO-8601>"
```

Verify the restored database before redirecting the app. Update the App Service connection setting only after approval and testing. Keep or delete the original/restored resources only after an explicit data-retention decision.

### BACPAC restore/import

Import to a new, empty target database. A target SQL database may be billable; check the selected SKU and Free eligibility before import:

```powershell
SqlPackage /Action:Import /SourceFile:"<protected-backup-path>\SportsTeamManagement.bacpac" /TargetConnectionString:"Server=tcp:<target-sql-server>.database.windows.net,1433;Initial Catalog=<new-target-database>;Authentication=Active Directory Interactive;Encrypt=True;TrustServerCertificate=False;"
```

Test export/import periodically with synthetic or approved data. This procedure is not a substitute for a tested production backup/recovery plan.

## Safe maintenance of the migration chain

- Add a new EF migration for each reviewed schema change; never edit a migration already applied to an environment.
- Generate an idempotent script and review all `ALTER`, `DROP`, data-copy, and default-value operations.
- Apply to a disposable development database first; record migration ID and verification results.
- Back up before any future data migration. Restore to a new target for tests; never overwrite production.
- Keep migration operator credentials out of GitHub workflow secrets and the app runtime.

References: [EF Core migrations](https://learn.microsoft.com/ef/core/managing-schemas/migrations/), [Azure SQL restore](https://learn.microsoft.com/azure/azure-sql/database/recovery-using-backups), [SQL Free tier](https://learn.microsoft.com/azure/azure-sql/database/free-tier).
