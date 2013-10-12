
using EventStore.Logging;
using Growthstories.Domain.Entities;
using System;
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


        public SyncHttpClient(IResponseFactory responseFactory, IEndpoint endpoint)
        {
            this.Endpoint = endpoint;
            this.ResponseFactory = responseFactory;
        }



        public Task<ISyncPushResponse> PushAsync(ISyncPushRequest request)
        {
            return Task.Run<ISyncPushResponse>(async () =>
            {
                var resp = await SendAndGetBodyAsync(CreatePushRequest(request));
                return ResponseFactory.CreatePushResponse(resp.Item1, resp.Item2);

            });
        }


        public Task<ISyncPullResponse> PullAsync(ISyncPullRequest request)
        {
            return Task.Run<ISyncPullResponse>(async () =>
            {
                var resp = await SendAndGetBodyAsync(CreatePullRequest(request));
                return ResponseFactory.CreatePullResponse(resp.Item1, resp.Item2);
            });
        }

        public Task<IAuthResponse> RequestAuthAsync(string username, string password)
        {
            return Task.Run<IAuthResponse>(async () =>
            {
                var resp = await SendAndGetBodyAsync(CreateAuthRequest(username, password));
                return ResponseFactory.CreateAuthResponse(resp.Item1, resp.Item2);
            });
        }


        public Task<IUserListResponse> ListUsersAsync(string username)
        {
            return Task.Run(async () =>
            {
                var request = new HttpRequestMessage(HttpMethod.Get, Endpoint.UserListUri(username));
                var resp = await SendAndGetBodyAsync(request);
                return ResponseFactory.CreateUserListResponse(resp.Item1, resp.Item2);
            });
        }

        public Task<IPhotoUploadUriResponse> RequestPhotoUploadUri()
        {
            return Task.Run(async () =>
            {
                var resp = await SendAndGetBodyAsync(new HttpRequestMessage(HttpMethod.Post,
                        Endpoint.PhotoUploadUri));
                return ((RequestResponseFactory)ResponseFactory).CreatePhotoUploadUriResponse(resp.Item1, resp.Item2);
            });
        }

        public Task<HttpResponseMessage> Upload(Uri uri, Stream file)
        {
            return Task.Run(async () =>
            {
                var req = new HttpRequestMessage(HttpMethod.Post, uri);
                var form = new MultipartFormDataContent();
                form.Add(new StreamContent(file), "file", "filename");

                req.Content = form;


                return await SendAsync(req);

            });
        }


        public Task<HttpResponseMessage> SendAsync(HttpRequestMessage request)
        {
            if (Client == null)
                InitClient();

            return Client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
        }

        protected Task<Tuple<HttpResponseMessage, string>> SendAndGetBodyAsync(HttpRequestMessage request)
        {
            return Task.Run(async () =>
            {
                var r = await SendAsync(request);
                var s = await r.Content.ReadAsStringAsync();
                Logger.Info("[RESPONSEBODY]\n" + s);
                return Tuple.Create(r, s);

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
                Handler.AuthToken = value;
            }
        }

        public HttpRequestMessage CreateAuthRequest(string username, string password)
        {
            return new HttpRequestMessage()
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


            if (request.Method == HttpMethod.Post && AuthToken != null)
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
