using Growthstories.Sync;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;



namespace Growthstories.Sync
{
    public class HttpPushRequest : ISyncPushRequest
    {

        [JsonProperty(PropertyName = Language.EVENTS)]
        public IEnumerable<IEventDTO> Events { get; set; }

        [JsonProperty(PropertyName = Language.PUSH_ID)]
        public Guid PushId { get; set; }

        [JsonProperty(PropertyName = Language.CLIENT_ID)]
        public Guid ClientDatabaseId { get; set; }

        [JsonIgnore]
        public ICollection<ISyncEventStream> Streams { get; set; }

    }
}
