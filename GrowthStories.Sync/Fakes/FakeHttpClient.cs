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

        public FakeHttpClient(IJsonFactory jFactory)
        {
            this.jFactory = jFactory;
        }

        public Func<ISyncRequest, int, object> CreateResponse;
        private JsonSerializerSettings JsonSettings;
        private int Num = 0;
        private IJsonFactory jFactory;

        public Task<HttpResponseMessage> SendAsync(ISyncRequest request)
        {


            return Task.Run(() =>
            {
                this.Num++;
                return new HttpResponseMessage()
                {
                    Content = new StringContent(
                        jFactory.Serialize(CreateResponse(request, Num)),
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
