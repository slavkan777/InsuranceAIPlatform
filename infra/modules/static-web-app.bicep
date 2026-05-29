// Azure Static Web Apps (Free tier) — hosts the React SPA on a global CDN.
// Public/anonymous load is static only (no backend/DB/AI). Built-in auth gates the
// protected workbench routes. Repo build/deploy wiring is configured at deploy time
// (SWA GitHub Action deployment token), NOT in this template.
@description('Resource name prefix.')
param prefix string
@description('Environment name.')
param environmentName string
@description('Location (SWA control plane region).')
param location string
@description('Tags.')
param tags object

resource swa 'Microsoft.Web/staticSites@2023-12-01' = {
  name: '${prefix}-${environmentName}-swa'
  location: location
  tags: tags
  sku: {
    name: 'Free'
    tier: 'Free'
  }
  properties: {
    allowConfigFileUpdates: true
    stagingEnvironmentPolicy: 'Enabled'
  }
}

output name string = swa.name
output defaultHostname string = swa.properties.defaultHostname
