
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
        Task<IPhotoUploadResponse> GetResponse();
        Stream Stream { get; }
    }

    public interface IPhotoUploadResponse : ISyncResponse
    {

        Photo Photo { get; }
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
        string AccessToken { get; }
        int ExpiresIn { get; }
        string RefreshToken { get; }
    }

    public interface IAuthUser : IAuthToken, IMemento
    {
        string Username { get; }
        string Password { get; }
        string Email { get; }
        Guid GardenId { get; }
        bool IsCollaborator { get; }
    }


    public sealed class PullStream
    {
        public readonly PullStreamType Type;
        public readonly Guid StreamId;
        public readonly Guid? AncestorId;

        public long SyncStamp;

        public PullStream(Guid streamId, PullStreamType type, Guid? ancestorId = null)
        {
            this.Type = type;
            this.StreamId = streamId;
            this.AncestorId = ancestorId;
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
        ICollection<IStreamSegment> Streams { get; }
        long SyncStamp { get; }


    }

    public interface ISyncPushRequest : ISyncRequest
    {

        Guid ClientDatabaseId { get; }
        int GlobalCommitSequence { get; }
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
        OK = 200,
        VERSION_TOO_LOW = 452,
        AUTHENTICATION_REQUIRED = 401,
        BAD_REQUEST = 400,
        INTERNAL_SERVER_ERROR = 500
    }


    //public interface IHttpRequestFactory
    //{
    //    HttpRequestMessage CreatePushRequest(ISyncPushRequest req);

    //    HttpRequestMessage CreatePullRequest(ISyncPullRequest req);

    //    HttpRequestMessage CreateAuthRequest(string username, string password);

    //}



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
