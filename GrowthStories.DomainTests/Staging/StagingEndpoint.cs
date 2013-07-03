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
        private Uri _Uri = new Uri("http://gs-staging.appspot.com");
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
                return new Uri(_Uri, "/api/push");
            }
        }


        public Uri AuthUri
        {
            get
            {
                return new Uri(_Uri, "/api/auth");
            }
        }
    }
}
