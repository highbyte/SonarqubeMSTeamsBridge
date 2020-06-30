namespace Highbyte.AzureFunctions
{
    public interface ISonarqubeToMSTeamsConvert
    {
        public MSTeamsComplexCard ToComplexCard(dynamic data);
        public MSTeamsSimpleCard ToSimpleCard(dynamic data);

    }
}