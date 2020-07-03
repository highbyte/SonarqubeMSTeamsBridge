using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(Highbyte.AzureFunctions.Startup))]

namespace Highbyte.AzureFunctions
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddHttpClient();

            builder.Services.AddSingleton<ISonarqubeSecretValidator, SonarqubeSecretValidator>();
            builder.Services.AddSingleton<ISonarqubeToMSTeamsConverter, SonarqubeToMSTeamsConverter>();
            builder.Services.AddSingleton<ISonarqubeToMSTeamsFilter, SonarqubeToMSTeamsFilter>();

        }
    }
}