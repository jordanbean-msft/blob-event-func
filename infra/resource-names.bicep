param appName string
param region string
param env string

output appInsightsName string = 'ai-${appName}-${region}-${env}'
output logAnalyticsWorkspaceName string = 'la-${appName}-${region}-${env}'
output storageAccountName string = toLower('sa${appName}${region}${env}')
output storageAccountInputContainerName string = 'input'
output storageAccountOutputContainerName string = 'output'
output appServicePlanName string = 'asp-${appName}-${region}-${env}'
output functionAppName string = 'func-${appName}-${region}-${env}'
output eventHubNamespaceName string = 'eh-${appName}-${region}-${env}'
output eventHubName string = 'input'
output privateDnsZoneName string = 'privatelink.azurewebsites.net'
output functionAppNetworkInterfaceName string = 'nic-${appName}-${region}-${env}'
output functionAppPrivateEndpointName string = 'pe-${appName}-${region}-${env}'
output applicationSubnetName string = 'application'
output privateEndpointSubnetName string = 'privateEndpoints'
output managedIdentityName string = 'mi-${appName}-${region}-${env}'
output vNetName string = 'vnet-${appName}-${region}-${env}'
output newBlobCreatedEventGridTopicName string = 'egt-NewInputBlobCreated-${appName}-${region}-${env}'
