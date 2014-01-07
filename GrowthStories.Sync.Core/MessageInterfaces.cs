
using CommonDomain;
using Growthstories.Core;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;


namespace Growthstories.Sync
{


    public interface IUserListResponse : ISyncResponse
    {
        List<RemoteUser> Users { get; }
    }

    public interface IPhotoUriResponse : ISyncResponse
    {
        Uri PhotoUri { get; }
    }

    public interface IPhotoUploadRequest : ISyncRequest
    {
        Uri UploadUri { get; }
        Photo Photo { get; }
        Guid PlantActionId { get; }
        Task<IPhotoUploadResponse> GetResponse();
        Stream Stream { get; }
    }

    public interface IPhotoUploadResponse : ISyncResponse
    {
        string BlobKey { get; }
        Photo Photo { get; }
        Guid PlantActionId { get; }

    }

    public interface IPhotoDownloadRequest : ISyncRequest
    {
        Uri DownloadUri { get; }
        Photo Photo { get; }
        Task<IPhotoDownloadResponse> GetResponse();
    }

    public interface IPhotoDownloadResponse : ISyncResponse
    {
        Photo Photo { get; }
        Stream Stream { get; }

    }




    public interface IAuthResponse : ISyncResponse
    {
        IAuthToken AuthToken { get; }
    }

    public interface IAuthToken
    {
        string AccessToken { get; set; }
        int ExpiresIn { get; set; }
        string RefreshToken { get; set; }
    }

    public interface IAuthUser : IAuthToken, IMemento
    {
        string Username { get; }
        string Password { get; }
        string Email { get; }
        Guid GardenId { get; }
        bool IsCollaborator { get; }
        bool IsRegistered { get; }
    }


    public sealed class PullStream
    {

        [JsonProperty(PropertyName = Language.STREAM_TYPE, Required = Required.Always)]
        public PullStreamType Type { get; private set; }

        [JsonProperty(PropertyName = Language.STREAM_SINCE, Required = Required.Default)]
        public long Since { get; set; }

        [JsonProperty(PropertyName = Language.STREAM_ENTITY, Required = Required.Always)]
        public Guid StreamId { get; private set; }

