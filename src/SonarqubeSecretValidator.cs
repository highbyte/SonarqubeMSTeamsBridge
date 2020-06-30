using System;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace Highbyte.AzureFunctions
{
    public static class SonarqubeSecretValidator
    {
        const string SonarqubeAuthSignatureHeaderName = "X-Sonar-Webhook-HMAC-SHA256";
        public static bool IsValidSignature(HttpRequest request, string requestBody, string sonarqubeWebhookSecret) 
        {
            // Read the header that is sent by Sonarqube, which contains a computed HMAC SHA256 hash based on the request body and a configured secret.
            StringValues headerValues = request.Headers[SonarqubeAuthSignatureHeaderName];
            if(headerValues.Count==0)
                return false;
            string receivedSignature = headerValues[0]; // Assume only one value for this header

            string expectedSignature = GetHMACSHA256Hash(requestBody, sonarqubeWebhookSecret);
            return object.Equals(expectedSignature, receivedSignature);  
        }

        public static string GetHMACSHA256Hash(string text, string key)
        {
            var encoding = new UTF8Encoding();

            Byte[] textBytes = encoding.GetBytes(text);
            Byte[] keyBytes = encoding.GetBytes(key);

            Byte[] hashBytes;

            using (HMACSHA256 hash = new HMACSHA256(keyBytes))
                hashBytes = hash.ComputeHash(textBytes);

            return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
        }        
    }
}
