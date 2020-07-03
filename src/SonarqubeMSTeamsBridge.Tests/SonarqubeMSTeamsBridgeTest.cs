using System;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using Xunit;
using Highbyte.AzureFunctions.TestHelpers;
using Highbyte.AzureFunctions;
using Moq;

namespace Highbyte.AzureFunctions.SonarqubeMSTeamsBridgeTests
{
    public class SonarqubeMSTeamsBridgeTest
    {

        [Fact]
        public async void function_http_trigger_should_return_500_if_secret_config_is_missing()
        {
            // Arrange
            SetEnvironmentVariables(sonarqubeWebhookSecret: null);

            var request = TestFactory.CreateHttpRequest();
            var sonarqubeMSTeamsBridgeFunction =  new SonarqubeMSTeamsBridge(null,null,null,null);
            ListLogger logger = (ListLogger)TestFactory.CreateLogger(LoggerTypes.List);

            // Act
            var response = (StatusCodeResult)await sonarqubeMSTeamsBridgeFunction.Run(request, logger);

            // Assert
            Assert.Equal(500, response.StatusCode);
            Assert.Contains($"Required setting {SonarqubeMSTeamsBridge.Setting_SonarqubeWebhookSecret} is missing.", logger.Logs);

        }

        [Fact]
        public async void function_http_trigger_should_return_500_if_teamswebhook_config_is_missing()
        {
            // Arrange
            SetEnvironmentVariables(teamsWebhookUrl: null);

            var request = TestFactory.CreateHttpRequest();
            var sonarqubeMSTeamsBridgeFunction =  new SonarqubeMSTeamsBridge(null,null,null,null);
            ListLogger logger = (ListLogger)TestFactory.CreateLogger(LoggerTypes.List);

            // Act
            var response = (StatusCodeResult)await sonarqubeMSTeamsBridgeFunction.Run(request, logger);

            // Assert
            Assert.Equal(500, response.StatusCode);
            Assert.Contains($"Required setting {SonarqubeMSTeamsBridge.Setting_TeamsWebhookUrl} is missing.", logger.Logs);
        }

        [Fact]
        public async void function_http_trigger_should_return_401_if_SonarqubeSecretValidator_rejects_request()
        {
            // Arrange
             SetEnvironmentVariables();

            var request = TestFactory.CreateHttpRequest();

            var sonarqubeSecretValidator = new Mock<ISonarqubeSecretValidator>();
            sonarqubeSecretValidator.Setup(x => x.IsValidSignature(
                request, 
                It.IsAny<string>(),
                Environment.GetEnvironmentVariable(SonarqubeMSTeamsBridge.Setting_SonarqubeWebhookSecret))
                ).Returns(false);

            var sonarqubeMSTeamsBridgeFunction =  new SonarqubeMSTeamsBridge(null,sonarqubeSecretValidator.Object,null,null);
            ListLogger logger = (ListLogger)TestFactory.CreateLogger(LoggerTypes.List);

            // Act
            var response = (StatusCodeResult)await sonarqubeMSTeamsBridgeFunction.Run(request, logger);

            // Assert
            Assert.Equal(401, response.StatusCode);
            Assert.Contains($"Sonarqube secret http header is missing or not valid. Config setting {SonarqubeMSTeamsBridge.Setting_SonarqubeWebhookSecret} must match secret in Sonarqube Webhook header {SonarqubeSecretValidator.SonarqubeAuthSignatureHeaderName}.", logger.Logs);
        }        

        [Fact]
        public async void function_http_trigger_should_return_200_if_valid_request_and_message_was_sent_to_msteams()
        {
            // Arrange
            SetEnvironmentVariables();

            var request = TestFactory.CreateHttpRequest();

            var sonarqubeSecretValidator = new Mock<ISonarqubeSecretValidator>();
            sonarqubeSecretValidator.Setup(x => x.IsValidSignature(
                request, 
                It.IsAny<string>(),
                Environment.GetEnvironmentVariable(SonarqubeMSTeamsBridge.Setting_SonarqubeWebhookSecret))
                ).Returns(true);

            var sonarqubeToMSTeamsFilter = new Mock<ISonarqubeToMSTeamsFilter>();
            sonarqubeToMSTeamsFilter.Setup(x => x.ShouldProcess(
                It.IsAny<object>(),
                It.IsAny<string>())
                ).Returns(true);

            var sonarqubeToMSTeamsConverterMock = new Mock<ISonarqubeToMSTeamsConverter>();
            sonarqubeToMSTeamsConverterMock.Setup(x => x.ToComplexCard(
                It.IsAny<object>(), 
                It.IsAny<string>())
                ).Returns(new MSTeamsComplexCard());


            var httpClient = new HttpClient(new VoidOkHttpMessageHandler());
            var sonarqubeMSTeamsBridgeFunction =  new SonarqubeMSTeamsBridge(httpClient, sonarqubeSecretValidator.Object, sonarqubeToMSTeamsConverterMock.Object, sonarqubeToMSTeamsFilter.Object);
            ListLogger logger = (ListLogger)TestFactory.CreateLogger(LoggerTypes.List);
            
            // Act
            var response = (StatusCodeResult)await sonarqubeMSTeamsBridgeFunction.Run(request, logger);

            // Assert
            Assert.Equal(200, response.StatusCode);
        }    

        private void SetEnvironmentVariables(string sonarqubeWebhookSecret="somesecret", string teamsWebhookUrl="https://somemsteamswebhookurl.com", string culture = null, string disableAuthentication = null) 
        {
            Environment.SetEnvironmentVariable(SonarqubeMSTeamsBridge.Setting_SonarqubeWebhookSecret, sonarqubeWebhookSecret);
            Environment.SetEnvironmentVariable(SonarqubeMSTeamsBridge.Setting_TeamsWebhookUrl, teamsWebhookUrl);

            // Optional
            Environment.SetEnvironmentVariable(SonarqubeMSTeamsBridge.Setting_DisableAuthentication, culture);
            Environment.SetEnvironmentVariable(SonarqubeMSTeamsBridge.Setting_Culture, disableAuthentication);
        }

    }
}
