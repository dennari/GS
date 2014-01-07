using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;

namespace Growthstories.Sync
{
    public interface IHttpClient
    {

        Task<HttpResponseMessage> SendAsync(HttpRequestMessage request);
        //Task<string> SendAndGetBodyAsync(HttpRequestMessage request);
    }



}
