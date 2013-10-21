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

    public class ResponseFactory : IResponseFactory
    {
        private readonly IJsonFactory jFactory;
        private readonly ITranslateEvents Translator;
        private static ILog Logger = LogFactory.BuildLogger(typeof(ResponseFactory));

        public ResponseFactory(ITranslateEvents translator, IJsonFactory jFactory)
        {
            this.jFactory = jFactory;
            this.Translator = translator;
        }



        protected GSStatusCode HandleHttpResponse(HttpResponseMessage resp)
        {
            if (resp.IsSuccessStatusCode)
                return GSStatusCode.OK;
            return GSStatusCode.FAIL;
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
                        .Select(x => new AggregateMessages(x))
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



    public class RequestFactory : IRequestFactory
    {
        private readonly IJsonFactory jFactory;
        private readonly ITranslateEvents Translator;
        private readonly IPersistSyncStreams SyncPersistence;
        private static ILog Logger = LogFactory.BuildLogger(typeof(RequestFactory));
        private readonly ITransportEvents Transporter;

        public RequestFactory(
            ITranslateEvents translator,
            ITransportEvents transporter,
            IJsonFactory jFactory,
            IPersistSyncStreams syncPersistence)
        {
            this.jFactory = jFactory;
            this.Translator = translator;
            this.Transporter = transporter;
            this.SyncPersistence = syncPersistence;
        }



        public ISyncPushRequest CreatePushRequest(int globalSequence)
        {

            return CreatePushRequest(GetPushStreams(globalSequence), globalSequence);
        }

        private IEnumerable<IAggregateMessages> GetPushStreams(int globalSequence)
        {
            foreach (var commits in SyncPersistence.GetUnsynchronizedCommits(globalSequence).GroupBy(x => x.StreamId))
            {
                yield return new AggregateMessages(commits);
            }
        }

        public ISyncPushRequest CreateUserSyncRequest(Guid userId)
        {
            //var streamsC = 

            //var ee = Translator.Out(streamsC).ToArray();

            var commit = SyncPersistence.GetFrom(userId, 0, 1).First();

            var stream = new AggregateMessages(userId);
            stream.AddMessage(commit.ActualEvents().First());

            var req = new HttpPushRequest(jFactory)
            {
                IsEmpty = false,
                Streams = new[] { stream },
                ClientDatabaseId = Guid.NewGuid(),
                Translator = Translator,
                Transporter = Transporter
            };

            return req;
        }


        public ISyncPushRequest CreatePushRequest(IEnumerable<IAggregateMessages> streams, int globalSequence)
        {
            var streamsC = streams.ToArray();

            var req = new HttpPushRequest(jFactory)
            {
                GlobalCommitSequence = globalSequence,
                IsEmpty = false,
                Streams = streamsC,
                ClientDatabaseId = Guid.NewGuid(),
                Translator = Translator,
                Transporter = Transporter
            };


            return req;
        }



        public ISyncPullRequest CreatePullRequest(ICollection<SyncStreamInfo> streams)
        {
            //var streamsC = streams.ToArray();
            return new HttpPullRequest(jFactory)
            {
                Streams = streams,
                Translator = Translator,
                Transporter = Transporter
            };
            //return req;
        }



    }


}
