// Storage account + blob containers + lifecycle TTL (self-cleaning demo data).
// App identity granted "Storage Blob Data Contributor" (passwordless).
// No account keys are emitted or stored.
@description('Resource name prefix.')
param prefix string
@description('Environment name.')
param environmentName string
@description('Location.')
param location string
@description('Tags.')
param tags object
@description('Principal id of the app managed identity (granted Blob Data Contributor).')
param appPrincipalId string
@description('Days after which demo blobs are auto-deleted by lifecycle policy.')
param ttlDays int = 14

var stName = take('${prefix}${environmentName}st${uniqueString(resourceGroup().id)}', 24)
// Built-in role: Storage Blob Data Contributor
var blobContributorRoleId = 'ba92f5b4-2d11-453d-a403-e96b0029c9fe'

resource st 'Microsoft.Storage/storageAccounts@2023-05-01' = {
  name: stName
  location: location
  tags: tags
  sku: { name: 'Standard_LRS' }
  kind: 'StorageV2'
  properties: {
    accessTier: 'Hot'
    allowBlobPublicAccess: false
    allowSharedKeyAccess: false // force Entra/Managed-Identity auth — no account keys
    minimumTlsVersion: 'TLS1_2'
    supportsHttpsTrafficOnly: true
  }
}

resource blob 'Microsoft.Storage/storageAccounts/blobServices@2023-05-01' = {
  parent: st
  name: 'default'
  properties: {
    deleteRetentionPolicy: { enabled: true, days: 7 }
  }
}

var containerNames = [
  'claim-docs-temp'
  'claim-evidence'
  'rag-policy-docs'
  'exports-temp'
]

resource containers 'Microsoft.Storage/storageAccounts/blobServices/containers@2023-05-01' = [for name in containerNames: {
  parent: blob
  name: name
  properties: { publicAccess: 'None' }
}]

// Lifecycle: auto-delete blobs in the *-temp / evidence containers after ttlDays (cost hygiene).
resource lifecycle 'Microsoft.Storage/storageAccounts/managementPolicies@2023-05-01' = {
  parent: st
  name: 'default'
  properties: {
    policy: {
      rules: [
        {
          enabled: true
          name: 'demo-data-ttl'
          type: 'Lifecycle'
          definition: {
            filters: {
              blobTypes: [ 'blockBlob' ]
              prefixMatch: [ 'claim-docs-temp', 'claim-evidence', 'exports-temp' ]
            }
            actions: {
              baseBlob: {
                delete: { daysAfterModificationGreaterThan: ttlDays }
              }
            }
          }
        }
      ]
    }
  }
}

resource blobRole 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(st.id, appPrincipalId, blobContributorRoleId)
  scope: st
  properties: {
    principalId: appPrincipalId
    principalType: 'ServicePrincipal'
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', blobContributorRoleId)
  }
}

output name string = st.name
output id string = st.id
output blobEndpoint string = st.properties.primaryEndpoints.blob
