using System;
using System.Globalization;

namespace Highbyte.AzureFunctions
{
    public class SonarqubeToMSTeamsConvert: ISonarqubeToMSTeamsConvert
    {
        private System.Uri activityImageSuccess = new System.Uri("https://upload.wikimedia.org/wikipedia/commons/thumb/7/73/Flat_tick_icon.svg/200px-Flat_tick_icon.svg.png");
        private System.Uri activityImageFailure = new System.Uri("https://upload.wikimedia.org/wikipedia/commons/thumb/7/7f/Dialog-error.svg/200px-Dialog-error.svg.png");
        private System.Uri activityImageInconclusive = new System.Uri("https://upload.wikimedia.org/wikipedia/commons/thumb/a/aa/Emblem-question-yellow.svg/200px-Emblem-question-yellow.svg.png");

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
            System.Uri activityImage;

            switch(qualityGateStatus.ToUpper()) 
            {
                case "SUCCESS":
                    themeColor="00FF00";    // Green
                    qualityGateResultText = "succeeded";
                    activityImage = activityImageSuccess;
                    break;
                case "ERROR": 
                    themeColor="FF0000";    // Red
                    qualityGateResultText = "_failed_";
                    activityImage = activityImageFailure;
                    break;
                default:    // Unkown quality gate status
                    themeColor="FFFF00";    // Yellow
                    qualityGateResultText = "was inconclusive";
                    activityImage = activityImageInconclusive;
                    break;
            }

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