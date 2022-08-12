param storageAccountName string
param location string
param logAnalyticsWorkspaceName string
param storageAccountInputContainerName string
param storageAccountOutputContainerName string
param managedIdentityName string

resource logAnalyticsWorkspace 'Microsoft.OperationalInsights/workspaces@2021-06-01' existing = {
  name: logAnalyticsWorkspaceName
}

resource storageAccount 'Microsoft.Storage/storageAccounts@2021-08-01' = {
  name: storageAccountName
  location: location
  sku: {
    name: 'Standard_LRS'
  }
  kind: 'StorageV2'
  properties: {}
}

resource inputContainer 'Microsoft.Storage/storageAccounts/blobServices/containers@2021-04-01' = {
  name: '${storageAccount.name}/default/${storageAccountInputContainerName}'
}

resource outputContainer 'Microsoft.Storage/storageAccounts/blobServices/containers@2021-04-01' = {
  name: '${storageAccount.name}/default/${storageAccountOutputContainerName}'
}

resource diagnosticSettings 'Microsoft.Insights/diagnosticSettings@2021-05-01-preview' = {
  name: 'Logging'
  scope: storageAccount
  properties: {
    workspaceId: logAnalyticsWorkspace.id
    metrics: [
      {
        category: 'Transaction'
        enabled: true
      }
    ]
  }
}

resource managedIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2018-11-30' existing = {
  name: managedIdentityName
}

resource storageBlobDataContributorRoleDefinition 'Microsoft.Authorization/roleDefinitions@2018-01-01-preview' existing = {
  scope: subscription()
  name: 'ba92f5b4-2d11-453d-a403-e96b0029c9fe'
}

resource roleAssignment 'Microsoft.Authorization/roleAssignments@2020-04-01-preview' = {
  scope: storageAccount
  name: guid(storageAccount.id, managedIdentity.name, storageBlobDataContributorRoleDefinition.name)
  properties: {
    roleDefinitionId: storageBlobDataContributorRoleDefinition.id
    principalId: managedIdentity.properties.principalId
    principalType: 'ServicePrincipal'
  }
}

output storageAccountName string = storageAccount.name
output storageAccountInputContainerName string = storageAccountInputContainerName
output storageAccountOutputContainerName string = storageAccountOutputContainerName
