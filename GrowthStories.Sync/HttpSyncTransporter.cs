
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

        public HttpSyncTransporter(
            IHttpClient client,
            IJsonFactory jFactory,
            IResponseFactory responseFactory)
        {
            this.client = client;
            this.ResponseFactory = responseFactory;
        }



        public Task<ISyncPushResponse> PushAsync(ISyncPushRequest request)
        {
            return Task.Run<ISyncPushResponse>(async () =>
            {
                var r = await SendAsync(request);
                return ResponseFactory.CreatePushResponse(r.Item1);

            });
        }


        public Task<ISyncPullResponse> PullAsync(ISyncPullRequest request)
        {
            return Task.Run<ISyncPullResponse>(async () =>
            {
                var r = await SendAsync(request);
                return ResponseFactory.CreatePullResponse(r.Item1);
            });
        }

        protected Task<Tuple<string, HttpResponseMessage>> SendAsync(ISyncRequest request)
        {
            return Task.Run<Tuple<string, HttpResponseMessage>>(async () =>
            {
                HttpResponseMessage HttpResponse = await client.SendAsync(request);
                var Body = await HttpResponse.Content.ReadAsStringAsync();
                return Tuple.Create(Body, HttpResponse);
            });
        }

        HttpCompletionOption _completion = HttpCompletionOption.ResponseContentRead;



    }
}
