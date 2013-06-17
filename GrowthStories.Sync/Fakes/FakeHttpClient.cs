using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Growthstories.Sync
{
    public class FakeHttpClient : IHttpClient
    {

        public FakeHttpClient(JsonSerializerSettings jsonSettings)
        {
            this.JsonSettings = jsonSettings;
        }

        public Func<HttpRequestMessage, object> CreateResponse;
        private JsonSerializerSettings JsonSettings;

        public Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, HttpCompletionOption httpCompletionOption)
        {


            return Task.Run(() =>
            {

                return new HttpResponseMessage()
                {
                    Content = new StringContent(
                        JsonConvert.SerializeObject(CreateResponse(request), Formatting.Indented, JsonSettings),
                        Encoding.UTF8,
                        "application/json"
                        )
                };
            });
        }
    }
}
