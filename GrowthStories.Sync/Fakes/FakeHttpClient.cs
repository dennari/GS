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


        private IJsonFactory jFactory;



        public Task<HttpResponseMessage> SendAsync(HttpRequestMessage request)
        {
            return Task.Run(() =>
            {

                return new HttpResponseMessage()
                {
                    Content = new StringContent(
                        "",
                        Encoding.UTF8,
                        "application/json"
                        )
                };
            });
        }

        public Task<string> SendAndGetBodyAsync(HttpRequestMessage request)
        {
            return Task.Run(() =>
            {
                return "";
            });
        }
    }
}
