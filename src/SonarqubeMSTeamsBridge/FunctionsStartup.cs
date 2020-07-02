using Highbyte.AzureFunctions;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;

[assembly: FunctionsStartup(typeof(Highbyte.AzureFunctions.Startup))]

namespace Highbyte.AzureFunctions
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddHttpClient();

            // builder.Services.AddSingleton<ISonarqubeToMSTeamsConvert>((s) => {
            //     return new SonarqubeToMSTeamsConvert();
            // });

            builder.Services.AddSingleton<ISonarqubeSecretValidator, SonarqubeSecretValidator>();
            builder.Services.AddSingleton<ISonarqubeToMSTeamsConvert, SonarqubeToMSTeamsConvert>();
            builder.Services.AddSingleton<ISonarqubeToMSTeamsFilter, SonarqubeToMSTeamsFilter>();

        }
    }
}