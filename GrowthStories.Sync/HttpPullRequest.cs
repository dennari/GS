using Growthstories.Sync;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Growthstories.Sync
{
    public class HttpPullRequest : ISyncPullRequest
    {

        [JsonIgnore]
        public ICollection<ISyncEventStream> Streams { get; set; }

    }
}
