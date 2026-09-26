@description('Azure region')
param location string = resourceGroup().location
@description('Globally unique base name, lowercase letters and numbers')
param prefix string
@secure()
@description('SQL administrator password; pass through a secure pipeline secret')
param sqlAdminPassword string
param sqlAdminLogin string = 'n4nadmin'
param publicOrigin string = 'https://narendra4news.net'
param skuName string = 'B1'
var sqlName = '${prefix}-sql'
var storageName = take(replace('${prefix}media', '-', ''),24)
var appName = '${prefix}-api'
resource plan 'Microsoft.Web/serverfarms@2023-12-01' = {
 name: '${prefix}-plan'
 location: location
 kind: 'linux'
 sku: { name: skuName }
 properties: { reserved: true }
}
resource sql 'Microsoft.Sql/servers@2023-08-01-preview' = {
 name: sqlName
 location: location
 properties: {
  administratorLogin: sqlAdminLogin
  administratorLoginPassword: sqlAdminPassword
  version: '12.0'
  publicNetworkAccess: 'Enabled'
 }
}
resource azureAccess 'Microsoft.Sql/servers/firewallRules@2023-08-01-preview' = { parent: sql; name: 'AllowAzureServices'; properties: { startIpAddress: '0.0.0.0'; endIpAddress: '0.0.0.0' } }
resource db 'Microsoft.Sql/servers/databases@2023-08-01-preview' = {
 parent: sql
 name: 'Narendra4News'
 location: location
 sku: { name: 'Basic'; tier: 'Basic' }
}
resource storage 'Microsoft.Storage/storageAccounts@2023-05-01' = {
 name: storageName
 location: location
 sku: { name: 'Standard_LRS' }
 kind: 'StorageV2'
 properties: { supportsHttpsTrafficOnly: true; minimumTlsVersion: 'TLS1_2'; allowBlobPublicAccess: false }
}
resource blob 'Microsoft.Storage/storageAccounts/blobServices@2023-05-01' = { parent: storage; name: 'default' }
resource media 'Microsoft.Storage/storageAccounts/blobServices/containers@2023-05-01' = {
 parent: blob
 name: 'media'
 properties: { publicAccess: 'None' }
}
resource insights 'Microsoft.Insights/components@2020-02-02' = {
 name: '${prefix}-insights'
 location: location
 kind: 'web'
 properties: { Application_Type: 'web' }
}
resource app 'Microsoft.Web/sites@2023-12-01' = {
 name: appName
 location: location
 kind: 'app,linux'
 identity: { type: 'SystemAssigned' }
 properties: {
  serverFarmId: plan.id
  httpsOnly: true
  siteConfig: {
   linuxFxVersion: 'DOTNETCORE|9.0'
   ftpsState: 'Disabled'
   minTlsVersion: '1.2'
   appSettings: [
    { name: 'ASPNETCORE_ENVIRONMENT'; value: 'Production' }
    { name: 'ASPNETCORE_FORWARDEDHEADERS_ENABLED'; value: 'true' }
    { name: 'PublicOrigin'; value: publicOrigin }
    { name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'; value: insights.properties.ConnectionString }
    { name: 'ConnectionStrings__Sql'; value: 'Server=tcp:${sql.properties.fullyQualifiedDomainName},1433;Initial Catalog=${db.name};Persist Security Info=False;User ID=${sqlAdminLogin};Password=${sqlAdminPassword};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;' }
    { name: 'AzureStorage__ConnectionString'; value: 'DefaultEndpointsProtocol=https;AccountName=${storage.name};AccountKey=${storage.listKeys().keys[0].value};EndpointSuffix=${environment().suffixes.storage}' }
    { name: 'AzureStorage__Container'; value: media.name }
   ]
  }
 }
}
output apiUrl string = 'https://${app.properties.defaultHostName}'
output sqlServer string = sql.properties.fullyQualifiedDomainName
output storageAccount string = storage.name
