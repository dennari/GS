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
            var req = new HttpPushRequest(jFactory)
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
            var req = new HttpPullRequest(jFactory)
            {
                Streams = streamsC
            };
            return req;
        }

        public ISyncPullResponse CreatePullResponse(string reponse)
        {
            Logger.Info(reponse);
            var helper = jFactory.Deserialize<HelperPullResponse>(reponse);
            var r = new HttpPullResponse();
            if (helper.DTOs != null && helper.DTOs.Count > 0)
            {
                r.StatusCode = GSStatusCode.OK;
                r.Events = helper.DTOs
                    .Select(x => Translator.In(x))
                    .OfType<EventBase>()
                    .GroupBy(x => x.StreamEntityId ?? x.EntityId);
            }

            //r.Translate = () => r.Streams = Translator.In(r.DTOs);
            return r;
        }

        public IAuthResponse CreateAuthResponse(string response)
        {

            var r = new AuthResponse();
            try
            {
                r.AuthToken = jFactory.Deserialize<AuthToken>(response);
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


        public IUserListResponse CreateUserListResponse(string response)
        {

            var r = new UserListResponse();
            try
            {
                r.Users = jFactory.Deserialize<List<RemoteUser>>(response);
                r.StatusCode = GSStatusCode.OK;
            }
            catch
            {
                r.StatusCode = GSStatusCode.INTERNAL_SERVER_ERROR;
            }
            return r;
        }

        public IPhotoUploadUriResponse CreatePhotoUploadUriResponse(string response)
        {

            return new PhotoUploadUriResponse()
            {
                StatusCode = GSStatusCode.OK,
                UploadUri = new Uri(response, UriKind.Absolute)
            };

        }


        public ISyncPushResponse CreatePushResponse(string response)
        {
            Logger.Info(response);
            return jFactory.Deserialize<HttpPushResponse>(response);
        }


    }
}
