using System;
using System.Linq;

namespace Highbyte.AzureFunctions
{
    public class SonarqubeToMSTeamsFilter: ISonarqubeToMSTeamsFilter
    {
        public bool ShouldProcess(dynamic data, string qualityGateStatusExcludes) 
        {
            bool allowedQualityGateStatus = AllowedQualityGateStatus(data, qualityGateStatusExcludes);
            return allowedQualityGateStatus;
        }

        public bool AllowedQualityGateStatus(dynamic data, string qualityGateStatusExcludes)
        {
            // If not specifying any Quality Gate Status exclude filter, allow any status.
            if(string.IsNullOrEmpty(qualityGateStatusExcludes))
                return true;

            string[] qualityGateStatusExcludeList = qualityGateStatusExcludes.Split(",");
            return !qualityGateStatusExcludeList.Contains((string)data.qualityGate.status, StringComparer.CurrentCultureIgnoreCase);
        }

    }
}