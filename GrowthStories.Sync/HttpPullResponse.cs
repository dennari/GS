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


        [JsonProperty(PropertyName = Language.COMMANDS, Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public IList<EventDTOUnion> DTOs { get; set; }

        [JsonIgnore]
        public IEnumerable<IEvent> Events { get; set; }



        [JsonProperty(PropertyName = Language.STATUS_CODE, Required = Required.Default)]
        public int StatusCode { get; set; }

        [JsonProperty(PropertyName = Language.STATUS_DESCRIPTION, Required = Required.Default)]
        public string StatusDesc { get; set; }
    }
}
