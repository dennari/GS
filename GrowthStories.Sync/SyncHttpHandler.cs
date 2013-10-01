
using EventStore.Logging;
using System.Net.Http;
using System.Threading;


namespace Growthstories.Sync
{
    public class SyncHttpHandler : MessageProcessingHandler
    {

        private static ILog Logger = LogFactory.BuildLogger(typeof(SyncHttpHandler));

        public SyncHttpHandler(HttpMessageHandler innerHandler)
            : base(innerHandler)
        {
        }

        protected override HttpRequestMessage ProcessRequest(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Logger.Info("[HTTPREQUEST]\n" + request.ToString());
            return request;
        }

        protected override HttpResponseMessage ProcessResponse(HttpResponseMessage response, CancellationToken cancellationToken)
        {
            Logger.Info("[HTTPRESPONSE]\n" + response.ToString());
            return response;
        }

        //protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        //{

        //}
    }
}
