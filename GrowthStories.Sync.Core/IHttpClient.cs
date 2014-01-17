using Growthstories.Core;
using System.Net.Http;
using System.Threading.Tasks;

namespace Growthstories.Sync
{
    public interface IHttpClient
    {

        Task<HttpResponseMessage> SendAsync(HttpRequestMessage request);

        IAuthToken AuthToken { get; set; }
        //Task<string> SendAndGetBodyAsync(HttpRequestMessage request);
    }



}
