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

    public class HttpRequestResponseFactory : IHttpRequestFactory, IHttpResponseFactory
    {
        private readonly IJsonFactory jFactory;
        private static ILog Logger = LogFactory.BuildLogger(typeof(HttpRequestResponseFactory));
        private IEndpoint Endpoint;
        private IAuthTokenResponse Auth;

        public HttpRequestResponseFactory(IJsonFactory jFactory, IEndpoint endpoint)
        {
            this.jFactory = jFactory;
            this.Endpoint = endpoint;
        }



        public IAuthTokenResponse CreateAuthTokenResponse(string response)
        {
            Logger.Info(response);
            this.Auth = jFactory.Deserialize<HttpAuthTokenResponse>(response);
            return this.Auth;
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
            }, false);
        }

        public HttpRequestMessage CreateClearDBRequest()
        {
            return AddDefaultHeaders(new HttpRequestMessage()
            {
                RequestUri = Endpoint.ClearDBUri,
                Method = HttpMethod.Get
            }, false);
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
            Logger.Info("[REQUESTBODY]\n" + sContent);


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

        protected HttpRequestMessage AddDefaultHeaders(HttpRequestMessage r, bool addAuthHeader = true)
        {
            if (addAuthHeader)
            {
                try
                {
                    r.Headers.Authorization = new AuthenticationHeaderValue("Bearer", this.Auth.AccessToken);
                }
                catch (Exception)
                {
                    //throw new InvalidOperationException("User not authenticated");
                }
            }


            r.Headers.UserAgent.Add(new ProductInfoHeaderValue("GrowthStories", "v0.1"));
            return r;
        }


    }
}
