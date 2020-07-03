
using System;
using System.Globalization;

namespace Highbyte.AzureFunctions
{
    public class SonarqubeToMSTeamsConverter: ISonarqubeToMSTeamsConverter
    {
        const string Setting_IconUrl_OK = "IconUrl_OK";
        const string Setting_IconUrl_ERROR = "IconUrl_ERROR";
        const string Setting_IconUrl_other = "IconUrl_other";
        public MSTeamsComplexCard ToComplexCard(dynamic data, string culture)
        {
            string taskStatus = data.status;                         // SUCCESS, IN_PROGRESS (should never occur in webhook?)

            string analyzedAtString;
            if(string.IsNullOrEmpty(culture)) 
            {
                analyzedAtString = data.analysedAt.Value.ToString("G");  // "G" = General date/time pattern (long time), formatted as default. Example: 6/29/2020 1:37 PM
            }
            else 
            {
                CultureInfo cultureInfo = new CultureInfo(culture);
                analyzedAtString = data.analysedAt.Value.ToString("G", cultureInfo);  // "G" = General date/time pattern (long time), formated depending on culture. Examples: 2020-06-29 13:37, 6/29/2020 1:37 PM
            }
            
            string projectName = data.project.name;
            string projectKey = data.project.key;
            string projectUrl = data.project.url;

            string qualityGateStatus = data.qualityGate.status;     // OK, ERROR
            string qualityGateName = data.qualityGate.name;         // Sonar way, etc

            string themeColor;
            string qualityGateResultText;
            System.Uri activityImage;

            switch(qualityGateStatus.ToUpper()) 
            {
                case "OK":
                    themeColor="00FF00";    // Green
                    qualityGateResultText = "succeeded";
                    activityImage = GetActivityImageSuccess();
                    break;
                case "ERROR": 
                    themeColor="FF0000";    // Red
                    qualityGateResultText = "_failed_";
                    activityImage = GetActivityImageFailure();
                    break;
                default:    // Unkown quality gate status
                    themeColor="FFFF00";    // Yellow
                    qualityGateResultText = "was inconclusive";
                    activityImage = GetActivityImageInconclusive();
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

        public static System.Uri GetActivityImageSuccess()
        {
            var url = Environment.GetEnvironmentVariable(Setting_IconUrl_OK, EnvironmentVariableTarget.Process);
            if(string.IsNullOrEmpty(url))
                url = "https://upload.wikimedia.org/wikipedia/commons/thumb/7/73/Flat_tick_icon.svg/200px-Flat_tick_icon.svg.png";
            return new System.Uri(url);
        }
        public static System.Uri GetActivityImageFailure()
        {
            var url = Environment.GetEnvironmentVariable(Setting_IconUrl_ERROR, EnvironmentVariableTarget.Process);
            if(string.IsNullOrEmpty(url))
                url = "https://upload.wikimedia.org/wikipedia/commons/thumb/7/7f/Dialog-error.svg/200px-Dialog-error.svg.png";
            return new System.Uri(url);
        }
        public static System.Uri GetActivityImageInconclusive()
        {
            var url = Environment.GetEnvironmentVariable(Setting_IconUrl_other, EnvironmentVariableTarget.Process);
            if(string.IsNullOrEmpty(url))
                url = "https://upload.wikimedia.org/wikipedia/commons/thumb/a/aa/Emblem-question-yellow.svg/200px-Emblem-question-yellow.svg.png";
            return new System.Uri(url);
        }        
    }
}