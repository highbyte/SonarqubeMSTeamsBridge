using Highbyte.AzureFunctions;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(Highbyte.Functions.Startup))]

namespace Highbyte.Functions
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            //builder.Services.AddHttpClient();

            // builder.Services.AddSingleton<ISonarqubeToMSTeamsConvert>((s) => {
            //     return new SonarqubeToMSTeamsConvert();
            // });

            builder.Services.AddSingleton<ISonarqubeToMSTeamsConvert, SonarqubeToMSTeamsConvert>();
            builder.Services.AddSingleton<ISonarqubeToMSTeamsFilter, SonarqubeToMSTeamsFilter>();
        }
    }
}