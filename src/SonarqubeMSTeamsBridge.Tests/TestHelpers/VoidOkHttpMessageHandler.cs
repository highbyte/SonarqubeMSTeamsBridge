using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Highbyte.AzureFunctions.TestHelpers
{
    public class VoidOkHttpMessageHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            //return Task.FromResult(new HttpResponseMessage() { StatusCode = HttpStatusCode.OK, Content = new StringContent("OK") }); 
            return Task.FromResult(new HttpResponseMessage() { StatusCode = HttpStatusCode.OK }); 
        }
    }
}