using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;


namespace Growthstories.Sync
{
    public interface IRequestFactory
    {
        ISyncPushRequest CreatePushRequest(IEnumerable<ISyncEventStream> streams);

        ISyncPullRequest CreatePullRequest(IEnumerable<ISyncEventStream> streams);


    }

    public interface IHttpRequestFactory
    {
        HttpRequestMessage CreatePushRequest(ISyncPushRequest req);

        HttpRequestMessage CreatePullRequest(ISyncPullRequest req);

        HttpRequestMessage CreateAuthTokenRequest(string username, string password);

    }

    public interface IHttpResponseFactory
    {

        IAuthTokenResponse CreateAuthTokenResponse(string response);
    }



}
