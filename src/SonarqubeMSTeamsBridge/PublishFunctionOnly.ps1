param ($functionAppName)

# -----------------------------------------------------
# Script purpose
# -----------------------------------------------------
# Publish Azure Function HttpTrigger (written in C#) to existing Azure Function resource.
# It creates or updates existing HttpTrigger.
# To create required Azure Resources (and Publish Azure Function), see script CreateAzureResourcesAndPublishFunction.ps1

# -----------------------------------------------------
# Script requirements
# -----------------------------------------------------
# Azure Function Core Tools (func) must have been installed for script below to work. 
#   Azure Functions Core Tools: https://docs.microsoft.com/en-us/azure/azure-functions/functions-run-local

# -----------------------------------------------------
# How to run script
# -----------------------------------------------------
# Send Azure resource names below to match your existing resources in the parameters to the script.
# Example: .\PublishFunctionOnly.ps1 -functionAppName "func-sqteamsbridge5678"

# -----------------------------------------------------
# Publish Azure Function application
# -----------------------------------------------------
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
