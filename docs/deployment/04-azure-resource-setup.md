# Azure resource setup

**No commands in this document have been executed.** Run each step only after confirming the tenant, subscription, region, names, current Free eligibility, and cost summary.

## Resource inventory

| Resource | Service and suggested configuration | Region / eligibility | Limits and possible charges |
| --- | --- | --- | --- |
| Resource group | Azure Resource Manager resource group dedicated to this POC | Select one region for deployment; the group itself is not a workload region | No direct charge. Deleting a group deletes all contained resources, so do not reuse a shared group. |
| SQL logical server | Azure SQL logical server with Microsoft Entra-only authentication and public endpoint protected by narrow firewall rules | Same region as the SQL database; server is not itself the free database offer | No separate server compute charge, but firewall/public network and Entra admin configuration must be reviewed. |
| SQL database | New General Purpose, serverless, 2-vCore configuration, 32-GB max, free limit enabled, `AutoPause` at limit | Check offer/region in the SQL hub; offer excludes Azure for Students Starter | 100,000 vCore-seconds, 32 GB data, and 32 GB backup storage monthly per database; no SLA. A wrong SKU or overage option can bill. |
| App Service plan | Linux Free F1 | Check selected region and subscription availability | Shared compute, daily CPU/storage quotas, no scale-out/SLA; plan upgrade incurs compute charges. |
| App Service web app | .NET 10, native Linux App Service, generated Azure hostname, HTTPS only | Same region as its plan; verify `DOTNETCORE:10.0` is available before creation | F1 limits apply. Do not add a custom domain or paid plan for this POC. |
| Managed identity | System-assigned identity on the web app | Created with the app; same tenant as SQL | No separate identity resource/secret; deleting the app deletes the identity. |
| App settings | ASP.NET Core environment/configuration values on the web app | Not region-specific | Encrypted at rest by App Service. A separately provisioned App Configuration or Key Vault service is not required for this managed-identity setup. |

## 1. Confirm Azure context and create a dedicated group