        [JsonProperty(PropertyName = Language.STREAM_ANCESTOR, Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public Guid? AncestorId { get; private set; }

        [JsonIgnore]
        public IDictionary<Guid, IStreamSegment> Segments { get; set; }

        [JsonProperty(PropertyName = Language.NEXT_SINCE, Required = Required.Default)]
        public long NextSince { get; set; }

        private PullStream()
        {

        }

        public PullStream(Guid streamId, PullStreamType type, Guid? ancestorId = null)
        {
            this.Type = type;
            this.StreamId = streamId;
            this.AncestorId = ancestorId;
            //this.Since = since;
        }

        public PullStream(Guid streamId, PullStreamType type, Dictionary<Guid, IStreamSegment> segments, long nextSince)
        {
            this.Type = type;
            this.StreamId = streamId;
            this.NextSince = nextSince;
            this.Segments = segments;
        }
    }

    public interface ISyncPullRequest : ISyncRequest
    {
        ICollection<PullStream> Streams { get; }
        Task<ISyncPullResponse> GetResponse();

    }


    public interface ISyncPullResponse : ISyncResponse
    {
        //IEnumerable<IGrouping<Guid, IEvent>> Events { get; }
        ICollection<PullStream> Projections { get; }
        ICollection<IStreamSegment> Streams { get; }

        //long SyncStamp { get; }
    }


    public sealed class SyncHead
    {
        [JsonProperty]
        public int GlobalCommitSequence { get; private set; }
        [JsonProperty]
        public int EventIndex { get; private set; }
        [JsonProperty]
        public int NumEvents { get; private set; }

        private SyncHead() { }

        public SyncHead(int globalCommitSequence, int numEvents, int eventIndex = 0)
        {
            this.GlobalCommitSequence = globalCommitSequence;
            this.EventIndex = eventIndex;
            this.NumEvents = numEvents;
        }

        public SyncHead(SyncHead other, int eventIndex)
        {
            this.GlobalCommitSequence = other.GlobalCommitSequence;
            this.EventIndex = eventIndex;
            this.NumEvents = other.NumEvents;
        }

    }

    public interface ISyncPushRequest : ISyncRequest
    {

        Guid ClientDatabaseId { get; }
        SyncHead SyncHead { get; }
        int NumLeftInCommit { get; }
        //Guid PushId { get; }

        ICollection<IStreamSegment> Streams { get; }
        //IEnumerable<IEventDTO> Events { get; }

        Task<ISyncPushResponse> GetResponse();

    }


    public interface ISyncPushResponse : ISyncResponse
    {

        Guid ClientDatabaseId { get; }
        Guid PushId { get; }
        bool AlreadyExecuted { get; }

        /**
         * Last command that was executed
         * 
         * value is zero (0) if no commands were executed
         */
        Guid LastExecuted { get; }

        //public Map<String, Long> guids = new HashMap<String, Long>();
    }




    public interface ISyncCommunication
    {

    }

    public interface ISyncRequest : ISyncCommunication
    {
        //ICollection<ISyncEventStream> Streams { get; }
        //void SetTransporter(ITransportEvents transport);
        bool IsEmpty { get; }


    }

    public interface ISyncResponse : ISyncCommunication
    {
        /**
        * Status code
        */
        GSStatusCode StatusCode { get; }

        string StatusDescription { get; }


    }

    public enum GSStatusCode
    {
        FAIL = 0,
        SERVER_UNREACHABLE = 2,

        OK = 200,
        VERSION_TOO_LOW = 452,
        AUTHENTICATION_REQUIRED = 401,
        BAD_REQUEST = 400,
        INTERNAL_SERVER_ERROR = 500,
        FORBIDDEN = 403, // returned for invalid passwords in /auth 

    }


    //public interface IHttpRequestFactory
    //{
    //    HttpRequestMessage CreatePushRequest(ISyncPushRequest req);

    //    HttpRequestMessage CreatePullRequest(ISyncPullRequest req);

    //    HttpRequestMessage CreateAuthRequest(string username, string password);

    //}

   
    public enum RegisterStatus
    {
        OK,
        USERNAME_EXISTS,
        EMAIL_EXISTS
    }


    public class APIRegisterResponse
    {
        [JsonProperty(PropertyName = "registerStatus", Required = Required.Always)]
        public RegisterStatus RegisterStatus {get; set;}

        [JsonIgnore]
        public HttpStatusCode HttpStatus;
    }


    public class RemoteUser
    {
        [JsonProperty(PropertyName = Language.GARDEN, Required = Required.Default)]
        public RemoteGarden Garden { get; set; }

        [JsonProperty(PropertyName = Language.PROPERTY_ENTITY_ID, Required = Required.Always)]
        public Guid AggregateId { get; set; }

        [JsonProperty(PropertyName = Language.USERNAME, Required = Required.Always)]
        public string Username { get; set; }

        [JsonIgnore]
        public string Password { get; set; }

        [JsonIgnore]
        public string Email { get; set; }

        [JsonIgnore]
        public Guid GardenId { get; set; }

        [JsonIgnore]
        public string AccessToken { get; set; }

        [JsonIgnore]
        public int ExpiresIn { get; set; }

        [JsonIgnore]
        public string RefreshToken { get; set; }

        [JsonIgnore]
        public Guid Id { get; set; }

        [JsonIgnore]
        public int Version { get; set; }


        public int PlantCount
        {
            get
            {
                int i = 0;
                if (Garden != null && Garden.Plants != null)
                {
                    foreach (RemotePlant p in Garden.Plants)
                    {
                        if (p.Public)
                        {
                            i++;
                        }

                    }
                }
                return i;
            }
        }


        [JsonIgnore]
        public string FriendlyPlantCount
        {
            get 
            {
                return PlantCount + " plants";
            }
        }

    }


    public class RemoteGarden
    {
        [JsonProperty(PropertyName = Language.PROPERTY_ENTITY_ID, Required = Required.Always)]
        public Guid EntityId { get; set; }

        [JsonProperty(PropertyName = Language.PLANTS, Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public List<RemotePlant> Plants { get; set; }

    }

    public class RemotePlant
    {
        [JsonProperty(PropertyName = Language.PROPERTY_ENTITY_ID, Required = Required.Always)]
        public Guid AggregateId { get; set; }

        [JsonProperty(PropertyName = Language.PLANT_NAME, Required = Required.Always)]
        public string Name { get; set; }

        [JsonProperty(PropertyName = Language.SHARED, Required = Required.Default)]
        public bool Public { get; set; }

        [JsonProperty(PropertyName = Language.TAGS, Required = Required.Default)]
        public HashSet<string> Tags { get; set; }

    }


}
