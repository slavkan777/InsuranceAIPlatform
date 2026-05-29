// Azure SQL Database — General Purpose Serverless with auto-pause.
// Auto-pause after 60 min idle → compute billed only when in use (storage-only when paused).
// Entra-only authentication (azureADOnlyAuthentication=true) → NO SQL password, NO secret in repo.
@description('Resource name prefix.')
param prefix string
@description('Environment name.')
param environmentName string
@description('Location.')
param location string
@description('Tags.')
param tags object

@description('Entra (Azure AD) admin object id (group preferred). PLACEHOLDER — supplied by the deploy gate; all-zeros will not deploy.')
param sqlAdminObjectId string = '00000000-0000-0000-0000-000000000000'
@description('Entra admin display/login name.')
param sqlAdminLogin string = 'iap-sql-admins'
@description('Auto-pause delay in minutes (-1 disables). 60 = pause after 1h idle.')
param autoPauseDelayMinutes int = 60

var serverName = '${prefix}-${environmentName}-sql-${uniqueString(resourceGroup().id)}'

resource sqlServer 'Microsoft.Sql/servers@2023-08-01-preview' = {
  name: serverName
  location: location
  tags: tags
  properties: {
    version: '12.0'
    minimalTlsVersion: '1.2'
    publicNetworkAccess: 'Enabled'
    administrators: {
      administratorType: 'ActiveDirectory'
      principalType: 'Group'
      login: sqlAdminLogin
      sid: sqlAdminObjectId
      tenantId: subscription().tenantId
      azureADOnlyAuthentication: true
    }
  }
}

resource sqlDb 'Microsoft.Sql/servers/databases@2023-08-01-preview' = {
  parent: sqlServer
  name: 'InsuranceAIPlatform'
  location: location
  tags: tags
  sku: {
    name: 'GP_S_Gen5_1'
    tier: 'GeneralPurpose'
    family: 'Gen5'
    capacity: 1
  }
  properties: {
    autoPauseDelay: autoPauseDelayMinutes
    minCapacity: json('0.5')
    maxSizeBytes: 2147483648 // 2 GB — tiny demo DB
    zoneRedundant: false
    readScale: 'Disabled'
  }
}

// Allow Azure services (e.g. Container Apps) to reach the server (demo-grade; tighten with VNet later).
resource allowAzure 'Microsoft.Sql/servers/firewallRules@2023-08-01-preview' = {
  parent: sqlServer
  name: 'AllowAllAzureServices'
  properties: {
    startIpAddress: '0.0.0.0'
    endIpAddress: '0.0.0.0'
  }
}

output serverName string = sqlServer.name
output serverFqdn string = sqlServer.properties.fullyQualifiedDomainName
output databaseName string = sqlDb.name
