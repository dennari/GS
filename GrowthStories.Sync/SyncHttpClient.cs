
using EventStore.Logging;
using Growthstories.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Growthstories.Sync
{
    public class SyncHttpClient : IHttpClient
    {
        HttpClient Client;


        void InitClient()
        {
            Client = new System.Net.Http.HttpClient(new SyncHttpHandler(new HttpClientHandler()));
            //Client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("GrowthStories", "v0.1"));
        }

        public Task<HttpResponseMessage> SendAsync(HttpRequestMessage request)
        {
            if (Client == null)
                InitClient();

            return Client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
        }

        public Task<string> SendAndGetBodyAsync(HttpRequestMessage request)
        {
            return Task.Run<string>(async () =>
            {
                try
                {
                    var r = await SendAsync(request);
                    var s = await r.Content.ReadAsStringAsync();
                    return s;
                }
                catch (Exception e)
                {
                    throw e;
                }

            });
        }


    }
}
