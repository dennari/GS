
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Growthstories.Sync
{
    public class SyncHttpClient : IHttpClient
    {
        System.Net.Http.HttpClient Client;
        private IJsonFactory jFactory;

        public SyncHttpClient(IJsonFactory jFactory)
        {
            this.jFactory = jFactory;
        }


        void InitClient()
        {
            Client = new System.Net.Http.HttpClient(new SyncHttpHandler(new HttpClientHandler()));
            Client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("GrowthStories", "v0.1"));
        }

        public Task<HttpResponseMessage> SendAsync(ISyncRequest request)
        {
            if (Client == null)
                InitClient();
            return Client.SendAsync(CreateHttpRequest(request), HttpCompletionOption.ResponseHeadersRead);
        }

        HttpRequestMessage CreateHttpRequest(ISyncRequest request)
        {
            return new HttpRequestMessage()
            {
                Method = HttpMethod.Post,
                Content = new StringContent(
                    jFactory.Serialize(request),
                    Encoding.UTF8,
                    "application/json"
                )
            };
        }

    }
}
