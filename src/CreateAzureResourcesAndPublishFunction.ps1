
# -----------------------------------------------------
# Script purpose
# -----------------------------------------------------
# 1. Create Azure Function resource
# 2. Publish Azure Function HttpTrigger (written in C#) to the Azure Function resource
# 3. Set Azure Function application setting for MS Teams Webhook URL

# Alternative to running this script is to use Azure Portal
# - Create Azure Function resource
# - Create a Function with Http Trigger in the Azure Function resource, copy/paste source code from SonarqubeMSTeamsBridge.cs
# - Set Azure Function application setting TeamsWebhookUrl

# -----------------------------------------------------
# Script requirements
# -----------------------------------------------------
# Azure CLI (az) and Azure Function Core Tools (func) must have been installed for script below to work. 
#   Azure CLI:                  https://docs.microsoft.com/en-us/cli/azure/?view=azure-cli-latest
#   Azure Functions Core Tools: https://docs.microsoft.com/en-us/azure/azure-functions/functions-run-local

# -----------------------------------------------------
# Script settings
# -----------------------------------------------------
# Change to desired Azure region here. 
# Examples: westeurope, northeurope, eastus, westus, etc.
$region          = "--Azure region goes here--"

# Change Azure resource names below if you want your own names.
# Note: Function app and storage account names must be unique across Azure
$resourceGroup   = "rg-sqteamsbridge"
$storageName     = "stsqteamsbridge$(Get-Random -Max 32767)"
$functionAppName = "func-sqteamsbridge$(Get-Random -Max 32767)"

# -----------------------------------------------------
# Create Azure Resources
# -----------------------------------------------------
# Create a resource group.
# Uncomment line if you use an existing group.
az group create --name $resourceGroup --location $region

# Create an Azure storage account in the resource group.
az storage account create `
  --name $storageName `
  --location $region `
  --resource-group $resourceGroup `
  --sku Standard_LRS

# Create a serverless function app in the resource group.
az functionapp create `
  --name $functionAppName `
  --storage-account $storageName `
  --consumption-plan-location $region `
  --resource-group $resourceGroup `
  --functions-version 3

# -----------------------------------------------------
# Publish Azure Function application
# -----------------------------------------------------
#Note: As the local caching of the Azure CLI (az) vs the Azure Functions Core Tools (func) is different,
#      it can take some time for the "func" command to detect a newly created Function Application with "az".
#      Can (probably) be worked around with a sleep delay, ref: https://github.com/Azure/azure-functions-core-tools/issues/1616
sleep 30

# Publish Azure Function Application (it will build and publish SonarqubeMSTeamsBridge.csproj in current folder)
func azure functionapp publish $functionAppName

# When the publish command has completed, it will display the URL used for the SonarqubeMSTeamsBridge function.
# Result from publish command_:
#Functions in func-sqteamsbridgeXXXX:
#    SonarqubeMSTeamsBridge - [httpTrigger]
#        Invoke url: https://func-sqteamsbridgeXXXXX.azurewebsites.net/api/sonarqubemsteamsbridge

#Afterwards this command can get you the URL also (replace last parameter with the name of the Function resource that was created )
#func azure functionapp list-functions "func-sqteamsbridgeXXXX"

## *** This is the URL you set in the Sonarqube Webhook ***

# -----------------------------------------------------
# List created resources
# -----------------------------------------------------
az resource list --resource-group $resourceGroup --output table

# Query resources created
#az group list --query "[?contains(name, 'sqteamsbridge')]"
#az storage account list --query "[?contains(name, 'sqteamsbridge')]"
#az functionapp list --query "[?contains(name, 'sqteamsbridge')]"

