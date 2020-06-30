using System;
using System.Linq;

namespace Highbyte.AzureFunctions
{
    public class SonarqubeToMSTeamsFilter: ISonarqubeToMSTeamsFilter
    {
        public bool ShouldProcess(dynamic data) 
        {
            bool allowedQualityGateStatus = AllowedQualityGateStatus(data);
            return allowedQualityGateStatus;
        }

        public bool AllowedQualityGateStatus(dynamic data)
        {
            // Setting QualityGateStatusExcludeList is a comma separated list of Sonarqube Quality Status values that should be ignored.
            var qualityGateStatusExcludes = Environment.GetEnvironmentVariable("QualityGateStatusExcludeList", EnvironmentVariableTarget.Process);
            // If not specifying any Quality Gate Status exclude filter, allow any status.
            if(string.IsNullOrEmpty(qualityGateStatusExcludes))
                return true;

            string[] qualityGateStatusExcludeList = qualityGateStatusExcludes.Split(",");
            return !qualityGateStatusExcludeList.Contains((string)data.qualityGate.status, StringComparer.CurrentCultureIgnoreCase);
        }

    }
}