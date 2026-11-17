# Maintenance, recovery, and cost controls

## Cost position

The intended recurring Azure price is **$0 only while** the API remains on eligible App Service F1, the database remains eligible for SQL Free with `AutoPause`, and usage stays inside each current monthly free allowance. This is not a spending cap or a cost guarantee.

| Component | POC target | Cost/limitation |
| --- | --- | --- |
| Linux App Service F1 | Free | Shared CPU; current published quota is 60 CPU minutes/app/day, 1 GB RAM, 1 GB storage; no scale-out/SLA. Any plan upgrade is billable. |
| Azure SQL Database Free | 100,000 vCore-seconds/month, 32 GB data, 32 GB backup storage (current offer), `AutoPause` at compute exhaustion | Free database count/eligibility and limits can change. Wrong settings or an explicitly paid restore/overage can incur charges. |
| Bandwidth | First 100 GB/month of internet egress currently free under general Azure bandwidth pricing; inbound is free | Internet egress beyond the allowance and inter-region transfer may be billed. Same-region API-to-SQL transfer currently has no additional charge. This allowance is not an F1-specific network quota. |
| SQL logical server / system identity / firewall rules | Required supporting configuration | No separate SQL server compute or identity request fee is expected; other supporting services/networking may cost. |
| GitHub Actions | Included account allowance | GitHub may bill over its own minutes/storage limits; not an Azure charge. |
| App Configuration, Key Vault, Log Analytics, Application Insights, Blob Storage, private endpoints, NAT | Not included in this POC | Do not add them without reviewing tier, region, ingestion/storage/network usage, and estimated costs. |

Prices vary by region, subscription, tax, offer, and date. Use the [Azure pricing calculator](https://azure.microsoft.com/pricing/calculator/) and verify the SQL Free offer in the Azure portal before deployment. A Cost Management budget/alert is advisory and does not stop resource usage. Review cost alerts and usage monthly.

If F1 cannot run the workload or the offer is unavailable, the cheapest suitable *paid* choice is region/usage-specific: compare App Service Basic (Linux) and the lowest-cost supported Azure SQL serverless configuration with auto-pause. Do not choose a tier until the calculator estimate, SQL minimum compute/billing, restore/backup costs, and network requirements are reviewed. Private SQL access or production availability may require paid tiers; do not treat the public F1 architecture as production-secure.

## Operations checklist

### Each deployment

- Review dependency advisories and apply supported patches; maintain aligned .NET 10 and EF Core 10 servicing versions.
- Run the Release API build and automated tests; deploy only from the manually approved workflow.
- Review App Service configuration drift, managed identity permissions, SQL firewall IP allowlist, and production log levels.
- Apply only reviewed EF migrations with the migration operator; retain the migration ID and deployment result.
- Verify `/health/live`, `/health/ready`, authorization, CORS, HTTPS, and production Swagger behavior.

### Monthly

- Review SQL Free monthly vCore usage remaining, data size, backup storage, and `AutoPause` state.
- Review App Service F1 CPU/storage quota and restarts.
- Check Cost Management actual/forecast usage and any unexpected supporting resource.
- Review identity provider role assignments, Azure RBAC, GitHub environment protection, and deployment logs.
- Confirm automated backup retention and test the documented export/recovery procedure on a non-production target when feasible.

## Backup and recovery expectations

Azure SQL automated backups are managed by the service; verify the database's current retention and redundancy. The Free offer is a POC facility, not a production SLA. Long-term retention and cross-region resilience may incur charges.

An Azure SQL point-in-time restore creates a new database; target provisioning may be billed and does not overwrite the source. Before restoring, review the target SKU and pricing. Test the restore, validate the restored database, then point the API at it only after approval. Keep the original until the data owner approves cleanup.

Use BACPAC export for portable logical backup, store it encrypted with least-privilege access, and never commit it. An Azure Storage account used for exports has a separate bill. Protect the backup file as highly sensitive child/student data.

Detailed commands are in [05-database-deployment.md](./05-database-deployment.md).

## Scaling and production transition

The first production blockers are not just compute size. Before real student data or multiple clubs:

1. Implement and test per-student/per-club authorization and tenant ownership.
2. Add guardian/consent and age-related safeguards, data minimization, and retention/deletion policies.
3. Confirm jurisdiction, data residency, encryption, backup/restore objectives, auditing, incident response, and privacy/legal obligations.
4. Select an SLA-backed App Service tier and secure private networking if required; size SQL and backup/retention to measured demand.
5. Add monitored diagnostics/alerts only with privacy-safe logging, access controls, retention limits, and cost approval.
6. Conduct security review/penetration testing and capacity/performance testing.

Do not scale the POC plan and assume those controls are automatically provided.

## Reference links

- [Azure SQL Free tier and usage tracking](https://learn.microsoft.com/azure/azure-sql/database/free-tier)
- [Azure SQL automated backups](https://learn.microsoft.com/azure/azure-sql/database/automated-backups-overview)
- [Azure SQL point-in-time restore](https://learn.microsoft.com/azure/azure-sql/database/recovery-using-backups)
- [App Service pricing plans](https://learn.microsoft.com/azure/app-service/overview-hosting-plans)
- [Azure pricing calculator](https://azure.microsoft.com/pricing/calculator/)
