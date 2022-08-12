param appName string
param environment string
param region string
param location string = resourceGroup().location

module names '../resource-names.bicep' = {
  name: 'resource-names'
  params: {
    appName: appName
    region: region
    env: environment
  }
}

module managedIdentityDeployment 'managed-identity.bicep' = {
  name: 'managed-identity-deployment'
  params: {
    location: location
    managedIdentityName: names.outputs.managedIdentityName
  }
}

module loggingDeployment 'logging.bicep' = {
  name: 'logging-deployment'
  params: {
    logAnalyticsWorkspaceName: names.outputs.logAnalyticsWorkspaceName
    location: location
    appInsightsName: names.outputs.appInsightsName
  }
}

module storageDeployment 'storage.bicep' = {
  name: 'storage-deployment'
  params: {
    location: location
    logAnalyticsWorkspaceName: loggingDeployment.outputs.logAnalyticsWorkspaceName
    storageAccountName: names.outputs.storageAccountName
    storageAccountInputContainerName: names.outputs.storageAccountInputContainerName
    storageAccountOutputContainerName: names.outputs.storageAccountOutputContainerName
    managedIdentityName: managedIdentityDeployment.outputs.managedIdentityName
  }
}

module functionDeployment 'function.bicep' = {
  name: 'function-deployment'
  params: {
    appServicePlanName: names.outputs.appServicePlanName
    functionAppName: names.outputs.functionAppName
    location: location
    logAnalyticsWorkspaceName: loggingDeployment.outputs.logAnalyticsWorkspaceName
    storageAccountName: storageDeployment.outputs.storageAccountName
    appInsightsName: loggingDeployment.outputs.appInsightsName
    managedIdentityName: managedIdentityDeployment.outputs.managedIdentityName
    storageAccountInputContainerName: storageDeployment.outputs.storageAccountInputContainerName
    storageAccountOutputContainerName: storageDeployment.outputs.storageAccountOutputContainerName
  }
}
