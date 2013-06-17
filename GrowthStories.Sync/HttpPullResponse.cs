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


namespace Growthstories.Sync
{
    public class HttpPullResponse : ISyncPullResponse
    {

        public HttpPullResponse() { }


        [JsonProperty(PropertyName = "cmds")]
        public IEnumerable<IEventDTO> Events
        {
            get;
            set;
        }



    }
}
