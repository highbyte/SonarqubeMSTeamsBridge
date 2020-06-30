# Sonarqube MS Teams Bridge

## What
Allows Sonarqube scan results to be shown in a MS Teams channel.

<img src="doc/MSTeams_cards.png" width="75%" height="75%" title="MS Teams card example">

## How
An Azure Function written in .NET Core v3 (C#) that processes incoming Sonarqube Webhook requests, and transforms them to a MS Teams "card" that is then sent to a MS Teams channel via a MS Teams Webhook.

Tested with (it way work for other versions)
* Sonarqube (Community) v8.3.1.34397
* MS Teams (Free) v1.3.00.13565

## Why
There was no Sonarqube plugin for integration with MS Teams when I checked. As I'm not sufficiently proficient in Java to implement a proper Sonarqube plugin, so I did the simplest thing possible for myself to provide the functionallity.

## Deployment to Azure
You deploy this Azure Function to your own Azure Subscription.

### Via script
There is a provided Powershell script ([src/CreateAzureResourcesAndPublishFunction.ps1](src/CreateAzureResourcesAndPublishFunction.ps1)) that creates necessary Azure resources, and compiles/uploads the Azure function project in this repository.

The Powershell script requires the following command line tools to be installed on the machine you run it from
* [Azure CLI (az)](https://docs.microsoft.com/en-us/cli/azure/?view=azure-cli-latest)
* [Azure Functions Core Tools (func)](https://docs.microsoft.com/en-us/azure/azure-functions/functions-run-local)

#### Login to Azure via the CLI tool
``` powershell
az login
```

#### Set current Subscription (if you have more than one)
``` powershell
az account set --subscription [your_subscription_id_or_name_here] 
```

#### Change script variables
Before running the script, you need to edit the script and set the following variables.
* MS Teams Webhook URL for your channel. Look [here](https://docs.microsoft.com/en-us/microsoftteams/platform/webhooks-and-connectors/how-to/add-incoming-webhook) on details on how to set it up.
``` powershell
$teamsWebhookUrl = "https://outlook.office.com/webhook/xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx@xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx/IncomingWebhook/xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx/xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx"
```

* The Azure region where the Azure function (and related resources) will be deployed to.
``` powershell
$region          = "westeurope"
```

Optionally you can also change the name of the Azure Resource group and other resources that are created. Note that Function app and storage account names must be unique across Azure.
``` powershell
$resourceGroup   = "rg-sqteamsbridge"
$storageName     = "stsqteamsbridge$(Get-Random -Max 32767)"
$functionAppName = "func-sqteamsbridge$(Get-Random -Max 32767)"
```

#### Run the script
``` powershell
cd src
.\CreateAzureResourcesAndPublishFunction.ps1
```

When the script has completed, it will output the Invoke url of the Azure Function. 
**This is the address that should be configured in Sonarqube as a Webhook.**
```
Functions in func-sqteamsbridgeXXXXX:
    SonarqubeMSTeamsBridge - [httpTrigger]
        Invoke url: https://func-sqteamsbridgeXXXXX.azurewebsites.net/api/sonarqubemsteamsbridge
```

It will also display the Azure resources created by the script
```
Name                     ResourceGroup     Location     Type                               Status
-----------------------  ----------------  -----------  ---------------------------------  --------
func-sqteamsbridgeXXXXX  rg-sqteamsbridge  westeurope   microsoft.insights/components
stsqteamsbridgeYYYYY     rg-sqteamsbridge  westeurope   Microsoft.Storage/storageAccounts
WestEuropePlan           rg-sqteamsbridge  westeurope   Microsoft.Web/serverFarms
func-sqteamsbridgeXXXXX  rg-sqteamsbridge  westeurope   Microsoft.Web/sites
```

### Manually
TODO

## Azure Function settings
The Azure Function uses the following settings from environment variables.

| Setting | Required| Default value | Description |
| --- | --- | --- | --- |
| TeamsWebhookUrl |  Yes | _n/a_ | The Webhook URL that is configured in MS Teams for your channel where messages will be sent to |
| QualityGateStatusExcludeList | No | _Not set, empty string_ | A comma-separated list of Sonarqube Quality Gate status values that should not be sent to MS Teams. By default this is not set, and means you will get MS Teams messages for both succeed and failed scans. If you only want failed scans, then you should set this value to SUCCESS |

You can see/change settings in the Azure Portal

<img src="doc/AzureFunction_Settings.png" width="75%" height="75%" title="Azure Function settings">

Or change settings via script. Note if you did run the deployment script, the setting TeamsWebhookUrl was set automatically. Other setting(s) are optional.

_Example script (change Azure resource names and setting values)_
``` powershell
az functionapp config appsettings set --name "func-sqteamsbridgeXXXXX" --resource-group "rg-sqteamsbridge" --settings "TeamsWebhookUrl=https://outlook.office.com/webhook/XXXX"
az functionapp config appsettings set --name "func-sqteamsbridgeXXXXX" --resource-group "rg-sqteamsbridge" --settings "QualityGateStatusExcludeList=SUCCESS"
```

## Configure Sonarqube
* Login as administrator in Sonarqube portal
* Goto Administration -> Configuration -> Webhooks
* Create Webhook
  * Name: _Any name works_
  * URL: _The Azure Function invoke URL created above_
  * Secret: _A strong secret password_

<img src="doc/Sonarqube_Webhook.png" width="75%" height="75%" title="Sonarqube Webhook configuration">

## Running locally in VS Code
TODO
