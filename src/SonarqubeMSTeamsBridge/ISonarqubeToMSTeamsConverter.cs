namespace Highbyte.AzureFunctions
{
    public interface ISonarqubeToMSTeamsConverter
    {
        public MSTeamsComplexCard ToComplexCard(dynamic data, string culture);
        //public MSTeamsSimpleCard ToSimpleCard(dynamic data);

    }
}