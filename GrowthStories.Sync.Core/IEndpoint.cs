using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Growthstories.Sync
{
    public interface IEndpoint
    {
        string Name { get; }
        Uri PullUri { get; }
        Uri PushUri { get; }
        Uri AuthUri { get; }
        Uri ClearDBUri { get; }


        Uri UserListUri(string username);

        Uri PhotoUploadUri { get; }
    }

    public class Endpoint : IEndpoint
    {
        private readonly Uri BaseUri;

        public Endpoint(Uri baseUri)
        {
            BaseUri = baseUri;
        }

        public string Name
        {
            get { return "Staging"; }
        }

        public Uri PushUri
        {
            get
            {
                return new Uri(BaseUri, "/api/push");
            }
        }

        public Uri PullUri
        {
            get
            {
                return new Uri(BaseUri, "/api/pull");
            }
        }


        public Uri AuthUri
        {
            get
            {
                return new Uri(BaseUri, "/api/auth");
            }
        }


        public Uri ClearDBUri
        {
            get { return new Uri(BaseUri, "/api/test/clearDS"); }
        }


        public Uri UserListUri(string username)
        {
            return new Uri(BaseUri, string.Format("/api/user/list?prefix={0}", username));
        }


        public Uri PhotoUploadUri
        {
            get { return new Uri(BaseUri, "/api/photo/uploadurl"); }
        }
    }
}
