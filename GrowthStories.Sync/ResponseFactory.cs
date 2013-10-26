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
                r.SyncStamp = helper.SyncStamp;

                if (helper.DTOs != null && helper.DTOs.Count > 0)
                {
                    r.Streams = helper.DTOs
                        .Select(x => Translator.In(x))
                        .OfType<EventBase>()
                        .GroupBy(x => x.StreamEntityId ?? x.AggregateId)
                        .Select(x => new StreamSegment(x))
                        .ToArray();
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

        T CreateWithStatusCode<T>(HttpResponseMessage resp) where T : HttpResponse, new()
        {
            return new T()
            {
                StatusCode = resp.IsSuccessStatusCode ? GSStatusCode.OK : GSStatusCode.FAIL
            };
        }

        public IPhotoUploadUriResponse CreatePhotoUploadUriResponse(Tuple<HttpResponseMessage, string> resp)
        {

            var r = CreateWithStatusCode<PhotoUploadUriResponse>(resp.Item1);
            if (r.StatusCode == GSStatusCode.OK)
            {
                r.UploadUri = new Uri(resp.Item2, UriKind.Absolute);
            }
            return r;


        }

        public IPhotoUploadResponse CreatePhotoUploadResponse(IPhotoUploadRequest req, Tuple<HttpResponseMessage, string> resp)
        {
            var r = CreateWithStatusCode<PhotoUploadResponse>(resp.Item1);
            if (r.StatusCode == GSStatusCode.OK)
            {
                r.Photo = req.Photo;
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
