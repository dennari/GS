using Growthstories.Sync;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Growthstories.DomainTests
{
    class StagingEndpoint : IEndpoint
    {
        private Uri _Uri = new Uri("http://server.lan:9000");
        public string Name
        {
            get { return "Staging"; }
        }

        public Uri PushUri
        {
            get
            {
                return new Uri(_Uri, "/api/push");
            }
        }

        public Uri PullUri
        {
            get
            {
                return new Uri(_Uri, "/api/pull");
            }
        }


        public Uri AuthUri
        {
            get
            {
                return new Uri(_Uri, "/api/auth");
            }
        }


        public Uri ClearDBUri
        {
            get { return new Uri(_Uri, "/api/test/clearDS"); }
        }


        public Uri UserListUri(string username)
        {
            return new Uri(_Uri, string.Format("/api/user/list?prefix={0}", username));
        }


        public Uri PhotoUploadUri
        {
            get { return new Uri(_Uri, "/api/photo/uploadurl"); }
        }
    }
}
