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

module eventGridSubscriptionDeployment 'eventSubscription.bicep' = {
  name: 'event-grid-subscription-deployment'
  params: {
    functionAppName: names.outputs.functionAppName
    newBlobCreatedEventGridTopicName: names.outputs.newBlobCreatedEventGridTopicName
    location: location
    logAnalyticsWorkspaceName: names.outputs.logAnalyticsWorkspaceName
    storageAccountName: names.outputs.storageAccountName
    storageAccountInputContainerName: names.outputs.storageAccountInputContainerName
  }
}
