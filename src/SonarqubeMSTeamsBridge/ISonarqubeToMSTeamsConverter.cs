namespace Highbyte.AzureFunctions
{
    public interface ISonarqubeToMSTeamsConvert
    {
        public MSTeamsComplexCard ToComplexCard(dynamic data, string culture);
        public MSTeamsSimpleCard ToSimpleCard(dynamic data);

    }
}