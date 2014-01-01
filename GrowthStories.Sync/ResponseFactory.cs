using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using EventStore.Logging;
using Growthstories.Core;
using System.Net.Http.Headers;
using System.Net;
using Growthstories.Domain.Messaging;
using Growthstories.Domain.Entities;
using EventStore.Persistence;
using EventStore;
using System.IO;

namespace Growthstories.Sync
{

    public class ResponseFactory : IResponseFactory
    {
        private readonly IJsonFactory jFactory;
        private readonly ITranslateEvents Translator;
        private static ILog Logger = LogFactory.BuildLogger(typeof(ResponseFactory));

        public ResponseFactory(ITranslateEvents translator, IJsonFactory jFactory)
        {
            this.jFactory = jFactory;
            this.Translator = translator;
        }



        protected GSStatusCode HandleHttpResponse(HttpResponseMessage resp)
        {
            if (resp.IsSuccessStatusCode)
                return GSStatusCode.OK;
            return GSStatusCode.FAIL;
        }



        public ISyncPullResponse CreatePullResponse(ISyncPullRequest req, Tuple<HttpResponseMessage, string> resp)
        {

            var r = CreateWithStatusCode<HttpPullResponse>(resp.Item1);

            if (r.StatusCode == GSStatusCode.OK)
            {
                var helper = jFactory.Deserialize<HelperPullResponse>(resp.Item2);
                //r.SyncStamp = helper.SyncStamp;

                if (helper.Streams != null && helper.Streams.Count > 0)
                {

                    r.Projections = helper.Streams
                        .Where(x => x.ErrorCode == "OK")
                        .Select(x =>
                        {
                            try
                            {
                                x.Stream.Segments = Translator.In(x.DTOs)
                                    .Select(y => (IStreamSegment)(new StreamSegment(y)))
                                    .ToDictionary(y => y.AggregateId);

                                x.Stream.NextSince = x.NextSince;
                                return x.Stream;

                            }
                            catch
                            {

                            }
                            return null;
                        })
                        .Where(x => x != null)
                        .ToArray();

                    //r.Streams = r.Projections.SelectMany(x => x.Segments.Values).ToArray();
                }
            }
            //r.Translate = () => r.Streams = Translator.In(r.DTOs);
            return r;
        }

        public ISyncPushResponse CreatePushResponse(ISyncPushRequest req, Tuple<HttpResponseMessage, string> resp)
        {

            if (!resp.Item1.IsSuccessStatusCode)
                return new HttpPushResponse()
                {
                    StatusCode = GSStatusCode.FAIL
                };

            return jFactory.Deserialize<HttpPushResponse>(resp.Item2);
        }


        public IAuthResponse CreateAuthResponse(Tuple<HttpResponseMessage, string> resp)
        {
            var r = CreateWithStatusCode<AuthResponse>(resp.Item1);

            if (r.StatusCode == GSStatusCode.OK)
            {
                r.AuthToken = jFactory.Deserialize<AuthToken>(resp.Item2);
                if (string.IsNullOrWhiteSpace(r.AuthToken.AccessToken)
                    || string.IsNullOrWhiteSpace(r.AuthToken.RefreshToken)
                    || r.AuthToken.ExpiresIn < 60)
                    throw new InvalidOperationException();
            }

            return r;
        }


        public IUserListResponse CreateUserListResponse(Tuple<HttpResponseMessage, string> resp)
        {
            var r = CreateWithStatusCode<UserListResponse>(resp.Item1);

            if (r.StatusCode == GSStatusCode.OK)
            {
                r.Users = jFactory.Deserialize<List<RemoteUser>>(resp.Item2);
            }

            return r;
        }


        public RemoteUser CreateUserInfoResponse(Tuple<HttpResponseMessage, string> resp)
        {
            //var r = CreateWithStatusCode<UserListResponse>(resp.Item1);
            if (resp.Item1.IsSuccessStatusCode)
            {
                return jFactory.Deserialize<RemoteUser>(resp.Item2);
            }

            // why is this a FileNotFoundException ? -- JOJ
            throw new FileNotFoundException("Error occured when trying to retrieve info for user");
        }

        T CreateWithStatusCode<T>(HttpResponseMessage resp) where T : HttpResponse, new()
        {
            var sc = GSStatusCode.FAIL;
            switch (resp.StatusCode)
            {
                case HttpStatusCode.Forbidden:
                    sc = GSStatusCode.FORBIDDEN;
                    break;

                case HttpStatusCode.OK:
                    sc = GSStatusCode.OK;
                    break;

                case HttpStatusCode.Unauthorized:
                    sc = GSStatusCode.AUTHENTICATION_REQUIRED;
                    break;

                case HttpStatusCode.InternalServerError:
                    sc = GSStatusCode.FORBIDDEN;
                    break;

                case HttpStatusCode.NotFound:
                    // for some reason the HttpClient returna http message with a 
                    // 404 status when server is unreachable
                    // we are not using the 404 code on the http status line currently
                    // for anything, so getting this code means that server is probably
                    // really unreachable (or down)
                    sc = GSStatusCode.SERVER_UNREACHABLE; 
                    break;

                default:
                    sc = GSStatusCode.FAIL;
                    break;
            }

            return new T()
            {
                StatusCode = sc
            };
        }

        //public IPhotoUriResponse CreatePhotoUploadUriResponse(Tuple<HttpResponseMessage, string> resp)
        //{

        //    var r = CreateWithStatusCode<PhotoUploadUriResponse>(resp.Item1);
        //    if (r.StatusCode == GSStatusCode.OK)
        //    {
        //        r.PhotoUri = new Uri(resp.Item2, UriKind.Absolute);
        //    }
        //    return r;


        //}

        public IPhotoUploadResponse CreatePhotoUploadResponse(IPhotoUploadRequest req, Tuple<HttpResponseMessage, string> resp)
        {
            var r = CreateWithStatusCode<PhotoUploadResponse>(resp.Item1);
            if (r.StatusCode == GSStatusCode.OK)
            {
                r.Photo = req.Photo;
                r.PlantActionId = req.PlantActionId;
                r.Photo.BlobKey = resp.Item2;
                r.BlobKey = resp.Item2;
            }
            return r;


        }

        public IPhotoDownloadResponse CreatePhotoDownloadResponse(IPhotoDownloadRequest req, Tuple<HttpResponseMessage, Stream> resp)
        {
            var r = CreateWithStatusCode<PhotoDownloadResponse>(resp.Item1);
            if (r.StatusCode == GSStatusCode.OK)
            {
                r.Photo = req.Photo;
                r.Stream = resp.Item2;
            }
            return r;


        }



    }


}
