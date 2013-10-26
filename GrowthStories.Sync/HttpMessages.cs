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
using System.IO;

namespace Growthstories.Sync
{
    public class HttpPullRequest : ISyncPullRequest
    {
        private readonly IJsonFactory jF;

        [JsonIgnore]
        public ITranslateEvents Translator { get; set; }

        [JsonIgnore]
        public ITransportEvents Transporter { get; set; }

        protected ICollection<PullStream> _Streams;
        [JsonIgnore]
        public ICollection<PullStream> Streams
        {
            get { return _Streams; }
            set
            {
                if (value != null)
                {
                    _Streams = value;
                    OutputStreams = value.Select(x => PullStreamDTO.Translate(x)).ToArray();
                }

            }
        }

        [JsonProperty(PropertyName = Language.STREAMS, Required = Required.Always)]
        public PullStreamDTO[] OutputStreams { get; protected set; }

        public HttpPullRequest(IJsonFactory jF)
        {
            this.jF = jF;
        }

        public override string ToString()
        {
            return jF.Serialize(this);
        }



        public Task<ISyncPullResponse> GetResponse()
        {
            return Transporter.PullAsync(this);
        }

        [JsonIgnore]
        public bool IsEmpty
        {
            get { return Streams == null || Streams.Count == 0; }
        }
    }


    public sealed class PullStreamDTO
    {

        private PullStreamDTO()
        {

        }

        [JsonProperty(PropertyName = Language.STREAM_TYPE, Required = Required.Always)]
        public string Type { get; private set; }

        [JsonProperty(PropertyName = Language.STREAM_SINCE, Required = Required.Always)]
        public long SyncStamp { get; private set; }

        [JsonProperty(PropertyName = Language.STREAM_ENTITY, Required = Required.Always)]
        public Guid StreamId { get; private set; }

        [JsonProperty(PropertyName = Language.STREAM_ANCESTOR, Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Include)]
        public Guid? StreamAncestorId { get; private set; }



        public static PullStreamDTO Translate(PullStream stream)
        {
            var r = new PullStreamDTO();
            r.SyncStamp = stream.SyncStamp;
            r.StreamId = stream.StreamId;
            r.Type = stream.Type == PullStreamType.PLANT ? "PLANT" : "USER";
            r.StreamAncestorId = stream.AncestorId;

            return r;
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

        public ICollection<IStreamSegment> Streams { get; set; }


    }


    public class PhotoUploadUriResponse : HttpResponse, IPhotoUploadUriResponse
    {

        [JsonProperty(PropertyName = "UploadUri", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public Uri UploadUri { get; set; }

    }

    public class PhotoUploadResponse : HttpResponse, IPhotoUploadResponse
    {

        public Photo Photo { get; set; }

    }

    public class PhotoUploadRequest : IPhotoUploadRequest
    {
        private readonly ITransportEvents Transporter;
        private readonly IJsonFactory jFactory;
        private readonly IFileOpener FileOpener;


        public PhotoUploadRequest(Photo photo, IJsonFactory jFactory, ITransportEvents transporter, IFileOpener fileOpener)
        {
            // TODO: Complete member initialization

            this.jFactory = jFactory;
            this.Photo = photo;
            this.FileOpener = fileOpener;
            this.Transporter = transporter;
        }

        public Photo Photo { get; private set; }

        public bool IsEmpty
        {
            get { return false; }
        }


        public async Task<IPhotoUploadResponse> GetResponse()
        {

            var uploadUriResponse = await Transporter.RequestPhotoUploadUri();
            if (uploadUriResponse.StatusCode != GSStatusCode.OK)
                throw new InvalidOperationException("Can't upload photo since upload uri can't be retrieved");

            this.UploadUri = uploadUriResponse.UploadUri;
            this.Stream = await FileOpener.OpenPhoto(Photo);

            return await Transporter.RequestPhotoUpload(this);


        }

        public Uri UploadUri { get; private set; }


        public Stream Stream { get; private set; }

    }

    public class PhotoDownloadRequest : IPhotoDownloadRequest
    {
        private readonly ITransportEvents Transporter;
        private readonly IJsonFactory jFactory;
        private readonly IFileOpener FileOpener;


        public PhotoDownloadRequest(Photo photo, IJsonFactory jFactory, ITransportEvents transporter, IFileOpener fileOpener)
        {
            // TODO: Complete member initialization

            this.jFactory = jFactory;
            this.Photo = photo;
            this.FileOpener = fileOpener;
            this.Transporter = transporter;
        }

        public Photo Photo { get; private set; }

        public bool IsEmpty
        {
            get { return false; }
        }


        public async Task<IPhotoDownloadResponse> GetResponse()
        {


            this.DownloadUri = new Uri(Photo.RemoteUri);
            return await Transporter.RequestPhotoDownload(this);


        }

        public Uri DownloadUri { get; private set; }




    }


    public class PhotoDownloadResponse : HttpResponse, IPhotoDownloadResponse
    {

        public Photo Photo { get; set; }
        public Stream Stream { get; set; }

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
        public IEnumerable<IEventDTO> Events
        {
            get
            {
                return Translator.Out(Streams);
            }
        }

        [JsonProperty(PropertyName = Language.CLIENT_ID)]
        public Guid ClientDatabaseId { get; set; }

        [JsonIgnore]
        public ICollection<IStreamSegment> Streams { get; set; }

        [JsonIgnore]
        public int GlobalCommitSequence { get; set; }


        [JsonIgnore]
        public ITranslateEvents Translator { get; set; }

        [JsonIgnore]
        public ITransportEvents Transporter { get; set; }

        public void Retranslate()
        {
            //if (translator != null && Streams.Count > 0)
            //    this.Events = translator.Out(Streams).ToArray();
        }

        public HttpPushRequest(IJsonFactory jF)
        {
            this.jF = jF;
        }

        public override string ToString()
        {
            return jF.Serialize(this);
        }



        [JsonIgnore]
        public bool IsEmpty
        {
            get { return Streams == null || Streams.Count == 0; }
        }



        public Task<ISyncPushResponse> GetResponse()
        {
            return Transporter.PushAsync(this);
        }



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



}
