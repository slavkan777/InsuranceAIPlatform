// =====================================================================
// InsuranceAIPlatform — Azure IaC skeleton (v0.1)
// SKELETON ONLY — NOT DEPLOYED. No `az deployment` has been run.
// Cost model: public static + free; compute/AI scale-to-zero / auto-pause
// / free-tier / manual. See docs/architecture/azure/.
// No secrets, no subscription IDs, no API keys in this file.
// =====================================================================
targetScope = 'subscription'

@description('Short environment name (prod | demo).')
param environmentName string = 'prod'

@description('Azure region for all resources (single region — no cross-region egress).')
param location string = 'westeurope'

@description('Resource name prefix.')
@minLength(2)
@maxLength(6)
param prefix string = 'iap'

@description('Deploy optional AI services (Azure OpenAI / Document Intelligence / AI Search). Default FALSE = zero AI cost until an explicit AI gate.')
param enableAi bool = false

@description('Deploy Azure SQL (serverless). Default FALSE for the minimal deploy — API runs without a DB (Mock AI + in-memory reads). Enable in a later SQL gate with a real Entra admin objectId.')
param enableSql bool = false

@description('Container image for insurance-api. Placeholder public image until our image is published to GHCR (set by the deploy gate).')
param insuranceApiImage string = 'mcr.microsoft.com/azuredocs/containerapps-helloworld:latest'

@description('Blob demo-data TTL in days (lifecycle auto-delete).')
@minValue(1)
@maxValue(90)
param blobTtlDays int = 14

var rgName = 'rg-${prefix}-${environmentName}'
var tags = {
  project: 'InsuranceAIPlatform'
  env: environmentName
  costCenter: 'portfolio-demo'
  managedBy: 'bicep'
}

resource rg 'Microsoft.Resources/resourceGroups@2024-03-01' = {
  name: rgName
  location: location
  tags: tags
}

module monitoring 'modules/monitoring.bicep' = {
  scope: rg
  name: 'monitoring'
  params: { prefix: prefix, environmentName: environmentName, location: location, tags: tags }
}

module identity 'modules/identity.bicep' = {
  scope: rg
  name: 'identity'
  params: { prefix: prefix, environmentName: environmentName, location: location, tags: tags }
}

module keyVault 'modules/key-vault.bicep' = {
  scope: rg
  name: 'key-vault'
  params: { prefix: prefix, environmentName: environmentName, location: location, tags: tags, appPrincipalId: identity.outputs.principalId }
}

module storage 'modules/storage.bicep' = {
  scope: rg
  name: 'storage'
  params: { prefix: prefix, environmentName: environmentName, location: location, tags: tags, appPrincipalId: identity.outputs.principalId, ttlDays: blobTtlDays }
}

module sql 'modules/sql-serverless.bicep' = if (enableSql) {
  scope: rg
  name: 'sql-serverless'
  params: { prefix: prefix, environmentName: environmentName, location: location, tags: tags }
}

module containerApps 'modules/container-apps.bicep' = {
  scope: rg
  name: 'container-apps'
  params: {
    prefix: prefix
    environmentName: environmentName
    location: location
    tags: tags
    image: insuranceApiImage
    userAssignedIdentityId: identity.outputs.id
    userAssignedClientId: identity.outputs.clientId
    logAnalyticsName: monitoring.outputs.logAnalyticsName
    appInsightsConnectionString: monitoring.outputs.appInsightsConnectionString
  }
}

module swa 'modules/static-web-app.bicep' = {
  scope: rg
  name: 'static-web-app'
  params: { prefix: prefix, environmentName: environmentName, location: location, tags: tags }
}

// Optional AI — only deployed when enableAi=true (default false → no AI cost).
module aiOptional 'modules/ai-optional.bicep' = if (enableAi) {
  scope: rg
  name: 'ai-optional'
  params: { prefix: prefix, environmentName: environmentName, location: location, tags: tags, appPrincipalId: identity.outputs.principalId }
}

// AKS is intentionally NOT modelled — deferred (24/7 node cost). See AZURE_SERVICE_MATRIX_V0.1.md.

output resourceGroupName string = rg.name
output staticWebAppName string = swa.outputs.name
output containerAppFqdn string = containerApps.outputs.fqdn
output keyVaultName string = keyVault.outputs.name
output sqlServerFqdn string = enableSql ? sql!.outputs.serverFqdn : ''
output appPrincipalId string = identity.outputs.principalId
