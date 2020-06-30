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
        // TODO: Inject HttpClient via Azure Function DI. Use .NET Core extension for providing HttpClient correctly (singleton)
        private static readonly HttpClient _httpClient = new HttpClient();
        private readonly ISonarqubeToMSTeamsConvert _sonarqubeToMSTeamsConvert;

        public SonarqubeMSTeamsBridge(ISonarqubeToMSTeamsConvert sonarqubeToMSTeamsConvert)
        {
            this._sonarqubeToMSTeamsConvert = sonarqubeToMSTeamsConvert;
        }

        [FunctionName("SonarqubeMSTeamsBridge")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            ILogger log,
            ExecutionContext context)
        {
            log.LogInformation("Request received from Sonarqube webhook.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);

            // Build MS Teams card from Sonarqube Webhook data 
            //var msTeamsCard = _sonarqubeToMSTeamsConvert.ToSimpleCard(data);
            var msTeamsCard = _sonarqubeToMSTeamsConvert.ToComplexCard(data);

            // Read MS Teams webhook url from config
            var teamsWebhookUrl = Environment.GetEnvironmentVariable("TeamsWebhookUrl", EnvironmentVariableTarget.Process);
            log.LogInformation($"Setting TeamsWebhookUrl = {teamsWebhookUrl}");

            // Serialize MS Teams card to JSON
            var teamsCardContent = new StringContent(JsonConvert.SerializeObject(msTeamsCard), Encoding.UTF8, "application/json");

            // Send message to MS Teams webhook url
            log.LogInformation("Sending request to MS Teams webhook.");
            await _httpClient.PostAsync(teamsWebhookUrl, teamsCardContent);
            log.LogInformation("Request successfully sent to MS Teams webhook.");

            return new OkResult();
        }

    }
}
