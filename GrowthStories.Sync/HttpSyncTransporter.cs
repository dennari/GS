using Ninject;
using Ninject.Parameters;
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
        private IKernel Kernel;
        private IHttpClient client;

        public IHttpClient Client
        {
            get
            {
                return client == null ? client = Kernel.Get<IHttpClient>() : client;
            }
        }


        public HttpSyncTransporter(IKernel kernel)
        {
            Kernel = kernel;
        }

        public ISyncPushRequest CreatePushRequest(ICollection<IEventDTO> syncDTOs)
        {
            return Kernel.Get<ISyncPushRequest>(new ConstructorArgument("transporter", this), new ConstructorArgument("events", syncDTOs));
        }

        public ISyncPullRequest CreatePullRequest()
        {
            return Kernel.Get<ISyncPullRequest>(new ConstructorArgument("transporter", this));
        }


        public Task<ISyncPushResponse> PushAsync(ICollection<IEventDTO> events)
        {
            return PushAsync(CreatePushRequest(events));
        }

        public Task<ISyncPushResponse> PushAsync(ISyncPushRequest request)
        {
            return Task.Run<ISyncPushResponse>(async () =>
            {
                var HttpResponse = await Client.SendAsync(new JsonRequest(request), _completion);
                var Body = await HttpResponse.Content.ReadAsStringAsync();
                return new HttpPushResponse(HttpResponse, Body);
            });
        }

        HttpCompletionOption _completion = HttpCompletionOption.ResponseContentRead;

        public Task<ISyncPullResponse> PullAsync(ISyncPullRequest request)
        {
            return Task.Run<ISyncPullResponse>(async () =>
            {
                var HttpResponse = await Client.SendAsync(new JsonRequest(request), _completion);
                var Body = await HttpResponse.Content.ReadAsStringAsync();
                return new HttpPullResponse(HttpResponse, Body);
            });
        }

    }
}
