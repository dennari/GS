using Growthstories.Sync;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using Growthstories.Domain.Messaging;
using Growthstories.Core;

namespace Growthstories.Sync
{
    public class HttpPullRequest : ISyncPullRequest
    {

        protected ICollection<ISyncEventStream> _Streams;
        [JsonIgnore]
        public ICollection<ISyncEventStream> Streams
        {
            get { return _Streams; }
            set
            {
                _Streams = value;
                OutputStreams = value.Select(x => SyncEventStreamDTO.Translate(x)).ToArray();
            }
        }

        [JsonProperty(PropertyName = Language.STREAMS, Required = Required.Always)]
        public ICollection<ISyncEventStreamDTO> OutputStreams { get; set; }

    }

    public class SyncEventStreamDTO : ISyncEventStreamDTO
    {

        protected SyncEventStreamDTO()
        {

        }

        [JsonProperty(PropertyName = Language.STREAM_TYPE, Required = Required.Always)]
        public string Type { get; protected set; }

        [JsonProperty(PropertyName = Language.ENTITY_VERSION_SINCE, Required = Required.Always)]
        public int SinceVersion { get; protected set; }

        [JsonProperty(PropertyName = Language.STREAM_ENTITY, Required = Required.Always)]
        public Guid StreamId { get; protected set; }

        [JsonProperty(PropertyName = Language.STREAM_ANCESTOR, Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Include)]
        public Guid? StreamAncestorId { get; protected set; }

        public static ISyncEventStreamDTO Translate(ISyncEventStream stream)
        {
            var r = new SyncEventStreamDTO();
            r.SinceVersion = stream.StreamRevision - stream.CommittedEvents.Count();
            r.StreamId = stream.StreamId;

            EventBase firstEvent = stream.Events(Growthstories.Core.Extensions.EventTypes.All)
                .OfType<EventBase>()
                .FirstOrDefault();

            if (firstEvent != null)
            {
                r.StreamAncestorId = firstEvent.StreamAncestorId;
                r.Type = firstEvent.StreamType.ToString().ToUpper();
            }



            return r;
        }

    }

}
