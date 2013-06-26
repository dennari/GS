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

        public Func<HttpRequestMessage, int, object> CreateResponse;
        private JsonSerializerSettings JsonSettings;
        private int Num = 0;

        public Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, HttpCompletionOption httpCompletionOption)
        {


            return Task.Run(() =>
            {
                this.Num++;
                return new HttpResponseMessage()
                {
                    Content = new StringContent(
                        JsonConvert.SerializeObject(CreateResponse(request, Num), Formatting.Indented, JsonSettings),
                        Encoding.UTF8,
                        "application/json"
                        )
                };
            });
        }

        public void Clear()
        {
            Num = 0;
        }
    }
}
