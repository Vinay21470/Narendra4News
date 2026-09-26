# Azure infrastructure

`main.bicep` creates a Linux App Service, Azure SQL, Storage and Application Insights in an existing resource group. It deliberately does not configure public image access or custom domains. It requires an available globally unique prefix.

```sh
az group create --name narendra4news-rg --location centralindia
az deployment group create --resource-group narendra4news-rg --template-file infra/main.bicep --parameters prefix=<unique-prefix> sqlAdminPassword=<secure-value>
```

**Security work before launch:** do not deploy with the SQL admin account as the application identity. Replace it with an Azure SQL contained identity and managed identity based access; narrow SQL firewall/network access; use private endpoints or other secure network path; use managed identity to access Blob storage; serve images through an approved CDN or signed URLs; configure backups and restore tests. The template is a low-cost initial environment, not a hardened production infrastructure configuration.

The App Service must be assigned a verified custom domain and certificate separately. Configure both apex and `www` DNS. Set `PublicOrigin` to the selected canonical domain and configure a frontend redirect for the alternate host. Publishing this project does not change your domain's DNS automatically.
