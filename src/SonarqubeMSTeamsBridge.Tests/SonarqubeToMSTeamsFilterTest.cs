using Xunit;
using Highbyte.AzureFunctions.TestHelpers;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.IO;

namespace Highbyte.AzureFunctions.SonarqubeMSTeamsBridgeTests
{
    public class SonarqubeToMSTeamsFilterTest
    {

        [Fact]
        public void if_no_quality_gate_status_exclude_filter_is_set_ok_status_should_be_processed()
        {
            // Arrange
            dynamic data = GetExampleSonarqubeQualityGateOkJson();
            string qualityGateStatusExcludes = null;

            // Act
            var sonarqubeToMSTeamsFilter = new SonarqubeToMSTeamsFilter();
            bool shouldProcess = sonarqubeToMSTeamsFilter.ShouldProcess(data, qualityGateStatusExcludes);

            // Assert
            Assert.True(shouldProcess);
        }

        [Fact]
        public void if_no_quality_gate_status_exclude_filter_is_set_error_status_should_be_processed()
        {
            // Arrange
            dynamic data = GetExampleSonarqubeQualityGateErrorJson();
            string qualityGateStatusExcludes = null;

            // Act
            var sonarqubeToMSTeamsFilter = new SonarqubeToMSTeamsFilter();
            bool shouldProcess = sonarqubeToMSTeamsFilter.ShouldProcess(data, qualityGateStatusExcludes);

            // Assert
            Assert.True(shouldProcess);
        }

        [Fact]
        public void if_no_quality_gate_status_exclude_filter_is_set_other_status_should_be_processed()
        {
            // Arrange
            dynamic data = GetExampleSonarqubeQualityGateOtherJson();
            string qualityGateStatusExcludes = null;

            // Act
            var sonarqubeToMSTeamsFilter = new SonarqubeToMSTeamsFilter();
            bool shouldProcess = sonarqubeToMSTeamsFilter.ShouldProcess(data, qualityGateStatusExcludes);

            // Assert
            Assert.True(shouldProcess);
        }

        [Fact]
        public void if_quality_gate_status_exclude_filter_include_status_ok_an_ok_status_should_not_be_processed()
        {
            // Arrange
            dynamic data = GetExampleSonarqubeQualityGateOkJson();
            string qualityGateStatusExcludes = "OK";

            // Act
            var sonarqubeToMSTeamsFilter = new SonarqubeToMSTeamsFilter();
            bool shouldProcess = sonarqubeToMSTeamsFilter.ShouldProcess(data, qualityGateStatusExcludes);

            // Assert
            Assert.False(shouldProcess);
        }        

        [Fact]
        public void if_quality_gate_status_exclude_filter_include_status_ok_an_error_status_should_be_processed()
        {
            // Arrange
            dynamic data = GetExampleSonarqubeQualityGateErrorJson();
            string qualityGateStatusExcludes = "OK";

            // Act
            var sonarqubeToMSTeamsFilter = new SonarqubeToMSTeamsFilter();
            bool shouldProcess = sonarqubeToMSTeamsFilter.ShouldProcess(data, qualityGateStatusExcludes);

            // Assert
            Assert.True(shouldProcess);
        }   

        private dynamic GetExampleSonarqubeQualityGateOkJson() 
        {
            return JsonConvert.DeserializeObject(File.ReadAllText(@"Example_Webhook_Request_From_Sonarqube_v8.3.1_quality_gate_ok.json"));
        }
        private dynamic GetExampleSonarqubeQualityGateErrorJson() 
        {
            return JsonConvert.DeserializeObject(File.ReadAllText(@"Example_Webhook_Request_From_Sonarqube_v8.3.1_quality_gate_error.json"));
        }
        private dynamic GetExampleSonarqubeQualityGateOtherJson() 
        {
            return JsonConvert.DeserializeObject(File.ReadAllText(@"Example_Webhook_Request_From_Sonarqube_v8.3.1_quality_gate_other.json"));
        }



    }
}
