// User-assigned managed identity for insurance-api.
// The Container App authenticates to Key Vault / Storage / SQL via this identity
// (passwordless — no connection-string secrets anywhere).
@description('Resource name prefix.')
param prefix string
@description('Environment name.')
param environmentName string
@description('Location.')
param location string
@description('Tags.')
param tags object

resource uami 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  name: '${prefix}-${environmentName}-api-mi'
  location: location
  tags: tags
}

output id string = uami.id
output principalId string = uami.properties.principalId
output clientId string = uami.properties.clientId
output name string = uami.name
