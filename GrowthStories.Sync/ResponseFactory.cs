﻿using System;
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
using System.Diagnostics;

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
                try
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
                                    Logger.Info("skipping out-of-sequence stream {0} {1}", x.Stream.StreamId, x.Stream.AncestorId);

                                }
                                // TODO/JOJ: is empty try catch and returning null a good idea?
                                return null;
                            })
                            .Where(x => x != null)
                            .ToArray();
                        //r.Streams = r.Projections.SelectMany(x => x.Segments.Values).ToArray();
                    }
                }
                catch
                {
                    Logger.Warn("Invalid pull response");
                    r.StatusCode = GSStatusCode.FAIL;
                }

            }
            //r.Translate = () => r.Streams = Translator.In(r.DTOs);
            return r;
        }


        public ISyncPushResponse CreatePushResponse(ISyncPushRequest req, Tuple<HttpResponseMessage, string> resp)
        {
            try
            {
                var ret = jFactory.Deserialize<HttpPushResponse>(resp.Item2);
                return ret;
            }
            catch
            {
                Logger.Warn("Invalid push response");
                return new HttpPushResponse()
                {
                    StatusCode = GSStatusCode.FAIL
                };
            }
        }


        public IAuthResponse CreateAuthResponse(Tuple<HttpResponseMessage, string> resp)
        {
            var r = CreateWithStatusCode<AuthResponse>(resp.Item1);

            if (r.StatusCode == GSStatusCode.OK)
            {
                try
                {
                    r.AuthToken = jFactory.Deserialize<AuthToken>(resp.Item2);
                    if (string.IsNullOrWhiteSpace(r.AuthToken.AccessToken)
                        || string.IsNullOrWhiteSpace(r.AuthToken.RefreshToken)
                        || r.AuthToken.ExpiresIn < 60)
                    {
                        Logger.Warn("Invalid auth response");
                        r.StatusCode = GSStatusCode.FAIL;
                    }
                }
                catch
                {
                    Logger.Warn("Invalid auth response (could not parse json)");
                    r.StatusCode = GSStatusCode.FAIL;
                }
            }
            return r;
        }


        public IUserListResponse CreateUserListResponse(Tuple<HttpResponseMessage, string> resp)
        {
            var r = CreateWithStatusCode<UserListResponse>(resp.Item1);

            if (r.StatusCode == GSStatusCode.OK)
            {
                try
                {
                    r.Users = jFactory.Deserialize<List<RemoteUser>>(resp.Item2);
                }
                catch
                {
                    Logger.Warn("Invalid user list response");
                    r.StatusCode = GSStatusCode.FAIL;
                }
            }

            return r;
        }


        public APIRegisterResponse CreateRegisterResponse(Tuple<HttpResponseMessage, string> resp)
        {
            APIRegisterResponse ret;

            if (resp.Item1.IsSuccessStatusCode)
            {
                try
                {
                    ret = jFactory.Deserialize<APIRegisterResponse>(resp.Item2);
                }
                catch
                {
                    Logger.Info("Received invalid register response");
                    ret = new APIRegisterResponse();
                    ret.HttpStatus = HttpStatusCode.InternalServerError;
                    return ret;
                }
            }
            else
            {
                ret = new APIRegisterResponse();
            }
            ret.HttpStatus = resp.Item1.StatusCode;

            return ret;
        }

        //
        // if this fails, it returns a FileNotFoundException
        // not sure why it is not a different exception -- JOJ
        // 
        public RemoteUser CreateUserInfoResponse(Tuple<HttpResponseMessage, string> resp)
        {
            //var r = CreateWithStatusCode<UserListResponse>(resp.Item1);
            if (resp.Item1.IsSuccessStatusCode)
            {
                try
                {
                    return jFactory.Deserialize<RemoteUser>(resp.Item2);
                }
                catch
                {
                    throw new FileNotFoundException("Error occured when trying to retrieve info for user (json deserialise failed)");
                }
            }

            throw new FileNotFoundException("Error occured when trying to retrieve info for user");
        }


        T CreateWithStatusCode<T>(HttpResponseMessage resp) where T : HttpResponse, new()
        {
            return new T()
            {
                StatusCode = GetGSStatusCode(resp.StatusCode)
            };
        }


        public GSStatusCode GetGSStatusCode(HttpStatusCode code)
        {
            var sc = GSStatusCode.FAIL;
            switch (code)
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
            return sc;
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

                if (resp.Item2 == null || resp.Item2.Length < 5)
                {
                    Logger.Info("Photo upload response was invalid");
                    r.StatusCode = GSStatusCode.FAIL;
                }
            }
            return r;
        }


        public IPhotoDownloadResponse CreatePhotoDownloadResponse(IPhotoDownloadRequest req, HttpResponseMessage resp)
        {
            IPhotoDownloadResponse R = null;
            var sc = GetGSStatusCode(resp.StatusCode);
            if (sc == GSStatusCode.OK)
            {
                R = new PhotoDownloadResponse(resp)
                {
                    StatusCode = sc,
                    Photo = req.Photo,
                    PlantActionId = req.PlantActionId
                };
            }
            else
            {
                R = new PhotoDownloadResponse(null)
                {
                    StatusCode = sc
                };
            }
            return R;
        }



    }


}
