using System;
using System.Globalization;

namespace Highbyte.AzureFunctions
{
    public class SonarqubeToMSTeamsConvert: ISonarqubeToMSTeamsConvert
    {

        public MSTeamsComplexCard ToComplexCard(dynamic data)
        {
            string taskStatus = data.status;                         // SUCCESS, IN_PROGRESS (should never occur in webhook?)

            string analyzedAtString;
            if(data.analysedAt.Value.GetType() == typeof(DateTime))
            {
                CultureInfo culture = new CultureInfo("sv-SE");
                analyzedAtString = data.analysedAt.Value.ToString("G", culture);  // "G" = General date/time pattern (long time), depending on culture. Examples: 2020-06-29 13:37, 6/29/2020 1:37 PM
            }
            else
            {
                analyzedAtString = data.analysedAt; // analyzedAt was not a DateTime object, just show the raw data for this field MS Teams
            }
            

            string projectName = data.project.name;
            string projectKey = data.project.key;
            string projectUrl = data.project.url;

            string qualityGateStatus = data.qualityGate.status;     // SUCCESS, ERROR
            string qualityGateName = data.qualityGate.name;         // Sonar way, etc

            string themeColor;
            string qualityGateResultText;
            switch(qualityGateStatus) 
            {
                case "SUCCESS":
                    themeColor="00FF00";
                    qualityGateResultText = "succeeded";
                    break;
                case "ERROR": 
                    themeColor="FF0000";
                    qualityGateResultText = "_failed_";
                    break;
                default:    // Unkown quality gate status
                    themeColor="FFFF00";
                    qualityGateResultText = "was inconclusive";
                    break;
            }

            // TODO: Find better URL to official Sonarqube logo. See https://www.sonarqube.org/logos/
            System.Uri activityImage = new  System.Uri("https://pbs.twimg.com/profile_images/1224335491899760641/h404B5dU_400x400.jpg");

            // Links for message card format and testing
            // https://docs.microsoft.com/en-us/microsoftteams/platform/webhooks-and-connectors/how-to/connectors-using
            // https://messagecardplayground.azurewebsites.net/
            var msTeamsComplexCard = new MSTeamsComplexCard 
            {
                Type = "MessageCard",
                Context = new System.Uri("http://schema.org/extensions"),
                ThemeColor = themeColor,
                Summary =  $"Sonarqube scan completed with quality gate status: {qualityGateStatus}",       // Summary is not shown in MS Teams card?
                Sections = new Section[]
                {
                    new Section 
                    {
                        Markdown = true,
                        ActivityImage = activityImage,
                        ActivityTitle = $"Sonarqube scan of **{projectName}** {qualityGateResultText}", // Normal font size
                        ActivitySubtitle = $"Quality Gate Status: {qualityGateStatus} ",                 // Smaller font size
                        //ActivityText = "Data from Sonarqube",                                          // Event smaller font size
                        Facts = new Fact[]
                        {
                            new Fact
                            {
                                Name = "Analyzed at",
                                Value = analyzedAtString
                            },
                            new Fact
                            {
                                Name = "Quality Gate",
                                Value = qualityGateName
                            },
                            new Fact
                            {
                                Name = "Task status",
                                Value = taskStatus
                            },
                        }
                    }
                },
                PotentialAction = new PotentialAction[] 
                {
                    new PotentialAction 
                    {
                        Type = "OpenUri",
                        Name = "Open in Sonarqube",
                        Targets = new Target[] 
                        {
                            new Target 
                            {
                                OS = "default",
                                Uri = new System.Uri(projectUrl)
                            }
                        }
                    }
                }

            };

            return msTeamsComplexCard;
        }

        public MSTeamsSimpleCard ToSimpleCard(dynamic data)
        {
            string taskStatus = data.status;                // SUCCESS, IN_PROGRESS (should never occur in webhook?)
            string projectName = data.project.name;
            string qualityGateStatus = data.qualityGate.status; // SUCCESS, ERROR

            var msTeamsSimpleCard = new MSTeamsSimpleCard
            {
                Title = $"Sonarqube project {projectName} was analyzed (task status {taskStatus})",
                Text = $"Quality Gate Status: {qualityGateStatus}"
            };

            return msTeamsSimpleCard;
        }

    }
}