Install/update the [Azure CLI](https://learn.microsoft.com/cli/azure/install-azure-cli), sign in, and inspect the active context. Replace every placeholder with your own values; do not paste credentials into chat or source control.

```powershell
az login
az account show --output table
az account set --subscription <subscription-id>
az account show --query "{subscription:name, tenant:tenantId, id:id}" --output table

az group create --name <resource-group> --location <region>
```

Portal alternative: search **Resource groups > Create**, select the intended subscription and region, enter a new dedicated group name, review, and create.

The region must be supported by both Free offers. Review the portal cost summary before continuing.

## 2. Create the SQL logical server with Entra-only authentication

Use a Microsoft Entra administrator account that you control. The CLI option below avoids provisioning a SQL username/password. Obtain the actual administrator display name and object ID from your Entra directory; the values below are placeholders, not real IDs.

```powershell
az sql server create --name <sql-server-name> --resource-group <resource-group> --location <region> --enable-ad-only-auth --external-admin-principal-type User --external-admin-name "<entra-admin-display-name>" --external-admin-sid <entra-admin-object-id>
```

Portal alternative: **Create a resource > SQL Database**, select the intended subscription/resource group/region, create or select a logical server, and set its Microsoft Entra administrator. Enable Entra-only authentication. Avoid setting or saving a SQL administrator password.

## 3. Create and verify the SQL Free database

Create a **new** database. The CLI command must include both `--use-free-limit true` and `--free-limit-exhaustion-behavior AutoPause`; omitting the first creates a billable serverless database.

```powershell
az sql db create --name <database-name> --server <sql-server-name> --resource-group <resource-group> --edition GeneralPurpose --family Gen5 --capacity 2 --compute-model Serverless --use-free-limit true --free-limit-exhaustion-behavior AutoPause
az sql db show --name <database-name> --server <sql-server-name> --resource-group <resource-group> --query "{state:status, freeLimit:useFreeLimit, exhaustion:freeLimitExhaustionBehavior, maxSize:maxSizeBytes, sku:sku}" --output json
```

Confirm the output shows `useFreeLimit: true` and `freeLimitExhaustionBehavior: AutoPause`. Stop if it does not. In the portal, the equivalent is **Azure SQL hub > Create database > Start free**; verify the Cost summary and free-limit behavior before choosing **Create**.

## 4. Create the Linux F1 App Service

Confirm `F1` and the .NET 10 runtime are currently available in the chosen region. Resource names must be globally unique as applicable.

```powershell
az appservice plan create --name <app-service-plan> --resource-group <resource-group> --location <region> --sku F1 --is-linux
az webapp create --name <app-name> --resource-group <resource-group> --plan <app-service-plan> --runtime "DOTNETCORE:10.0"
az webapp update --name <app-name> --resource-group <resource-group> --https-only true
az webapp identity assign --name <app-name> --resource-group <resource-group>
```

If `az webapp create` reports that the runtime or F1 SKU is unavailable, stop and recheck the portal/region. Do not silently substitute a paid SKU. The portal equivalent is **App Services > Create > Publish: Code > Runtime stack: .NET 10 > Operating System: Linux > Pricing plan: Free F1**.

The web app hostname is `https://<app-name>.azurewebsites.net`. Do not add a custom domain.

Portal alternative: **App Services > Create > Web App**, choose **Code**, Linux, a supported .NET 10 runtime, the F1 plan, and the same region; review the plan/SKU before creating the app. In the app, set **Settings > Configuration > HTTPS Only** to **On** and save.

## 5. Restrict SQL firewall access

The F1 plan uses public networking; this POC does not provision VNet integration or a private endpoint. Do not enable the SQL firewall option that allows every Azure resource (`0.0.0.0`).

For operator access during setup, add only the operator's current public IP as a temporary SQL firewall rule. Remove it when no longer needed:

```powershell
az sql server firewall-rule create --name AllowOperator --server <sql-server-name> --resource-group <resource-group> --start-ip-address <operator-public-ip> --end-ip-address <operator-public-ip>
```

Portal alternative: on the logical SQL server, open **Security > Networking**, add a firewall rule with the same operator start/end IPv4 address, and do not enable broad Azure-service access. Remove the operator rule after setup.

For app access, allow only the App Service's possible outbound IP addresses. These are platform-managed and can change when the app/plan changes; refresh the rules after a change. Obtain the list:

```powershell
az webapp show --name <app-name> --resource-group <resource-group> --query possibleOutboundIpAddresses --output tsv
```

Create one narrow firewall rule per returned IP (start IP = end IP). Do not use the `Allow Azure services and resources to access this server` shortcut for this student-data API. Recheck the list after scaling, moving, or recreating the app. A paid private networking design is required for private-only SQL connectivity.

## 6. Grant the web app identity database access

Get the app identity's principal ID and confirm it is a system-assigned identity:

```powershell
az webapp identity show --name <app-name> --resource-group <resource-group> --query "{type:type, principalId:principalId, tenantId:tenantId}" --output json
```

Portal alternative: open the web app, choose **Settings > Identity > System assigned**, set **Status** to **On**, and save. Copy the principal (object) ID from the saved identity page.

Connect to the target database as the SQL server's Entra administrator (Azure Data Studio/SSMS/query editor with Microsoft Entra authentication), then run the following against **the application database**, substituting the exact web app identity display name:

```sql
CREATE USER [<app-name>] FROM EXTERNAL PROVIDER;
ALTER ROLE db_datareader ADD MEMBER [<app-name>];
ALTER ROLE db_datawriter ADD MEMBER [<app-name>];
```

Do not grant `db_owner` or schema-change permissions to the runtime identity. If the SQL server cannot resolve the principal through Entra, follow Microsoft's [managed identity SQL tutorial](https://learn.microsoft.com/azure/app-service/tutorial-connect-msi-sql-database) and [CREATE USER reference](https://learn.microsoft.com/sql/t-sql/statements/create-user-transact-sql?view=azuresqldb-current); database administrators can use the documented `SID`/`TYPE=E` form with the actual principal object ID. Do not invent or copy a sample object ID.

Use a separate, temporary migration operator identity with DDL permissions to apply the initial migration. Do not give the app runtime identity migration rights.

## 7. App Service settings

Configure the required API values as App Service **App settings** (not the portal's typed SQL connection-string section, so the exact ASP.NET key is preserved). See [06-api-deployment.md](./06-api-deployment.md) for all required keys.

Portal: open **App Services > <app> > Settings > Environment variables > App settings**, add the listed key/value pairs, then select **Apply**. Use **App settings**, including for `ConnectionStrings__DefaultConnection`; do not use the separately typed connection-string section.

```powershell
az webapp config appsettings set --name <app-name> --resource-group <resource-group> --settings ASPNETCORE_ENVIRONMENT=Production ASPNETCORE_FORWARDEDHEADERS_ENABLED=true ConnectionStrings__DefaultConnection="<managed-identity SQL connection string>" Authentication__Authority="<actual HTTPS issuer>" Authentication__Audience="<actual API audience>" Authentication__RoleClaimType="roles"
```

Set `Cors__AllowedOrigins__0` only when a browser frontend has an exact HTTPS origin. Do not add `ikigai.mk` without an approved frontend origin; the existing website is not part of this deployment. A native MAUI client does not use browser CORS.

The connection string should use `Authentication=Active Directory Managed Identity`, TLS encryption, and `TrustServerCertificate=False`; it contains no password. Never commit an actual connection string, issuer secret, client secret, or token. App Service settings changes restart the app.

## 8. Final resource check

Before deploying, verify:

- The active subscription and resource group are the intended ones.
- App Service is Linux F1, not a paid plan, and uses the generated hostname.
- SQL is a new Free General Purpose serverless database with the free limit enabled and `AutoPause`.
- SQL has an Entra administrator and narrow firewall rules; no `0.0.0.0` allow-all rule exists.
- The web app has a system identity and a least-privilege contained database user.
- HTTPS-only is enabled; `ASPNETCORE_ENVIRONMENT=Production`.
- All required auth/database settings are present and contain no committed credentials.
- No migration or cloud deployment has happened without your approval.

References: [Azure SQL Free tier](https://learn.microsoft.com/azure/azure-sql/database/free-tier), [App Service plans](https://learn.microsoft.com/azure/app-service/overview-hosting-plans), [managed identity to SQL](https://learn.microsoft.com/azure/app-service/tutorial-connect-msi-sql-database).
