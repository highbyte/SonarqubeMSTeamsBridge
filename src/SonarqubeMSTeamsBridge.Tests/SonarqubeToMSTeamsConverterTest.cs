using Xunit;
using System.IO;
using Newtonsoft.Json;
using System.Globalization;

namespace Highbyte.AzureFunctions.SonarqubeMSTeamsBridgeTests
{
    public class SonarqubeToMSTeamsConverterTest
    {

        [Fact]
        public void can_convert_valid_sonarqube_webhook_to_msteams_card()
        {
            // Arrange
            var sonarqubeToMSTeamsConverter = new SonarqubeToMSTeamsConverter();
            dynamic data = GetExampleSonarqubeQualityGateOkJson();
            string culture = null;

            // Act
            var msTeamsComplexCard = sonarqubeToMSTeamsConverter.ToComplexCard(data, culture);

            // Assert
            Assert.NotNull(msTeamsComplexCard);
            Assert.Equal("MessageCard", msTeamsComplexCard.Type);
            Assert.Equal(new System.Uri("http://schema.org/extensions"), msTeamsComplexCard.Context);
            Assert.NotEmpty(msTeamsComplexCard.Summary);
            Assert.True(msTeamsComplexCard.Sections.Length>0);

            var potentialAction = msTeamsComplexCard.PotentialAction[0];
            Assert.Equal("OpenUri", potentialAction.Type);
            
        }

        public void msteams_card_will_contain_a_potentialaction_with_link_to_sonarqube_portal()
        {
            // Arrange
            var sonarqubeToMSTeamsConverter = new SonarqubeToMSTeamsConverter();
            dynamic data = GetExampleSonarqubeQualityGateOkJson();
            string culture = null;

            // Act
            var msTeamsComplexCard = sonarqubeToMSTeamsConverter.ToComplexCard(data, culture);

            // Assert
            var potentialAction = msTeamsComplexCard.PotentialAction[0];
            Assert.Equal("OpenUri", potentialAction.Type);
            Assert.Equal("Open in Sonarqube", potentialAction.Name);

            var potentialActionTarget = potentialAction.Targets[0];
            Assert.Equal("Default", potentialActionTarget.OS);
            Assert.Equal(new System.Uri(data.project.url), potentialActionTarget.Uri);
            
        }        

        #region MS Teams card color
        [Fact]
        public void sonarqube_webhook_with_quality_gate_ok_gets_green_msteams_card()
        {
            // Arrange
            var sonarqubeToMSTeamsConverter = new SonarqubeToMSTeamsConverter();
            dynamic data = GetExampleSonarqubeQualityGateOkJson();
            string culture = null;

            // Act
            var msTeamsComplexCard = sonarqubeToMSTeamsConverter.ToComplexCard(data, culture);

            // Assert
            Assert.Equal("00FF00", msTeamsComplexCard.ThemeColor);
            var section =  msTeamsComplexCard.Sections[0];
            Assert.Equal(SonarqubeToMSTeamsConverter.ActivityImageSuccess, section.ActivityImage);
            Assert.Contains("succeeded", section.ActivityTitle);
            Assert.Contains((string)data.qualityGate.status, section.ActivitySubtitle);
        }

        [Fact]
        public void sonarqube_webhook_with_quality_gate_error_gets_red_msteams_card()
        {
            // Arrange
            var sonarqubeToMSTeamsConverter = new SonarqubeToMSTeamsConverter();
            dynamic data = GetExampleSonarqubeQualityGateErrorJson();
            string culture = null;

            // Act
            var msTeamsComplexCard = sonarqubeToMSTeamsConverter.ToComplexCard(data, culture);

            // Assert
            Assert.Equal("FF0000", msTeamsComplexCard.ThemeColor);
            var section =  msTeamsComplexCard.Sections[0];
            Assert.Equal(SonarqubeToMSTeamsConverter.ActivityImageFailure, section.ActivityImage);
            Assert.Contains("failed", section.ActivityTitle);
            Assert.Contains((string)data.qualityGate.status, section.ActivitySubtitle);
        }           

