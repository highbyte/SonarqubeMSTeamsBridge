namespace Highbyte.AzureFunctions
{
    public interface ISonarqubeToMSTeamsConverter
    {
        public MSTeamsComplexCard ToComplexCard(dynamic data, string culture);
    }
}