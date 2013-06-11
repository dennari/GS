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
        public Func<HttpRequestMessage, object> CreateResponse;

        public Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, HttpCompletionOption httpCompletionOption)
        {


            string json = JsonConvert.SerializeObject(CreateResponse(request), Formatting.Indented, new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            });
            var content = new StringContent(json);

            return Task.Run(() =>
            {

                return new HttpResponseMessage()
                {
                    Content = content
                };
            });
        }
    }
}
