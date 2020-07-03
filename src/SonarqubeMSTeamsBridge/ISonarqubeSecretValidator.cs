using Microsoft.AspNetCore.Http;

namespace Highbyte.AzureFunctions
{
    public interface ISonarqubeSecretValidator
    {
        bool IsValidSignature(HttpRequest request, string requestBody, string sonarqubeWebhookSecret);
        string GetHMACSHA256Hash(string text, string key);
    }
}
