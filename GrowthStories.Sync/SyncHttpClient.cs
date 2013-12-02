
using EventStore.Logging;
using Growthstories.Domain.Entities;
using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Growthstories.Sync
{
    public class SyncHttpClient : IHttpClient, ITransportEvents
    {
        HttpClient Client;
        private static ILog Logger = LogFactory.BuildLogger(typeof(SyncHttpClient));


        void InitClient()
        {

            Client = new System.Net.Http.HttpClient(this.Handler);
            //Client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("GrowthStories", "v0.1"));
        }

        SyncHttpHandler _Handler;
        SyncHttpHandler Handler
        {
            get
            {
                return _Handler ?? (_Handler = new SyncHttpHandler(new HttpClientHandler()));
            }

        }



        private readonly IResponseFactory ResponseFactory;
        private readonly IEndpoint Endpoint;


        public SyncHttpClient(IResponseFactory responseFactory, IEndpoint endpoint, IJsonFactory jFactory)
        {
            this.Endpoint = endpoint;
            this.ResponseFactory = responseFactory;
            this.Serializer = jFactory;
        }



        public async Task<ISyncPushResponse> PushAsync(ISyncPushRequest request)
        {

            return ResponseFactory.CreatePushResponse(request, await SendAndGetBodyAsync(CreatePushRequest(request)));

        }


        public async Task<ISyncPullResponse> PullAsync(ISyncPullRequest request)
        {

            return ResponseFactory.CreatePullResponse(request, await SendAndGetBodyAsync(CreatePullRequest(request)));
        }

        public async Task<IAuthResponse> RequestAuthAsync(string username, string password)
        {
            return ResponseFactory.CreateAuthResponse(await SendAndGetBodyAsync(CreateAuthRequest(username, password)));
        }


        public async Task<IUserListResponse> ListUsersAsync(string username)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, Endpoint.UserListUri(username));

            return ResponseFactory.CreateUserListResponse(await SendAndGetBodyAsync(request));
        }

        public async Task<RemoteUser> UserInfoAsync(string email)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, Endpoint.UserInfoUri(email));

            return ResponseFactory.CreateUserInfoResponse(await SendAndGetBodyAsync(request));
        }

        public async Task<IPhotoUriResponse> RequestPhotoUploadUri()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, Endpoint.PhotoUploadUri);
            var response = await SendAndGetBodyAsync(request);
            var r = new PhotoUriResponse()
            {
                StatusCode = GSStatusCode.FAIL
            };
            if (response.Item1.IsSuccessStatusCode)
            {
                r.PhotoUri = new Uri(response.Item2, UriKind.Absolute);
                r.StatusCode = GSStatusCode.OK;
            }
            return r;
        }

        public async Task<IPhotoUriResponse> RequestPhotoDownloadUri(string blobKey)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, Endpoint.PhotoDownloadUri(blobKey));
            var response = await SendAndGetBodyAsync(request);
            var r = new PhotoUriResponse()
            {
                StatusCode = GSStatusCode.FAIL
            };
            if (response.Item1.IsSuccessStatusCode)
            {
                r.PhotoUri = new Uri(response.Item2, UriKind.Absolute);
                r.StatusCode = GSStatusCode.OK;
            }
            return r;
        }

        public async Task<IPhotoUploadResponse> RequestPhotoUpload(IPhotoUploadRequest req)
        {

            return ResponseFactory.CreatePhotoUploadResponse(req, await Upload(req.UploadUri, req.Stream));
        }

        public async Task<IPhotoDownloadResponse> RequestPhotoDownload(IPhotoDownloadRequest req)
        {

            return ResponseFactory.CreatePhotoDownloadResponse(req, await Download(req.DownloadUri));
        }


        public Task<Tuple<HttpResponseMessage, string>> Upload(Uri uri, Stream file)
        {
            var req = new HttpRequestMessage(HttpMethod.Post, uri);
            var form = new MultipartFormDataContent();
            
            StreamContent c = new StreamContent(file);
            c.Headers.Remove("Content-Disposition");
            c.Headers.TryAddWithoutValidation("Content-Disposition", "form-data; name=\"file\"; filename=\"filename.jpg\"");
            form.Add(c);

            // the content-disposition header has to be set manually because
            // the app engine production parser insist on a space before filename
            // http://stackoverflow.com/questions/2893268/appengine-blobstore-upload-failing-with-a-request-that-works-in-the-development

            req.Content = form;
            
            return SendAndGetBodyAsync(req);

        }

        public async Task<Tuple<HttpResponseMessage, Stream>> Download(Uri uri)
        {
            var req = new HttpRequestMessage(HttpMethod.Get, uri);

            var r = await SendAsync(req);

            Stream c = null;
            if (r.IsSuccessStatusCode)
            {
                c = await r.Content.ReadAsStreamAsync();
            }

            return Tuple.Create(r, c);

        }


        public Task<HttpResponseMessage> SendAsync(HttpRequestMessage request)
        {
            if (Client == null)
                InitClient();

            return Client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
        }

        protected async Task<Tuple<HttpResponseMessage, string>> SendAndGetBodyAsync(HttpRequestMessage request)
        {
            HttpResponseMessage r = null;
            string s = null;
            try
            {
                r = await SendAsync(request);
                s = await r.Content.ReadAsStringAsync();

            }
            catch (Exception e)
            {
                Logger.Error(e.ToString());
                throw e;
            }




            Logger.Info("[RESPONSEBODY]\n" + s);
            return Tuple.Create(r, s);
        }

        protected async Task<Tuple<HttpResponseMessage, string>> SendAndReadIfSuccess(HttpRequestMessage request)
        {
            var r = await SendAsync(request);
            string s = null;
            if (r.IsSuccessStatusCode)
                s = await r.Content.ReadAsStringAsync();
            return Tuple.Create(r, s);
        }

        protected IAuthToken _AuthToken;
        private IJsonFactory Serializer;
        public IAuthToken AuthToken
        {
            get
            {
                return _AuthToken;
            }
            set
            {
                _AuthToken = value;
                Handler.AuthToken = value;
            }
        }

        public HttpRequestMessage CreateAuthRequest(string username, string password)
        {
            var dict = new Dictionary<string, string>()
                    {
                        {"grant_type","password"},
                        {"client_id","wp"},
                        {"username",username},
                        {"password",password}
                    };
            Logger.Info("[REQUESTBODY]\n" + Serializer.Serialize(dict));


            var fContent = new FormUrlEncodedContent(dict);

            //var builder = new UriBuilder(Endpoint.AuthUri);
            //builder.Port = -1;
            //var qs = string.Join(@"&", dict.Select(x => string.Format("{0}={1}", x.Key, x.Value)).ToArray());
            //builder.Query = string.IsNullOrWhiteSpace(builder.Query) ? qs : builder.Query + @"&" + qs;

            return new HttpRequestMessage()
            {
                RequestUri = Endpoint.AuthUri,
                Method = HttpMethod.Post,
                Content = fContent
            };
        }



        public HttpRequestMessage CreateClearDBRequest()
        {
            return new HttpRequestMessage()
            {
                RequestUri = Endpoint.ClearDBUri,
                Method = HttpMethod.Get
            };
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

            string sContent = req.ToString();
            Logger.Info("[REQUESTBODY]\n" + sContent);


            return new HttpRequestMessage()
            {
                RequestUri = uri,
                Method = HttpMethod.Post,
                Content = new StringContent(
                    sContent,
                    Encoding.UTF8,
                    "application/json"
                )
            };
        }







    }

    public class SyncHttpHandler : MessageProcessingHandler
    {

        private static ILog Logger = LogFactory.BuildLogger(typeof(SyncHttpHandler));

        public SyncHttpHandler(HttpMessageHandler innerHandler)
            : base(innerHandler)
        {
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

        protected override HttpRequestMessage ProcessRequest(HttpRequestMessage request, CancellationToken cancellationToken)
        {


            if ((request.Method == HttpMethod.Post || request.Method == HttpMethod.Get) && AuthToken != null)
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", AuthToken.AccessToken);
            }
            request.Headers.UserAgent.Add(new ProductInfoHeaderValue("GrowthStories", "v0.1"));

            Logger.Info("[HTTPREQUEST]\n" + request.ToString());

            return request;

        }

        protected override HttpResponseMessage ProcessResponse(HttpResponseMessage response, CancellationToken cancellationToken)
        {
            Logger.Info("[HTTPRESPONSE]\n" + response.ToString());
            return response;
        }

        //protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        //{

        //}
    }
}
