using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;

namespace Growthstories.Sync
{
    public class FakeRequestResponseFactory : IResponseFactory, IRequestFactory
    {

        public Func<ISyncPushRequest, ISyncPushResponse> BuildPushResponse;

        public Func<ISyncPullRequest, Tuple<HttpPullResponse, Func<ISyncPushRequest, ISyncPushResponse>>> BuildPullResponse;

        private ISyncPullRequest LastPullRequest;
        private ISyncPushRequest LastPushRequest;

        private ITranslateEvents Translator;

        public FakeRequestResponseFactory(ITranslateEvents translator)
        {
            this.Translator = translator;
        }


        public ISyncPullResponse CreatePullResponse(string reponse)
        {
            var r = BuildPullResponse(LastPullRequest);
            if (r.Item2 != null)
                this.BuildPushResponse = r.Item2;
            var resp = r.Item1;
            //resp.Streams = Translator.In(resp.DTOs);
            return r.Item1;
        }

        public ISyncPushResponse CreatePushResponse(string response)
        {
            return BuildPushResponse(LastPushRequest);
        }


        public ISyncPushRequest CreatePushRequest(IEnumerable<ISyncEventStream> streams)
        {
            var streamsC = streams.ToArray();

            //var events = streamsC.
            var req = new HttpPushRequest()
            {
                Events = Translator.Out(streamsC).ToArray(),
                Streams = streamsC,
                //PushId = Guid.NewGuid(),
                ClientDatabaseId = Guid.NewGuid()
            };
            this.LastPushRequest = req;
            return req;
        }

        public ISyncPullRequest CreatePullRequest(IEnumerable<ISyncEventStream> streams)
        {
            var streamsC = streams.ToArray();
            var req = new HttpPullRequest()
            {
                Streams = streamsC
            };
            this.LastPullRequest = req;
            return req;
        }
    }
}
