using Xunit;
using Highbyte.AzureFunctions.TestHelpers;
using System.Collections.Generic;

namespace Highbyte.AzureFunctions.SonarqubeMSTeamsBridgeTests
{
    public class SonarqubeSecretValidatorTest
    {

        [Fact]
        public void if_sonarqube_auth_header_is_missing_the_signature_should_not_be_valid()
        {
            // Arrange
            var request = TestFactory.CreateHttpRequest();
            string requestBody = "";
            string sonarqubeWebhookSecret = "";

            // Act
            var sonarqubeSecretValidator = new SonarqubeSecretValidator();
            bool isValidSignature = sonarqubeSecretValidator.IsValidSignature(request, requestBody, sonarqubeWebhookSecret);

            // Assert
            Assert.False(isValidSignature);
        }

        [Fact]
        public void if_sonarqube_auth_header_exists_but_has_invalid_hash_the_signature_should_not_be_valid()
        {
            // Arrange
            string sonarqubeAuthHash = "someinvalidhash";
            var request = TestFactory.CreateHttpRequest(new Dictionary<string,string>(){{SonarqubeSecretValidator.SonarqubeAuthSignatureHeaderName, sonarqubeAuthHash}});
            string requestBody = "{\"test\":\"test\"}"; // Some test data, doesn't matter
            string sonarqubeWebhookSecret = "somesecret";

            // Act
            var sonarqubeSecretValidator = new SonarqubeSecretValidator();
            bool isValidSignature = sonarqubeSecretValidator.IsValidSignature(request, requestBody, sonarqubeWebhookSecret);

            // Assert
            Assert.False(isValidSignature);
        }

        [Fact]
        public void if_sonarqube_auth_header_exists_and_has_valid_has_has_the_signature_should_be_valid()
        {
            // Arrange
            var sonarqubeSecretValidator = new SonarqubeSecretValidator();
            string requestBody = "{\"test\":\"test\"}"; // Some test data, doesn't matter
            string sonarqubeWebhookSecret = "somesecret";   // Some secret, doesn't matter
            // Calculate what the hash would be based on request body and secret
            string sonarqubeAuthHash = sonarqubeSecretValidator.GetHMACSHA256Hash(requestBody, sonarqubeWebhookSecret);
            // Set the valid hash on the request header
            var request = TestFactory.CreateHttpRequest(new Dictionary<string,string>(){{SonarqubeSecretValidator.SonarqubeAuthSignatureHeaderName, sonarqubeAuthHash}});

            // Act
            bool isValidSignature = sonarqubeSecretValidator.IsValidSignature(request, requestBody, sonarqubeWebhookSecret);

            // Assert
            Assert.True(isValidSignature);
        }  

    }
}
