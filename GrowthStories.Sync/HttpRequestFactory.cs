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

    public class HttpRequestFactory : IRequestFactory, IResponseFactory, IHttpRequestFactory
    {
        private readonly IJsonFactory jFactory;
        private readonly ITranslateEvents Translator;
        private static ILog Logger = LogFactory.BuildLogger(typeof(HttpRequestFactory));
        private IEndpoint Endpoint;
        private IUserService UserService;

        public HttpRequestFactory(ITranslateEvents translator, IJsonFactory jFactory, IEndpoint endpoint, IUserService currentUser)
        {
            this.jFactory = jFactory;
            this.Translator = translator;
            this.Endpoint = endpoint;
            this.UserService = currentUser;
        }

        public ISyncPushRequest CreatePushRequest(IEnumerable<ISyncEventStream> streams)
        {
            var streamsC = streams.ToArray();
            //var logS = new StringBuilder();
            //foreach (var stream in streamsC)
            //{
            //    foreach (var e in stream.CommittedEvents)
            //    {

            //        logS.AppendLine(((IEvent)e.Body).ToString());
            //    }
            //}
            //Logger.Info("Syncing events: {0}", logS.ToString());
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
            r.Streams = Translator.In(r.DTOs);
            return r;
        }

        public ISyncPushResponse CreatePushResponse(string response)
        {
            Logger.Info(response);
            return jFactory.Deserialize<HttpPushResponse>(response);
        }

        public IAuthTokenResponse CreateAuthTokenResponse(string response)
        {
            Logger.Info(response);
            return jFactory.Deserialize<HttpAuthTokenResponse>(response);
        }

        public HttpRequestMessage CreateAuthTokenRequest(string username, string password)
        {
            return AddDefaultHeaders(new HttpRequestMessage()
            {
                RequestUri = Endpoint.AuthUri,
                Method = HttpMethod.Post,
                Content = new FormUrlEncodedContent(new Dictionary<string, string>()
                    {
                        {"grant_type","password"},
                        {"client_id","wp"},
                        {"username",username},
                        {"password",password}
                    }
                )
            });
        }


        public HttpRequestMessage CreatePushRequest(ISyncPushRequest req)
        {

            return CreateSyncRequest(req, Endpoint.PushUri);
        }

        public HttpRequestMessage CreatePullRequest(ISyncPullRequest req)
        {
            return CreateSyncRequest(req, Endpoint.PullUri);
        }

        HttpRequestMessage CreateSyncRequest(ISyncRequest req, Uri uri)
        {

            string sContent = jFactory.Serialize(req);
            Logger.Info(sContent);


            return AddDefaultHeaders(new HttpRequestMessage()
            {
                RequestUri = uri,
                Method = HttpMethod.Post,
                Content = new StringContent(
                    sContent,
                    Encoding.UTF8,
                    "application/json"
                )
            });
        }

        protected HttpRequestMessage AddDefaultHeaders(HttpRequestMessage r)
        {
            try
            {
                r.Headers.Authorization = new AuthenticationHeaderValue("Bearer", UserService.CurrentUser.AccessToken);
            }
            catch (Exception) { }

            r.Headers.UserAgent.Add(new ProductInfoHeaderValue("GrowthStories", "v0.1"));
            return r;
        }

    }
}
