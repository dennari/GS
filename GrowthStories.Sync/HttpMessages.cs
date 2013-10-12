using Growthstories.Sync;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using Growthstories.Domain.Messaging;
using Growthstories.Core;
using System.Net;
using EventStore;
using Growthstories.Domain.Entities;

namespace Growthstories.Sync
{
    public class HttpPullRequest : ISyncPullRequest
    {
        private readonly IJsonFactory jF;


        protected ICollection<SyncStreamInfo> _Streams;
        [JsonIgnore]
        public ICollection<SyncStreamInfo> Streams
        {
            get { return _Streams; }
            set
            {
                _Streams = value;
                OutputStreams = value.Select(x => SyncEventStreamDTO.Translate(x)).ToArray();
            }
        }

        [JsonProperty(PropertyName = Language.STREAMS, Required = Required.Always)]
        public ISyncEventStreamDTO[] OutputStreams { get; protected set; }

        public HttpPullRequest(IJsonFactory jF)
        {
            this.jF = jF;
        }

        public override string ToString()
        {
            return jF.Serialize(this);
        }

    }

    public class HelperPullResponse
    {
        [JsonProperty(PropertyName = Language.EVENTS, Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public IList<EventDTOUnion> DTOs { get; set; }

        [JsonProperty(PropertyName = Language.USE_SINCE, Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public long SyncStamp { get; set; }
    }

    public class HttpPullResponse : HttpResponse, ISyncPullResponse
    {
        public long SyncStamp { get; set; }

        public ICollection<ISyncEventStream> Streams { get; set; }


    }


    public class PhotoUploadUriResponse : HttpResponse, IPhotoUploadUriResponse
    {

        [JsonProperty(PropertyName = "UploadUri", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public Uri UploadUri { get; set; }

    }

    public class UserListResponse : HttpResponse, IUserListResponse
    {

        [JsonProperty(PropertyName = Language.USERS, Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public List<RemoteUser> Users { get; set; }

    }



    public class HttpPushRequest : ISyncPushRequest
    {
        private readonly IJsonFactory jF;

        [JsonProperty(PropertyName = Language.EVENTS)]
        public IEnumerable<IEventDTO> Events { get; set; }

        [JsonProperty(PropertyName = Language.CLIENT_ID)]
        public Guid ClientDatabaseId { get; set; }

        [JsonIgnore]
        public ICollection<ISyncEventStream> Streams { get; set; }

        public HttpPushRequest(IJsonFactory jF)
        {
            this.jF = jF;
        }

        public override string ToString()
        {
            return jF.Serialize(this);
        }



        public bool IsEmpty { get; set; }

    }

    public class HttpPushResponse : HttpResponse, ISyncPushResponse
    {
        public Guid ClientDatabaseId { get; set; }

        public Guid PushId { get; set; }

        public bool AlreadyExecuted { get; set; }

        public Guid LastExecuted { get; set; }


    }

    public class AuthResponse : HttpResponse, IAuthResponse
    {


        public IAuthToken AuthToken { get; set; }

    }

    public class HttpResponse : ISyncResponse
    {
        [JsonProperty(PropertyName = Language.STATUS_CODE, Required = Required.Always)]
        public GSStatusCode StatusCode { get; set; }

        [JsonProperty(PropertyName = Language.STATUS_DESCRIPTION, Required = Required.Always)]
        public string StatusDescription { get; set; }

    }

    public class SyncEventStreamDTO : ISyncEventStreamDTO
    {

        protected SyncEventStreamDTO()
        {

        }

        [JsonProperty(PropertyName = Language.STREAM_TYPE, Required = Required.Always)]
        public string Type { get; protected set; }

        [JsonProperty(PropertyName = Language.STREAM_SINCE, Required = Required.Always)]
        public long SyncStamp { get; protected set; }

        [JsonProperty(PropertyName = Language.STREAM_ENTITY, Required = Required.Always)]
        public Guid StreamId { get; protected set; }

        [JsonProperty(PropertyName = Language.STREAM_ANCESTOR, Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Include)]
        public Guid? StreamAncestorId { get; protected set; }

        public static ISyncEventStreamDTO Translate(ISyncEventStream stream)
        {
            var r = new SyncEventStreamDTO();
            r.SyncStamp = stream.SyncStamp;
            r.StreamId = stream.StreamId;
            r.Type = stream.Type == SyncStreamType.PLANT ? "PLANT" : "USER";

            return r;
        }

        public static ISyncEventStreamDTO Translate(SyncStreamInfo stream)
        {
            var r = new SyncEventStreamDTO();
            r.SyncStamp = stream.SyncStamp;
            r.StreamId = stream.StreamId;
            r.Type = stream.Type == StreamType.PLANT ? "PLANT" : "USER";
            r.StreamAncestorId = stream.AncestorId;

            return r;
        }

    }

}