        [Fact]
        public void sonarqube_webhook_with_quality_gate_neither_ok_nor_error_gets_yellow_msteams_card()
        {
            // Arrange
            var sonarqubeToMSTeamsConverter = new SonarqubeToMSTeamsConverter();
            dynamic data = GetExampleSonarqubeQualityGateOtherJson();
            string culture = null;

            // Act
            var msTeamsComplexCard = sonarqubeToMSTeamsConverter.ToComplexCard(data, culture);

            // Assert
            Assert.Equal("FFFF00", msTeamsComplexCard.ThemeColor);
            var section =  msTeamsComplexCard.Sections[0];
            Assert.Equal(SonarqubeToMSTeamsConverter.ActivityImageInconclusive, section.ActivityImage);
            Assert.Contains("inconclusive", section.ActivityTitle);
            Assert.Contains((string)data.qualityGate.status, section.ActivitySubtitle);
        }
        #endregion

        #region MS Teams card fact: Analyzed at

        [Fact]
        public void msteams_card_fact_analyzed_at_is_set_from_sonarqube_webhook_data()
        {
            // Arrange
            var sonarqubeToMSTeamsConverter = new SonarqubeToMSTeamsConverter();
            dynamic data = GetExampleSonarqubeQualityGateOkJson();
            string culture = null;

            // Act
            var msTeamsComplexCard = sonarqubeToMSTeamsConverter.ToComplexCard(data, culture);

            // Assert
            var fact = msTeamsComplexCard.Sections[0].Facts[0];
            Assert.Equal("Analyzed at", fact.Name);
            Assert.Equal(data.analysedAt.Value.ToString("G"), fact.Value);
        }

        [Fact]
        public void msteams_card_fact_analyzed_at_is_set_from_sonarqube_webhook_data_based_on_specified_culture()
        {
            // Arrange
            var sonarqubeToMSTeamsConverter = new SonarqubeToMSTeamsConverter();
            dynamic data = GetExampleSonarqubeQualityGateOkJson();
            string culture = "sv-SE";

            // Act
            var msTeamsComplexCard = sonarqubeToMSTeamsConverter.ToComplexCard(data, culture);

            // Assert
            var fact = msTeamsComplexCard.Sections[0].Facts[0];
            CultureInfo cultureInfo = new CultureInfo(culture); 
            Assert.Equal(data.analysedAt.Value.ToString("G",cultureInfo), fact.Value);
        }

        #endregion


        #region MS Teams card fact: Quality Gate

        [Fact]
        public void msteams_card_fact_quality_gate_is_set_from_sonarqube_webhook_data()
        {
            // Arrange
            var sonarqubeToMSTeamsConverter = new SonarqubeToMSTeamsConverter();
            dynamic data = GetExampleSonarqubeQualityGateOkJson();
            string culture = null;

            // Act
            var msTeamsComplexCard = sonarqubeToMSTeamsConverter.ToComplexCard(data, culture);

            // Assert
            var fact = msTeamsComplexCard.Sections[0].Facts[1];
            Assert.Equal("Quality Gate", fact.Name);
            Assert.Equal((string)data.qualityGate.name, fact.Value);
        }
        #endregion        

        #region MS Teams card fact: Task status

        [Fact]
        public void msteams_card_fact_task_status_is_set_from_sonarqube_webhook_data()
        {
            // Arrange
            var sonarqubeToMSTeamsConverter = new SonarqubeToMSTeamsConverter();
            dynamic data = GetExampleSonarqubeQualityGateOkJson();
            string culture = null;

            // Act
            var msTeamsComplexCard = sonarqubeToMSTeamsConverter.ToComplexCard(data, culture);

            // Assert
            var fact = msTeamsComplexCard.Sections[0].Facts[2];
            Assert.Equal("Task status", fact.Name);
            Assert.Equal((string)data.status, fact.Value);
        }
        #endregion        


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
