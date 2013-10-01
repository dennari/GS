using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using EventStore.Logging;
using Growthstories.Core;
using System.Net.Http.Headers;


namespace Growthstories.Sync
{

    public class RequestResponseFactory : IRequestFactory, IResponseFactory
    {
        private readonly IJsonFactory jFactory;
        private readonly ITranslateEvents Translator;
        private static ILog Logger = LogFactory.BuildLogger(typeof(RequestResponseFactory));

        public RequestResponseFactory(ITranslateEvents translator, IJsonFactory jFactory)
        {
            this.jFactory = jFactory;
            this.Translator = translator;
        }

        public ISyncPushRequest CreatePushRequest(IEnumerable<ISyncEventStream> streams)
        {
            var streamsC = streams.ToArray();

            //var ee = Translator.Out(streamsC).ToArray();
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
            Logger.Info(reponse);
            var r = jFactory.Deserialize<HttpPullResponse>(reponse);
            if (r.DTOs != null && r.DTOs.Count > 0)
            {
                r.StatusCode = 200;
                r.Events = r.DTOs.Select(x => Translator.In(x));
            }

            //r.Translate = () => r.Streams = Translator.In(r.DTOs);
            return r;
        }

        public ISyncPushResponse CreatePushResponse(string response)
        {
            Logger.Info(response);
            return jFactory.Deserialize<HttpPushResponse>(response);
        }


    }
}
