using Growthstories.Sync;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Growthstories.DomainTests
{
    class StagingEndpoint : Endpoint
    {
        public StagingEndpoint() : base(new Uri("http://dennari-macbook.lan:8080")) { }
    }
}
