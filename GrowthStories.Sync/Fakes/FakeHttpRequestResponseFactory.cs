using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;

namespace Growthstories.Sync
{
    public class FakeHttpRequestFactory
    {


        public Func<string, string, IAuthToken> BuildAuthResponse;

        public ISyncPullRequest LastPullRequest;
        public ISyncPushRequest LastPushRequest;
        public string Username;
        public string Password;

        public FakeHttpRequestFactory()
        {
            this.BuildAuthResponse = (u, p) => new AuthToken("1234", 5600, "1234");
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

        public HttpRequestMessage CreateAuthRequest(string username, string password)
        {
            this.Username = username;
            this.Password = password;
            return new HttpRequestMessage();
        }
    }
}
