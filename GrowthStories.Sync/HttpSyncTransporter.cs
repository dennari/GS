using Newtonsoft.Json;
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

        private JsonSerializerSettings _JsonSettings;

        public JsonSerializerSettings JsonSettings
        {
            get
            {
                return _JsonSettings == null ? _JsonSettings = Kernel.Get<JsonSerializerSettings>() : _JsonSettings;
            }
        }


        public HttpSyncTransporter(IKernel kernel)
        {
            Kernel = kernel;
        }

        public ISyncPushRequest CreatePushRequest(IEnumerable<IEventDTO> syncDTOs)
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
                var r = await SendAsync<HttpPushResponse>(request);
                return (ISyncPushResponse)r;
            });
        }

        HttpCompletionOption _completion = HttpCompletionOption.ResponseContentRead;

        public Task<ISyncPullResponse> PullAsync(ISyncPullRequest request)
        {
            return Task.Run<ISyncPullResponse>(async () =>
            {
                var r = await SendAsync<HttpPullResponse>(request);
                return (ISyncPullResponse)r;
            });
        }

        protected Task<TResponse> SendAsync<TResponse>(ISyncRequest request)
        {
            return Task.Run<TResponse>(async () =>
            {
                var HttpResponse = await Client.SendAsync(CreateHttpRequest(request), _completion);
                var Body = await HttpResponse.Content.ReadAsStringAsync();
                return (TResponse)JsonConvert.DeserializeObject(Body, typeof(TResponse), JsonSettings);
            });
        }

        protected HttpRequestMessage CreateHttpRequest(ISyncRequest request)
        {
            return new HttpRequestMessage()
            {
                Method = HttpMethod.Post,
                Content = new StringContent(
                    JsonConvert.SerializeObject(request, JsonSettings),
                    Encoding.UTF8,
                    "application/json"
                )
            };
        }



    }
}
