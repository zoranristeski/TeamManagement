# Azure prerequisites and free-tier decision

**Reviewed:** 2026-11-17  
**Target:** Development / proof of concept only. No Azure resource has been created.

## Recommended initial architecture

| Component | Selected option | Free availability and limitations |
| --- | --- | --- |
| API hosting | Azure App Service on Linux, Free F1 plan | The current Linux F1 pricing table lists shared compute with 60 CPU minutes/app/day, 1 GB RAM, and 1 GB storage. It is not an SLA-backed or production hosting plan; see [Linux App Service pricing](https://azure.microsoft.com/pricing/details/app-service/linux/) and the [App Service plan comparison](https://learn.microsoft.com/azure/app-service/overview-hosting-plans). |
| Database | A new Azure SQL Database, General Purpose serverless, free limit enabled, exhaustion behavior `AutoPause` | Current offer: 100,000 vCore-seconds, 32 GB data, and 32 GB backup storage per database per month; up to 10 free databases per subscription. The free offer has no SLA and is for new development/sample databases. Do not convert or restore an existing database into the offer. Confirm the current offer and region in the Azure portal before creation: [Azure SQL free tier](https://learn.microsoft.com/azure/azure-sql/database/free-tier). |
| SQL logical server | Azure SQL logical server, Microsoft Entra-only authentication | The logical server is required to host the SQL Database and configure its Entra administrator and firewall. Use Entra-only authentication; no SQL administrator password is needed for the documented CLI creation path. |
| App-to-SQL identity | App Service system-assigned managed identity | Azure manages its credentials. Create a contained database user for the identity and grant only runtime data permissions. Managed identity itself has no separately metered request charge; verify the selected App Service SKU and region support it in the portal. See [App Service managed identity](https://learn.microsoft.com/azure/app-service/overview-managed-identity) and the [App Service to Azure SQL tutorial](https://learn.microsoft.com/azure/app-service/tutorial-connect-msi-sql-database). |
| Configuration | App Service application settings/environment variables | No separate Azure App Configuration resource is required for this single-app POC. App Service settings are encrypted at rest and override ASP.NET Core settings. This avoids an additional dependency/resource and preserves the standard configuration-provider chain. |
| Source and delivery | GitHub repository and GitHub Actions with OIDC | GitHub Actions usage may be subject to GitHub plan quotas. Azure deployment uses a federated identity, not a stored client secret. |

No custom domain, Docker image, Application Insights workspace, Key Vault, private endpoint, NAT gateway, or Azure App Configuration resource is needed for the initial POC. Those can add cost or configuration. The existing `ikigai.mk` website is outside this deployment and is not changed.

## Current free-offer findings

### Azure App Service

- The Free F1 option is still documented, but it uses shared infrastructure, has no SLA, and cannot scale out. Free/Shared plans have a daily CPU quota and low local storage; the exact published limits and features can change. Check the linked limits page and the plan picker for the selected OS and region immediately before provisioning.
- The current Linux F1 published limits are **60 CPU minutes per app per day, 1 GB RAM, and 1 GB storage**. This is a daily CPU quota, not an entitlement to 60 minutes every month; unused daily quota does not make the shared plan reliable.
- The generated `*.azurewebsites.net` hostname and HTTPS-only configuration meet the no-domain requirement. Do not buy an App Service domain or certificate.
- A free plan is suitable only for a low-traffic demonstration. It is not a suitable production target for student/child records or reliable service availability.
- Features such as dedicated workers, private networking, deployment slots, and higher availability require a higher plan. A private endpoint/VNet path to SQL is not assumed free.
- The recommended system-assigned identity connection requires no paid identity resource. If the selected F1/region cannot enable or use it, the preferred alternative is an App Service plan that supports managed identity (likely a paid tier) while keeping SQL Entra-only. A strictly free but weaker fallback would require a dedicated SQL-authenticated database principal and a password in encrypted App Service settings; that changes the server authentication design and is not implemented here. Do not silently switch to it. If Entra-only authentication is mandatory and managed identity is unavailable, stop and obtain approval for the paid plan.

### Azure SQL Database Free

- The current offer is available for most Azure subscription types, including existing eligible subscriptions; Microsoft Azure for Students Starter is excluded. It is not restricted to a new Azure free-account signup.
- The offer applies to a new free-tier database. It cannot be used to convert an existing database or as the destination of a restore.
- Current limits are per database and monthly: 100,000 vCore-seconds, 32 GB data, and 32 GB backup storage; up to 10 databases per subscription. The portal is the authoritative place to confirm offer eligibility and regional availability for the selected subscription.
- Use `AutoPause` at the free compute limit. Do not select `BillOverUsage`; that setting permits paid compute overages.
- Azure SQL's managed engine supports SQL Server compatibility for this application's EF Core SQL Server provider. Actual migration and CRUD compatibility remain unverified until deployed to a real Azure SQL database.

### Networking allowance

- Azure's current bandwidth pricing page lists the **first 100 GB/month of internet egress as free** for Azure customers; outbound transfer above that is billed by source zone. Inbound transfer is free. This is a general Azure bandwidth allowance, not an App Service F1-specific quota; confirm current pricing for the subscription.
- Azure's bandwidth FAQ states that data transfer between services in the same Azure region (including this API and SQL in the same region) has no additional transfer charge. Inter-region data transfer can be billed.
- The F1 app uses public networking. SQL firewall access still must be restricted to the App Service's possible outbound IPs. Private endpoints/VNet features are not part of this free design.

## Region and eligibility decision

1. Sign in to the intended Azure tenant and subscription; confirm the subscription is active and eligible.
2. In the Azure portal, check that **App Service / Linux / Free F1** can be selected in the intended region.
3. In the Azure SQL hub's **Start free** flow, check that the subscription and region offer the free database configuration.
4. Prefer one region where both services are available, ideally close to the initial users. Do not assume the same region is eligible for both offers. The offer selector and current regional service availability are authoritative.
5. Before creating anything, review the cost summary and selected SKU. Stop if SQL `useFreeLimit` is not enabled or exhaustion is not `AutoPause`.

No region, subscription ID, resource name, tenant ID, or resource ID is preselected here.

## Expected costs and cost traps

The target Azure usage charge is **$0 only while each selected Free allowance and configuration remains eligible and within its limits**. This is not a zero-cost guarantee.

Potential charges include:

- Any App Service plan upgrade from F1 or a different SKU selected accidentally.
- SQL paid compute if `useFreeLimit` is omitted, the free offer is disabled, or `BillOverUsage` is selected.
- Paid SQL usage or storage/backup settings outside the current free allowance.
- Restoring a database (restore creates a new database and may be billed at its selected service tier); LTR backup storage.
- Outbound internet/cross-region data transfer or other separately metered networking.
- Optional Log Analytics/Application Insights ingestion, Key Vault, Azure App Configuration, storage for exported backups, private endpoints, VNet integration, or NAT.
- GitHub Actions minutes beyond the GitHub account's included quota.
- The selected external OIDC provider (Auth0 or another owner-selected service) has its own pricing/quotas; its terms are not part of the Azure Free estimate.

Use the [Azure pricing calculator](https://azure.microsoft.com/pricing/calculator/) and a Cost Management budget/alert before creating resources. A budget sends alerts; it does not cap or stop charges.

## Identity and application prerequisites

- A supported OpenID Connect provider is required. The existing `User.Auth0Id` field suggests Auth0, but the repository contains no Authority, Audience, client registration, role-claim configuration, or provider credentials. Supply the actual HTTPS issuer and API audience; do not copy example values into production.
- The API fails at startup if its database connection string or authentication settings are missing. Configure these before deploying.
- The JWT must include a role claim that maps to `Admin` for the default protected API policy. The default claim name is `roles`; configure `Authentication__RoleClaimType` if the provider emits a different claim name. Role values are case-sensitive.
- An Entra administrator must be assigned to the SQL logical server. The App Service identity also needs a contained database user with runtime data permissions.
- Create an empty, new database for the initial migration. Do not point the initial migration at a database containing existing tables or production data.
- The deployment workflow expects a GitHub repository and a federated Entra application/service principal with narrowly scoped Website Contributor access to the App Service.

## Azure App Configuration decision

For this one API, App Service application settings are the selected ASP.NET Core environment provider. Use the exact key `ConnectionStrings__DefaultConnection` and the `Authentication__...` / `Cors__...` keys documented in [06-api-deployment.md](./06-api-deployment.md). The API does not require the separate Azure App Configuration service. Add that service only if settings must be centrally managed across multiple applications or updated without App Service restarts; evaluate its current tier and any Key Vault reference costs at that time.

## Official references

- [App Service plans and tiers](https://learn.microsoft.com/azure/app-service/overview-hosting-plans)
- [App Service subscription limits](https://learn.microsoft.com/azure/azure-resource-manager/management/azure-subscription-service-limits#azure-app-service-limits)
- [Azure SQL Database Free tier](https://learn.microsoft.com/azure/azure-sql/database/free-tier)
- [Azure bandwidth pricing and egress allowance](https://azure.microsoft.com/pricing/details/bandwidth/)
- [App Service application settings](https://learn.microsoft.com/azure/app-service/configure-common#configure-app-settings)
- [Secure App Service connections](https://learn.microsoft.com/azure/app-service/tutorial-connect-overview)
- [Managed identity to Azure SQL from App Service](https://learn.microsoft.com/azure/app-service/tutorial-connect-msi-sql-database)
