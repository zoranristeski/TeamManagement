# GitHub Actions CI and manual Azure deployment

## Workflow behavior

The workflow is [.github/workflows/azure-deploy.yml](../../.github/workflows/azure-deploy.yml).

- `push` to `main` and pull requests run restore, Release build, tests, and publish.
- The Azure deployment job runs only for a manually dispatched workflow on `main`.
- Deployment waits for the build/test/publish job to succeed.
- The workflow does not provision Azure resources, configure secrets, or run EF migrations.
- No long-lived Azure client secret or publish profile is required; Azure login uses GitHub OIDC.

This separation prevents code pushes from deploying to Azure automatically. Add a required reviewer to the GitHub `azure-poc` environment before the first deployment if approval-in-GitHub is desired.

## One-time Azure OIDC identity setup

Complete this only after the App Service exists and you have approved the required Azure resources.

1. Sign in to the intended subscription:

   ```powershell
   az login
   az account set --subscription <subscription-id>
   az account show --query "{subscription:name, tenant:tenantId, id:id}" --output table
   ```

2. Create a dedicated Entra application and service principal for GitHub deployment:

   ```powershell
   $clientId = az ad app create --display-name "<unique-github-deploy-app-name>" --query appId --output tsv
   $applicationObjectId = az ad app list --filter "appId eq '$clientId'" --query "[0].id" --output tsv
   $servicePrincipalObjectId = az ad sp create --id $clientId --query id --output tsv
   ```

3. Configure a federated identity credential matching the repository and the `azure-poc` GitHub environment. Replace `<OWNER>` and `<REPOSITORY>` with their actual values. Use the same values later in GitHub:

   ```powershell
   $federatedCredential = @{
       name = "github-azure-poc"
       issuer = "https://token.actions.githubusercontent.com"
       subject = "repo:<OWNER>/<REPOSITORY>:environment:azure-poc"
       description = "GitHub Actions OIDC for the manual Azure POC deployment"
       audiences = @("api://AzureADTokenExchange")
   } | ConvertTo-Json -Depth 4
   $federatedCredential | Set-Content -Path .\federated-credential.json -Encoding utf8
   az ad app federated-credential create --id $applicationObjectId --parameters .\federated-credential.json
   Remove-Item .\federated-credential.json
   ```

4. Scope `Website Contributor` to this App Service only (not the whole subscription/resource group):

   ```powershell
   $webAppResourceId = az webapp show --name <app-name> --resource-group <resource-group> --query id --output tsv
   az role assignment create --assignee-object-id $servicePrincipalObjectId --assignee-principal-type ServicePrincipal --role "Website Contributor" --scope $webAppResourceId
   ```

   The signed-in administrator needs permission to create an Entra application/service principal and assign Azure roles. If organizational policy forbids this, ask the tenant administrator to configure the OIDC trust and role assignment; do not substitute a client secret or publish profile.

## GitHub environment and variables

In the repository, open **Settings > Environments**, create `azure-poc`, and optionally require reviewers. Add these **environment variables**:

| Variable | Value |
| --- | --- |
| `AZURE_CLIENT_ID` | Client ID / `appId` of the Entra application used for GitHub OIDC |
| `AZURE_TENANT_ID` | Tenant ID of the Azure subscription |
| `AZURE_SUBSCRIPTION_ID` | Intended subscription ID |
| `AZURE_WEBAPP_NAME` | Exact existing App Service name |

These are resource/tenant identifiers, not credentials. The workflow requires **no GitHub secrets**. Do not add a SQL connection string, Auth0 secret, token, or Azure client secret as a workflow variable. The API's database and authentication configuration belongs in App Service settings and the App Service uses its own system-assigned identity for SQL.

## Run a manual deployment

1. Confirm the selected GitHub ref is `main`.
2. Confirm Azure resource setup, database migration, identity permissions, firewall, required App Service settings, HTTPS-only, and production auth values are all verified.
3. In GitHub, open **Actions > Azure App Service deploy > Run workflow** and dispatch from `main`.
4. The build job restores, Release-builds, runs the API integration tests, and publishes the API. Any failure prevents the deploy job from running.
5. The deployment job obtains an OIDC token, logs into Azure, and deploys the published package to the named web app.
6. Verify `/health/live`, `/health/ready`, 401/403 behavior, and the production Swagger absence using [09-deployment-validation.md](./09-deployment-validation.md).

Do not run the deployment workflow until you intend to deploy. No cloud deployment has been run by this work.

## Workflow maintenance

- Keep deployment limited to a protected GitHub environment and `main`.
- Review action releases and consider pinning third-party actions to immutable commit SHAs for production.
- Protect `main` with required build/test checks and restrict who can dispatch deployments.
- Rotate/revoke the federated credential and remove the role assignment if the repository or deployment identity is retired.
- Do not add database migrations to this workflow without a separate reviewed design for network access, identity, approvals, and rollback.

References: [Azure OIDC from GitHub Actions](https://learn.microsoft.com/azure/developer/github/connect-from-azure-openid-connect), [GitHub OIDC hardening](https://docs.github.com/actions/deployment/security-hardening-your-deployments/configuring-openid-connect-in-azure), [Azure Login action](https://github.com/Azure/login), [App Service deploy action](https://github.com/Azure/webapps-deploy).
