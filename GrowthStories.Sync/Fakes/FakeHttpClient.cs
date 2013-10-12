using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Growthstories.Sync
{
    public class FakeHttpClient : IHttpClient, ITransportEvents
    {


        private readonly IResponseFactory ResponseFactory;


        public FakeHttpClient(
            IResponseFactory responseFactory
            )
        {
            this.ResponseFactory = responseFactory;
        }




        public Task<HttpResponseMessage> SendAsync(HttpRequestMessage request)
        {
            return Task.Run(() =>
            {

                return new HttpResponseMessage()
                {
                    Content = new StringContent(
                        "",
                        Encoding.UTF8,
                        "application/json"
                        )
                };
            });
        }

        public Task<string> SendAndGetBodyAsync(HttpRequestMessage request)
        {
            return Task.Run(() =>
            {
                return "";
            });
        }

        public Task<ISyncPushResponse> PushAsync(ISyncPushRequest request)
        {
            throw new NotImplementedException();
        }


        public Task<ISyncPullResponse> PullAsync(ISyncPullRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<IAuthResponse> RequestAuthAsync(string username, string password)
        {
            throw new NotImplementedException();

        }


        protected IAuthToken _AuthToken;
        public IAuthToken AuthToken
        {
            get
            {
                return _AuthToken;
            }
            set
            {
                _AuthToken = value;
            }
        }



        public Task<IUserListResponse> ListUsersAsync(string username)
        {
            throw new NotImplementedException();
        }


        public Task<IPhotoUploadUriResponse> RequestPhotoUploadUri()
        {
            throw new NotImplementedException();
        }
    }
}
