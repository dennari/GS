
using EventStore;
using Growthstories.Domain.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Growthstories.Sync
{


    public class HttpSyncTransporter : ITransportEvents
    {

        private readonly IHttpClient client;
        private readonly IResponseFactory ResponseFactory;
        private IHttpRequestFactory RequestFactory;


        public HttpSyncTransporter(
            IHttpClient client,
            IResponseFactory responseFactory,
            IHttpRequestFactory requestFactory)
        {
            this.client = client;
            this.ResponseFactory = responseFactory;
            this.RequestFactory = requestFactory;
        }



        public Task<ISyncPushResponse> PushAsync(ISyncPushRequest request)
        {
            return Task.Run<ISyncPushResponse>(async () =>
            {
                return ResponseFactory.CreatePushResponse(await client.SendAndGetBodyAsync(RequestFactory.CreatePushRequest(request)));

            });
        }


        public Task<ISyncPullResponse> PullAsync(ISyncPullRequest request)
        {
            return Task.Run<ISyncPullResponse>(async () =>
            {
                return ResponseFactory.CreatePullResponse(await client.SendAndGetBodyAsync(RequestFactory.CreatePullRequest(request)));
            });
        }



    }
}
