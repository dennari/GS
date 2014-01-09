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


        public FakeHttpClient(IResponseFactory responseFactory)
        {
            this.ResponseFactory = responseFactory;
        }

        public Task<APIRegisterResponse> RegisterAsync(string username, string email, string password)
        {
            throw new NotImplementedException();
        }

        public Task<HttpResponseMessage> SendAsync(HttpRequestMessage request)
        {
            throw new NotImplementedException();

        }

        public Task<string> SendAndGetBodyAsync(HttpRequestMessage request)
        {
            throw new NotImplementedException();
        }

        public Task<ISyncPushResponse> PushAsync(ISyncPushRequest request)
        {
            return Task.FromResult(this.PushResponse(request));
        }

        private ISyncPushResponse PushResponse(ISyncPushRequest request)
        {
            if (PushResponseFactory == null)
                return new HttpPushResponse()
                {
                    StatusCode = GSStatusCode.OK
                };
            else
                return PushResponseFactory(request);
        }

        private ISyncPullResponse PullResponse(ISyncPullRequest request)
        {
            if (PullResponseFactory == null)
                return new HttpPullResponse()
                {
                    StatusCode = GSStatusCode.OK
                };
            else
                return PullResponseFactory(request);
        }


        public Func<ISyncPushRequest, ISyncPushResponse> PushResponseFactory { get; set; }

        public Func<ISyncPullRequest, ISyncPullResponse> PullResponseFactory { get; set; }

        public Func<string, IUserListResponse> ListUsersFactory { get; set; }




        public Task<ISyncPullResponse> PullAsync(ISyncPullRequest request)
        {
            return Task.FromResult(this.PullResponse(request));

        }

        public Task<IAuthResponse> RequestAuthAsync(string username, string password)
        {
            return Task.FromResult<IAuthResponse>(new AuthResponse()
            {
                AuthToken = new AuthToken("sdfgsd", 3600, "dfgdfg"),
                StatusCode = GSStatusCode.OK,
                StatusDescription = "OK"
            });

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
            return Task.FromResult(ListUsersFactory(username));
        }


        public Func<IPhotoUriResponse> PhotoUploadUriResponseFactory { get; set; }


        public Task<IPhotoUriResponse> RequestPhotoUploadUri()
        {
            if (PhotoUploadUriResponseFactory != null)
                return Task.FromResult(PhotoUploadUriResponseFactory());

            return Task.FromResult<IPhotoUriResponse>(new PhotoUriResponse()
            {
                StatusCode = GSStatusCode.OK,
                PhotoUri = new Uri("http://random.com")
            });
        }

        public Func<string, IPhotoUriResponse> PhotoDownloadUriResponseFactory { get; set; }


        public Task<IPhotoUriResponse> RequestPhotoDownloadUri(string blobKey)
        {

            if (PhotoUploadUriResponseFactory != null)
                return Task.FromResult(PhotoDownloadUriResponseFactory(blobKey));

            return Task.FromResult<IPhotoUriResponse>(new PhotoUriResponse()
            {
                StatusCode = GSStatusCode.OK,
                PhotoUri = new Uri("http://upload.wikimedia.org/wikipedia/commons/e/e3/CentaureaCyanus-bloem-kl.jpg")
            });

        }


        public Func<IPhotoUploadRequest, IPhotoUploadResponse> PhotoUploadResponseFactory { get; set; }


        public Task<IPhotoUploadResponse> RequestPhotoUpload(IPhotoUploadRequest request)
        {
            if (PhotoUploadResponseFactory != null)
                return Task.FromResult(PhotoUploadResponseFactory(request));

            return Task.FromResult<IPhotoUploadResponse>(new PhotoUploadResponse()
            {
                StatusCode = GSStatusCode.OK,
                Photo = request.Photo
            });
        }

        public Func<IPhotoDownloadRequest, Task<IPhotoDownloadResponse>> PhotoDownloadResponseFactory { get; set; }

        public Task<IPhotoDownloadResponse> RequestPhotoDownload(IPhotoDownloadRequest request)
        {
            if (PhotoDownloadResponseFactory != null)
                return PhotoDownloadResponseFactory(request);

            return Task.FromResult<IPhotoDownloadResponse>(new PhotoDownloadResponse()
            {
                StatusCode = GSStatusCode.OK,
                Photo = request.Photo
            });
        }


        public Task<RemoteUser> UserInfoAsync(string email)
        {
            throw new NotImplementedException();
        }
    }
}
