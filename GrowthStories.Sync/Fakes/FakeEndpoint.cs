using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Growthstories.Sync
{
    public class FakeEndpoint : Endpoint
    {
        public FakeEndpoint() : base(new Uri("http://server.lan:9000")) { }
    }
}
