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
using Ninject;
using Growthstories.Domain.Messaging;


namespace Growthstories.Sync
{
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class HttpPushRequest : ISyncPushRequest
    {


        private readonly ITransportEvents Transporter;

        [JsonProperty(PropertyName = Language.EVENTS)]
        public IEnumerable<IEventDTO> Events { get; private set; }

        [JsonProperty(PropertyName = Language.PUSH_ID)]
        public Guid PushId { get; private set; }

        [JsonProperty(PropertyName = Language.CLIENT_ID)]
        public Guid ClientDatabaseId { get; private set; }


        [Inject]
        public HttpPushRequest(ITransportEvents transporter, IEnumerable<IEventDTO> events)
        {
            // TODO: Complete member initialization
            Events = events;
            PushId = Guid.NewGuid();
            ClientDatabaseId = Guid.NewGuid();
            Transporter = transporter;

        }



        public Task<ISyncPushResponse> ExecuteAsync()
        {

            return Transporter.PushAsync(this);

        }
    }
}
