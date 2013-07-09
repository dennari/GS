using Growthstories.Core;
using Growthstories.Domain.Messaging;
using Growthstories.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Growthstories.Sync
{
    public class AuthTokenService : ProjectionBase, IAsyncEventHandler<UserSynchronized>
    {
        private IHttpClient client;
        private IResponseFactory ResponseFactory;
        private IHttpRequestFactory RequestFactory;

        public AuthTokenService(
            IHttpClient client,
            IHttpRequestFactory requestFactory,
            IResponseFactory responseFactory)
        {
            this.client = client;
            this.ResponseFactory = responseFactory;
            this.RequestFactory = requestFactory;
        }

        public Task HandleAsync(UserSynchronized @event)
        {
            return Task.Run(async () =>
            {
                var r = RequestFactory.CreateAuthTokenRequest(@event.Username, @event.Password);
                string HttpResponse = await client.SendAndGetBodyAsync(r);
                return ResponseFactory.CreateAuthTokenResponse(HttpResponse);
            });
        }

        public Task<IAuthTokenResponse> GetAuthToken(string username, string password)
        {
            return Task.Run(async () =>
            {
                var r = RequestFactory.CreateAuthTokenRequest(username, password);
                string HttpResponse = await client.SendAndGetBodyAsync(r);
                return ResponseFactory.CreateAuthTokenResponse(HttpResponse);
            });
        }



    }
}
