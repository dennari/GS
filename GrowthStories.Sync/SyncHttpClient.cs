﻿
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EventStore.Logging;
using Growthstories.Core;
using Growthstories.Domain;

namespace Growthstories.Sync
{


    public class SyncHttpClient : IHttpClient, ITransportEvents
    {

        HttpClient Client;
        private static ILog Logger = LogFactory.BuildLogger(typeof(SyncHttpClient));
        public const int NormalTimeout = 30;
        public const int UploadTimeout = 300;


        void InitClient()
        {

            Client = new System.Net.Http.HttpClient(this.Handler)
            {
                Timeout = TimeSpan.FromSeconds(NormalTimeout)
            };

            //Client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("GrowthStories", "v0.1"));
        }


        private HttpClient _UploadClient;
        private HttpClient UploadClient
        {

            get
            {

                if (_UploadClient == null)
                {
                    _UploadClient = new HttpClient(this.Handler)
                    {
                        Timeout = TimeSpan.FromSeconds(UploadTimeout)
                    };
                }
                return _UploadClient;
            }

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
            Tuple<HttpResponseMessage, string> response = null;
            using (var rrequest = CreatePushRequest(request))
            using ((response = await SendAndGetBodyAsync(rrequest)).Item1)
                return ResponseFactory.CreatePushResponse(request, response);
        }


        public async Task<ISyncPullResponse> PullAsync(ISyncPullRequest request)
        {
            Tuple<HttpResponseMessage, string> response = null;
            using (var rrequest = CreatePullRequest(request))
            using ((response = await SendAndGetBodyAsync(rrequest)).Item1)
                return ResponseFactory.CreatePullResponse(request, response);

        }


        public async Task<IAuthResponse> RequestAuthAsync(string username, string password)
        {
            Tuple<HttpResponseMessage, string> response = null;
            using (var rrequest = CreateAuthRequest(username, password))
            using ((response = await SendAndGetBodyAsync(rrequest)).Item1)
                return ResponseFactory.CreateAuthResponse(response);
        }


        public async Task<IUserListResponse> ListUsersAsync(string username)
        {

            Tuple<HttpResponseMessage, string> response = null;
            using (var rrequest = new HttpRequestMessage(HttpMethod.Get, Endpoint.UserListUri(username)))
            using ((response = await SendAndGetBodyAsync(rrequest)).Item1)
                return ResponseFactory.CreateUserListResponse(response);

        }


        public async Task<RemoteUser> UserInfoAsync(string email)
        {


            Tuple<HttpResponseMessage, string> response = null;
            using (var rrequest = new HttpRequestMessage(HttpMethod.Get, Endpoint.UserInfoUri(email)))
            using ((response = await SendAndGetBodyAsync(rrequest)).Item1)
                return ResponseFactory.CreateUserInfoResponse(response);
        }


        public async Task<APIRegisterResponse> RegisterAsync(string username, string email, string password)
        {
            Tuple<HttpResponseMessage, string> response = null;
            using (var rrequest = new HttpRequestMessage(HttpMethod.Post, Endpoint.RegisterUri(username, email, password)))
            using ((response = await SendAndGetBodyAsync(rrequest)).Item1)
                return ResponseFactory.CreateRegisterResponse(response);
        }


        public async Task<IPhotoUriResponse> RequestPhotoUploadUri()
        {
            Tuple<HttpResponseMessage, string> response = null;

            using (var request = new HttpRequestMessage(HttpMethod.Post, Endpoint.PhotoUploadUri))
            using ((response = await SendAndGetBodyAsync(request)).Item1)
            {
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

        }


        public async Task<IPhotoUriResponse> RequestPhotoDownloadUri(string blobKey)
        {
            Tuple<HttpResponseMessage, string> response = null;

            using (var request = new HttpRequestMessage(HttpMethod.Get, Endpoint.PhotoDownloadUri(blobKey)))
            using ((response = await SendAndGetBodyAsync(request)).Item1)
            {
                var r = new PhotoUriResponse()
                {
                    StatusCode = GSStatusCode.FAIL
                };
                if (response.Item1.IsSuccessStatusCode 
                    && response.Item2 != null 
                    && response.Item2.StartsWith("http") 
                    && response.Item2.Length > 10)
                {
                    r.PhotoUri = new Uri(response.Item2, UriKind.Absolute);
                    r.StatusCode = GSStatusCode.OK;
                }
                return r;
            }

        }


        public async Task<IPhotoUploadResponse> RequestPhotoUpload(IPhotoUploadRequest req)
        {

            Tuple<HttpResponseMessage, string> response = null;
            using ((response = await Upload(req.UploadUri, req.Stream)).Item1)
                return ResponseFactory.CreatePhotoUploadResponse(req, response);


        }


        public async Task<IPhotoDownloadResponse> RequestPhotoDownload(IPhotoDownloadRequest req)
        {

            return ResponseFactory.CreatePhotoDownloadResponse(req, await Download(req.DownloadUri));
        }


        public async Task<Tuple<HttpResponseMessage, string>> Upload(Uri uri, Stream file)
        {
            using (var req = new HttpRequestMessage(HttpMethod.Post, uri))
            using (var form = new MultipartFormDataContent())
            using (StreamContent c = new StreamContent(file))
            {
                c.Headers.Remove("Content-Disposition");
                c.Headers.TryAddWithoutValidation("Content-Disposition", "form-data; name=\"file\"; filename=\"filename.jpg\"");
                form.Add(c);
                // the content-disposition header has to be set manually because
                // the app engine production parser insist on a space before filename
                // http://stackoverflow.com/questions/2893268/appengine-blobstore-upload-failing-with-a-request-that-works-in-the-development

                req.Content = form;

                string s = null;
                HttpResponseMessage r = new HttpResponseMessage(System.Net.HttpStatusCode.RequestTimeout);
                try
                {
                    r = await UploadClient.SendAsync(req, HttpCompletionOption.ResponseHeadersRead);
                    s = await r.Content.ReadAsStringAsync();

                }
                catch (Exception e)
                {
                    Logger.DebugExceptionExtended("Upload exception", e);
                    //throw e;
                }

                Logger.Info("[RESPONSEBODY]\n{0}", s);
                return Tuple.Create(r, s);
            }
        }



        public async Task<HttpResponseMessage> Download(Uri uri)
        {
            using (var req = new HttpRequestMessage(HttpMethod.Get, uri))
            {
                return await SendAsync(req);

            }
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
                r = new HttpResponseMessage(System.Net.HttpStatusCode.RequestTimeout);
                Logger.Error(e.ToString());
                //throw e;
            }

            Logger.Info("[RESPONSEBODY]\n{0}", s);
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

            Logger.Info("AuthRequest\n{0}", Serializer.Serialize(dict));

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

            Logger.Info("REQUESTBODY\n{0}", sContent);


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


        public static bool HasInternetConnection
        {
            get
            {
                return NetworkInterface.GetIsNetworkAvailable();
            }
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

            Logger.Info("HTTPREQUEST\n{0}", request.ToString());

            return request;
        }


        protected override HttpResponseMessage ProcessResponse(HttpResponseMessage response, CancellationToken cancellationToken)
        {
            Logger.Info("HTTPRESPONSE\n{0}", response.ToString());
            return response;
        }

        //protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        //{

        //}
    }
}
