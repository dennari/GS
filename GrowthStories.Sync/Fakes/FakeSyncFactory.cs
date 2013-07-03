using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;

namespace Growthstories.Sync
{
    public class FakeSyncFactory : IResponseFactory, IHttpRequestFactory
    {

        public Func<ISyncPushRequest, ISyncPushResponse> BuildPushResponse;

        public Func<ISyncPullRequest, Tuple<ISyncPullResponse, Func<ISyncPushRequest, ISyncPushResponse>>> BuildPullResponse;

        public Func<string, string, IAuthTokenResponse> BuildAuthResponse;

        private ISyncPullRequest LastPullRequest;
        private ISyncPushRequest LastPushRequest;
        private string Username;
        private string Password;




        public ISyncPullResponse CreatePullResponse(string reponse)
        {
            var r = BuildPullResponse(LastPullRequest);
            if (r.Item2 != null)
                this.BuildPushResponse = r.Item2;
            return r.Item1;
        }

        public ISyncPushResponse CreatePushResponse(string response)
        {
            return BuildPushResponse(LastPushRequest);
        }

        public IAuthTokenResponse CreateAuthTokenResponse(string response)
        {
            return BuildAuthResponse(Username, Password);
        }

        public HttpRequestMessage CreatePushRequest(ISyncPushRequest req)
        {
            this.LastPushRequest = req;
            return new HttpRequestMessage();
        }

        public HttpRequestMessage CreatePullRequest(ISyncPullRequest req)
        {
            this.LastPullRequest = req;
            return new HttpRequestMessage();
        }

        public HttpRequestMessage CreateAuthTokenRequest(string username, string password)
        {
            this.Username = username;
            this.Password = password;
            return new HttpRequestMessage();
        }
    }
}
