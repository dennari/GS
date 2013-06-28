using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;


namespace Growthstories.Sync
{

    public class HttpRequestFactory : IRequestFactory, IResponseFactory
    {
        private readonly IJsonFactory jFactory;
        private readonly ITranslateEvents Translator;


        public HttpRequestFactory(ITranslateEvents translator, IJsonFactory jFactory)
        {
            this.jFactory = jFactory;
            this.Translator = translator;
        }

        public ISyncPushRequest CreatePushRequest(IEnumerable<ISyncEventStream> streams)
        {
            var streamsC = streams.ToArray();
            var req = new HttpPushRequest()
            {
                Events = Translator.Out(streamsC),
                Streams = streamsC,
                //PushId = Guid.NewGuid(),
                ClientDatabaseId = Guid.NewGuid()
            };

            return req;
        }

        public ISyncPullRequest CreatePullRequest(IEnumerable<ISyncEventStream> streams)
        {
            var streamsC = streams.ToArray();
            var req = new HttpPullRequest()
            {
                Streams = streamsC
            };
            return req;
        }

        public ISyncPullResponse CreatePullResponse(string reponse)
        {
            var r = jFactory.Deserialize<HttpPullResponse>(reponse);
            r.Streams = Translator.In(r.DTOs);
            return r;
        }

        public ISyncPushResponse CreatePushResponse(string response)
        {
            return jFactory.Deserialize<HttpPushResponse>(response);
        }
    }
}
