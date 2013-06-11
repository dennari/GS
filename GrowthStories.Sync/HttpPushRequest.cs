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


namespace Growthstories.Sync
{
    [JsonObject(MemberSerialization = MemberSerialization.OptOut)]
    public class HttpPushRequest : ISyncPushRequest
    {


        private readonly ITransportEvents Transporter;

        [JsonProperty(PropertyName = "cmds")]
        public ICollection<IEventDTO> Events { get; private set; }
        public Guid PushId { get; private set; }
        public Guid ClientDatabaseId { get; private set; }


        [Inject]
        public HttpPushRequest(ITransportEvents transporter, ICollection<IEventDTO> events)
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
