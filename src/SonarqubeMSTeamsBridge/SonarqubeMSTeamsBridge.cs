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
        public const string Setting_TeamsWebhookUrl = "TeamsWebhookUrl"; 
        public const string Setting_SonarqubeWebhookSecret = "SonarqubeWebhookSecret";
        public const string Setting_QualityGateStatusExcludeList = "QualityGateStatusExcludeList";
        public const string Setting_Culture = "Culture";
        public const string Setting_DisableAuthentication = "DisableAuthentication";
        
        
        private readonly HttpClient _httpClient;
        private readonly ISonarqubeSecretValidator _sonarqubeSecretValidator;
        private readonly ISonarqubeToMSTeamsConverter _sonarqubeToMSTeamsConverter;
        private readonly ISonarqubeToMSTeamsFilter _sonarqubeToMSTeamsFilter;

        public SonarqubeMSTeamsBridge(HttpClient httpClient, ISonarqubeSecretValidator sonarqubeSecretValidator, ISonarqubeToMSTeamsConverter sonarqubeToMSTeamsConverter, ISonarqubeToMSTeamsFilter sonarqubeToMSTeamsFilter)
        {
            _httpClient = httpClient;
            _sonarqubeSecretValidator = sonarqubeSecretValidator;
            _sonarqubeToMSTeamsConverter = sonarqubeToMSTeamsConverter;
            _sonarqubeToMSTeamsFilter = sonarqubeToMSTeamsFilter;
        }

        [FunctionName("SonarqubeMSTeamsBridge")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            ILogger log)
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

            // Optional setting: Culture. Affects displayed Date/time values in MS Teams card. Examples: en-US, sv-SE. If not specified, dates/times are displayed as received from Sonarqube
            var culture = Environment.GetEnvironmentVariable(Setting_Culture, EnvironmentVariableTarget.Process);

            // Optional setting: DisableAuthentication. Disables validation of Sonarqube Webhook secret. Use for local development only!
            var disableAuthenticationString = Environment.GetEnvironmentVariable(Setting_DisableAuthentication, EnvironmentVariableTarget.Process);
            bool disableAuthentication = !string.IsNullOrEmpty(disableAuthenticationString) && disableAuthenticationString.Equals("true",  StringComparison.CurrentCultureIgnoreCase);

            // ----------------------------------------------------
            // Validate signature in http header
            // ----------------------------------------------------
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            if(!disableAuthentication && !_sonarqubeSecretValidator.IsValidSignature(req, requestBody, sonarqubeWebhookSecret))
            {
                log.LogWarning($"Sonarqube secret http header is missing or not valid. Config setting {Setting_SonarqubeWebhookSecret} must match secret in Sonarqube Webhook header {SonarqubeSecretValidator.SonarqubeAuthSignatureHeaderName}.");
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
            //var msTeamsCard = _sonarqubeToMSTeamsConverter.ToSimpleCard(sonarqubeRequestJson);
            var msTeamsCard = _sonarqubeToMSTeamsConverter.ToComplexCard(sonarqubeRequestJson, culture);

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
