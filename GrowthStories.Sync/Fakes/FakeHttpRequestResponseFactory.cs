using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;

namespace Growthstories.Sync
{
    public class FakeHttpRequestResponseFactory : IHttpResponseFactory, IHttpRequestFactory
    {


        public Func<string, string, IAuthTokenResponse> BuildAuthResponse;

        public ISyncPullRequest LastPullRequest;
        public ISyncPushRequest LastPushRequest;
        public string Username;
        public string Password;

        public FakeHttpRequestResponseFactory()
        {
            this.BuildAuthResponse = (u, p) => new HttpAuthTokenResponse("1234", 5600, "1234");
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
