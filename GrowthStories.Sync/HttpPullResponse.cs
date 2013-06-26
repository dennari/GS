using Growthstories.Sync;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Net.Http;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Growthstories.Domain.Messaging;
using Growthstories.Core;


namespace Growthstories.Sync
{
    public class HttpPullResponse : ISyncPullResponse
    {

        public HttpPullResponse()
        {

        }

        [JsonProperty(PropertyName = Language.EVENTS, Required = Required.AllowNull)]
        public IList<EventDTOUnion> DTOs { get; set; }

        [JsonIgnore]
        public ICollection<ISyncEventStream> Streams { get; set; }


    }
}
