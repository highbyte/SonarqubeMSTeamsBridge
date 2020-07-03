namespace Highbyte.AzureFunctions
{
    public interface ISonarqubeToMSTeamsFilter
    {
        public bool ShouldProcess(dynamic data, string qualityGateStatusExcludes);

    }
}