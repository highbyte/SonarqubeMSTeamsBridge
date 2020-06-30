using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;

namespace Highbyte.AzureFunctions
{
    public class SonarqubeMSTeamsBridge
    {
        const string Setting_TeamsWebhookUrl = "TeamsWebhookUrl";
        const string Setting_SonarqubeWebhookSecret = "SonarqubeWebhookSecret";
        const string Setting_QualityGateStatusExcludeList = "QualityGateStatusExcludeList";
        

        // TODO: Inject HttpClient via Azure Function DI. Use .NET Core extension for providing HttpClient correctly (singleton)
        private static readonly HttpClient _httpClient = new HttpClient();
        private readonly ISonarqubeToMSTeamsConvert _sonarqubeToMSTeamsConvert;
        private readonly ISonarqubeToMSTeamsFilter _sonarqubeToMSTeamsFilter;

        public SonarqubeMSTeamsBridge(ISonarqubeToMSTeamsConvert sonarqubeToMSTeamsConvert, ISonarqubeToMSTeamsFilter sonarqubeToMSTeamsFilter)
        {
            _sonarqubeToMSTeamsConvert = sonarqubeToMSTeamsConvert;
            _sonarqubeToMSTeamsFilter = sonarqubeToMSTeamsFilter;
        }

        [FunctionName("SonarqubeMSTeamsBridge")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            ILogger log,
            ExecutionContext context)
        {
            log.LogInformation("Request received from Sonarqube webhook.");

            // ----------------------------------------------------
            // Get and validate config settings
            // ----------------------------------------------------
            // Required setting: SonarqubeWebhookSecret. Contains the Sonarqube Webhook secret. It's the same secret that was configured in the Sonarqube Webhook.
            string sonarqubeWebhookSecret = Environment.GetEnvironmentVariable(Setting_SonarqubeWebhookSecret, EnvironmentVariableTarget.Process);
            if(string.IsNullOrEmpty(sonarqubeWebhookSecret))
            {
                log.LogError($"Required setting {Setting_SonarqubeWebhookSecret} is missing.");
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
            // Required setting: TeamsWebhookUrl
            var teamsWebhookUrl = Environment.GetEnvironmentVariable(Setting_TeamsWebhookUrl, EnvironmentVariableTarget.Process);
            if(string.IsNullOrEmpty(teamsWebhookUrl))
            {
                log.LogError($"Required setting {Setting_TeamsWebhookUrl} is missing.");
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
            // Optional setting: QualityGateStatusExcludeList. A comma separated list of Sonarqube Quality Status values that should be ignored.
            var qualityGateStatusExcludes = Environment.GetEnvironmentVariable(Setting_QualityGateStatusExcludeList, EnvironmentVariableTarget.Process);

            // ----------------------------------------------------
            // Validate signature in http header
            // ----------------------------------------------------
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            if(!SonarqubeSecretValidator.IsValidSignature(req, requestBody, sonarqubeWebhookSecret))
            {
                log.LogWarning($"Sonarqube secret http header is missing or not valid. Config setting {Setting_SonarqubeWebhookSecret} must match secret in Sonarqube Webhook.");
                return new UnauthorizedResult();
            }

            // ----------------------------------------------------
            // Check if a card should be sent to MS Teams or not.
            // ----------------------------------------------------
            dynamic sonarqubeRequestJson = JsonConvert.DeserializeObject(requestBody);

            if(!_sonarqubeToMSTeamsFilter.ShouldProcess(sonarqubeRequestJson, qualityGateStatusExcludes)) 
            {
                log.LogInformation($"Message was not sent to MS Teams due to filter.");
                return new OkResult();
            }

            // ----------------------------------------------------
            // Build MS Teams card from Sonarqube Webhook data 
            // ----------------------------------------------------
            //var msTeamsCard = _sonarqubeToMSTeamsConvert.ToSimpleCard(sonarqubeRequestJson);
            var msTeamsCard = _sonarqubeToMSTeamsConvert.ToComplexCard(sonarqubeRequestJson);

            // ----------------------------------------------------
            // Send message to MS Teams webhook url
            // ----------------------------------------------------
            log.LogInformation($"Sending request to MS Teams webhook URL: {teamsWebhookUrl}");
            var teamsCardContent = new StringContent(JsonConvert.SerializeObject(msTeamsCard), Encoding.UTF8, "application/json");
            await _httpClient.PostAsync(teamsWebhookUrl, teamsCardContent);

            log.LogInformation("Request successfully sent to MS Teams webhook.");
            return new OkResult();
        }

    }
}
