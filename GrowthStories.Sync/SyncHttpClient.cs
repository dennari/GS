
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Growthstories.Sync
{
    public class SyncHttpClient : IHttpClient
    {
        public SyncHttpClient()
        {

        }

        HttpClient Client;
        void InitClient()
        {
            Client = new HttpClient(new SyncHttpHandler(new HttpClientHandler()));
            Client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("GrowthStories", "v0.1"));
        }

        public Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, HttpCompletionOption httpCompletionOption)
        {
            if (Client == null)
                InitClient();
            return Client.SendAsync(request, httpCompletionOption);
        }
    }
}
