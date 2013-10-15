using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using EventStore.Logging;
using Growthstories.Core;
using System.Net.Http.Headers;
using System.Net;
using Growthstories.Domain.Messaging;
using EventStore.Persistence;
using EventStore;

namespace Growthstories.Sync
{

    public class RequestResponseFactory : IRequestFactory, IResponseFactory
    {
        private readonly IJsonFactory jFactory;
        private readonly ITranslateEvents Translator;
        private readonly IPersistSyncStreams SyncPersistence;
        private static ILog Logger = LogFactory.BuildLogger(typeof(RequestResponseFactory));
        private readonly ICommitEvents Persistence;

        public RequestResponseFactory(
            ITranslateEvents translator,
            IJsonFactory jFactory,
            ICommitEvents persistence,
            IPersistSyncStreams syncPersistence)
        {
            this.jFactory = jFactory;
            this.Translator = translator;
            this.SyncPersistence = syncPersistence;
            this.Persistence = persistence;
        }


        public List<ISyncEventStream> MatchStreams(ISyncPullResponse resp, ISyncRequest req)
        {
            List<ISyncEventStream> unmatched = new List<ISyncEventStream>();
            //foreach (var g in resp.Events)
            //{
            //    ISyncEventStream match = req.Streams.FirstOrDefault(x => x.StreamId == g.Key);
            //    if (match == null)
            //    {
            //        match = new SyncEventStream(g.Key, this.Persistence, this.SyncPersistence);
            //        unmatched.Add(match);
            //    }
            //    foreach (var e in g.OrderBy(y => y.AggregateVersion))
            //        match.AddRemote(e);
            //}
            return unmatched;
        }

        protected GSStatusCode HandleHttpResponse(HttpResponseMessage resp)
        {
            if (resp.IsSuccessStatusCode)
                return GSStatusCode.OK;
            return GSStatusCode.FAIL;
        }

        public ISyncPushRequest CreatePushRequest()
        {

            return CreatePushRequest(GetPushStreams());
        }

        private IEnumerable<ISyncEventStream> GetPushStreams()
        {
            foreach (var commits in SyncPersistence.GetUnsynchronizedCommits().GroupBy(x => x.StreamId))
            {
                yield return new SyncEventStream(commits, Persistence, SyncPersistence);
            }
        }

        public ISyncPushRequest CreatePushRequest(IEnumerable<ISyncEventStream> streams)
        {
            var streamsC = streams.ToArray();

            var ee = Translator.Out(streamsC).ToArray();
            var req = new HttpPushRequest(jFactory)
            {
                Events = ee,
                IsEmpty = ee.Length == 0,
                Streams = streamsC,
                //PushId = Guid.NewGuid(),
                ClientDatabaseId = Guid.NewGuid()
            };
            req.SetTranslator(Translator);

            return req;
        }


        //public ISyncPullRequest CreatePullRequest()
        //{

        //    return CreatePullRequest(GetPullStreams());
        //}

        private IEnumerable<ISyncEventStream> GetPullStreams()
        {
            foreach (var streamHead in SyncPersistence.GetAllSyncStreams())
            {
                yield return new SyncEventStream(streamHead, Persistence, SyncPersistence);
            }
        }

        public ISyncPullRequest CreatePullRequest(ICollection<SyncStreamInfo> streams)
        {
            //var streamsC = streams.ToArray();
            return new HttpPullRequest(jFactory)
            {
                Streams = streams
            };
            //return req;
        }

        public ISyncPullResponse CreatePullResponse(HttpResponseMessage resp, string content = null)
        {

            var r = new HttpPullResponse();
            r.StatusCode = HandleHttpResponse(resp);

            if (r.StatusCode == GSStatusCode.OK)
            {
                var helper = jFactory.Deserialize<HelperPullResponse>(content);
                r.SyncStamp = helper.SyncStamp;

                if (helper.DTOs != null && helper.DTOs.Count > 0)
                {
                    r.Streams = helper.DTOs
                        .Select(x => Translator.In(x))
                        .OfType<EventBase>()
                        .GroupBy(x => x.StreamEntityId ?? x.AggregateId)
                        .Select(x => new SyncEventStream(x, Persistence, SyncPersistence) { SyncStamp = helper.SyncStamp })
                        .ToArray();
                }
            }
            //r.Translate = () => r.Streams = Translator.In(r.DTOs);
            return r;
        }

        public IAuthResponse CreateAuthResponse(HttpResponseMessage resp, string content = null)
        {

            var r = new AuthResponse();
            try
            {
                r.AuthToken = jFactory.Deserialize<AuthToken>(content);
                if (string.IsNullOrWhiteSpace(r.AuthToken.AccessToken)
                    || string.IsNullOrWhiteSpace(r.AuthToken.RefreshToken)
                    || r.AuthToken.ExpiresIn < 60)
                    throw new InvalidOperationException();
                r.StatusCode = GSStatusCode.OK;
            }
            catch
            {
                r.StatusCode = GSStatusCode.INTERNAL_SERVER_ERROR;
            }
            return r;
        }


        public IUserListResponse CreateUserListResponse(HttpResponseMessage resp, string content = null)
        {

            var r = new UserListResponse();
            try
            {
                r.Users = jFactory.Deserialize<List<RemoteUser>>(content);
                r.StatusCode = GSStatusCode.OK;
            }
            catch
            {
                r.StatusCode = GSStatusCode.INTERNAL_SERVER_ERROR;
            }
            return r;
        }

        public IPhotoUploadUriResponse CreatePhotoUploadUriResponse(HttpResponseMessage resp, string content = null)
        {

            return new PhotoUploadUriResponse()
            {
                StatusCode = GSStatusCode.OK,
                UploadUri = new Uri(content, UriKind.Absolute)
            };

        }


        public ISyncPushResponse CreatePushResponse(HttpResponseMessage resp, string content = null)
        {

            if (!resp.IsSuccessStatusCode)
                return new HttpPushResponse()
                {
                    StatusCode = GSStatusCode.FAIL
                };

            return jFactory.Deserialize<HttpPushResponse>(content);
        }


    }
}
