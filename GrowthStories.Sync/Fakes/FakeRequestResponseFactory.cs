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

        //private ISyncPullRequest LastPullRequest;
        private ISyncPushRequest LastPushRequest;

        private ITranslateEvents Translator;
        private IJsonFactory jFac;

        public FakeRequestResponseFactory(ITranslateEvents translator, IJsonFactory jFac)
        {
            this.Translator = translator;
            this.jFac = jFac;
        }

        public IAuthResponse CreateAuthResponse(string response)
        {
            return new AuthResponse()
            {
                AuthToken = new AuthToken("gfdg", 7200, "fghfghf"),
                StatusCode = GSStatusCode.OK,
                StatusDescription = "OK"
            };


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
            var req = new HttpPushRequest(jFac)
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
            //var streamsC = streams.ToArray();
            //var req = new HttpPullRequest(jFac)
            //{
            //    Streams = streamsC
            //};
            //this.LastPullRequest = req;
            //return req;
            throw new NotImplementedException();
        }


        public IUserListResponse CreateUserListResponse(string response)
        {
            throw new NotImplementedException();
        }


        public ISyncPushRequest CreatePushRequest()
        {
            throw new NotImplementedException();
        }

        public ISyncPullRequest CreatePullRequest()
        {
            throw new NotImplementedException();
        }

        public List<ISyncEventStream> MatchStreams(ISyncPullResponse resp, ISyncRequest req)
        {
            throw new NotImplementedException();
        }


        public ISyncPullRequest CreatePullRequest(ICollection<SyncStreamInfo> streams)
        {
            throw new NotImplementedException();
        }

        public ISyncPullResponse CreatePullResponse(HttpResponseMessage resp, string content = null)
        {
            throw new NotImplementedException();
        }

        public ISyncPushResponse CreatePushResponse(HttpResponseMessage resp, string content = null)
        {
            throw new NotImplementedException();
        }

        public IAuthResponse CreateAuthResponse(HttpResponseMessage resp, string content = null)
        {
            throw new NotImplementedException();
        }

        public IUserListResponse CreateUserListResponse(HttpResponseMessage resp, string content = null)
        {
            throw new NotImplementedException();
        }
    }
}
