#!/bin/bash
region=$1
resourceGroup=$2
storageName=$3
functionAppName=$4

# -----------------------------------------------------
# Script purpose
# -----------------------------------------------------
# 1. Create Azure Function resource
# 2. Publish Azure Function HttpTrigger (written in C#) to the Azure Function resource

# -----------------------------------------------------
# Script requirements
# -----------------------------------------------------
# Azure CLI (az) and Azure Function Core Tools (func) must have been installed for script below to work. 
#   Azure CLI:                  https://docs.microsoft.com/en-us/cli/azure/?view=azure-cli-latest
#   Azure Functions Core Tools: https://docs.microsoft.com/en-us/azure/azure-functions/functions-run-local

# -----------------------------------------------------
# How to run script
# -----------------------------------------------------
# Send Azure resource names below to match your existing resources in the parameters to the script.
# -arg #1     Code for region where resources will be created. Example: "westus", "eastus", "northeurope", "westeurope"
# -arg #2     Name of Resource Group that will be created
# -arg #3     Name of Storage Account that will be created. Note: name must be unique in all of Azure
# -arg #4     Name of Function App that will be created. Note: name must be unique in all of Azure

# Example: 
# ./CreateAzureResourcesAndPublishFunction.sh "westus" "rg-sqteamsbridge" "stsqteamsbridge$RANDOM" "func-sqteamsbridge$RANDOM"

# -----------------------------------------------------
# Create Azure Resources
# -----------------------------------------------------
# Create a resource group.
# Uncomment line if you use an existing group.
az group create --name $resourceGroup --location $region

# Create an Azure storage account in the resource group.
# Uncomment line if you use an existing group.
az storage account create \
  --name $storageName \
  --location $region \
  --resource-group $resourceGroup \
  --sku Standard_LRS

# Create a serverless function app in the resource group.
# The .NET Core 3.1 HttpTrigger function we'll deploy works on --os-type either Windows or Linux
az functionapp create \
  --name $functionAppName \
  --storage-account $storageName \
  --consumption-plan-location $region \
  --resource-group $resourceGroup \
  --os-type "Windows" \
  --runtime "dotnet" \
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

