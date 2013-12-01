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

        [JsonProperty(PropertyName = Language.STREAMS, Required = Required.Always)]
        public ICollection<PullStream> Streams { get; set; }


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
        [JsonProperty(PropertyName = Language.STREAM, Required = Required.Always)]
        public PullStream Stream { get; set; }

        [JsonProperty(PropertyName = Language.EVENTS, Required = Required.Always)]
        public IList<EventDTOUnion> DTOs { get; set; }

        [JsonProperty(PropertyName = Language.ERROR_CODE, Required = Required.Default)]
        public string ErrorCode { get; set; }

        [JsonProperty(PropertyName = Language.NEXT_SINCE, Required = Required.Default)]
        public long NextSince { get; set; }

    }

    public class HelperPullResponse
    {
        [JsonProperty(PropertyName = Language.STREAMS, Required = Required.Always)]
        public IList<PullStreamDTO> Streams { get; set; }

        [JsonProperty(PropertyName = Language.LIMIT, Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int Limit { get; set; }
    }



    public class HttpPullResponse : HttpResponse, ISyncPullResponse
    {
        //public long SyncStamp { get; set; }


        public ICollection<PullStream> Projections { get; set; }

        ICollection<IStreamSegment> _Streams;
        public ICollection<IStreamSegment> Streams
        {
            get
            {
                if (_Streams == null && Projections != null)
                {
                    _Streams = Projections.SelectMany(x => x.Segments.Values).ToArray();
                }
                return _Streams;
            }
            set
            {
                _Streams = value;
            }
        }


    }


    public class PhotoUriResponse : HttpResponse, IPhotoUriResponse
    {

        public Uri PhotoUri { get; set; }

    }

    public class PhotoUploadResponse : HttpResponse, IPhotoUploadResponse
    {

        public Photo Photo { get; set; }
        public string BlobKey { get; set; }
        public Guid PlantActionId { get; set; }


    }

    public class PhotoUploadRequest : IPhotoUploadRequest
    {
        private readonly ITransportEvents Transporter;
        private readonly IJsonFactory jFactory;
        private readonly IPhotoHandler FileOpener;


        public PhotoUploadRequest(Photo photo, Guid plantActionId, IJsonFactory jFactory, ITransportEvents transporter, IPhotoHandler fileOpener)
        {
            // TODO: Complete member initialization

            this.jFactory = jFactory;
            this.Photo = photo;
            this.PlantActionId = plantActionId;
            this.FileOpener = fileOpener;
            this.Transporter = transporter;
        }

        public Photo Photo { get; private set; }
        public Guid PlantActionId { get; private set; }

        public bool IsEmpty
        {
            get { return false; }
        }


        public async Task<IPhotoUploadResponse> GetResponse()
        {

            var uploadUriResponse = await Transporter.RequestPhotoUploadUri();
            if (uploadUriResponse.StatusCode != GSStatusCode.OK)
                throw new InvalidOperationException("Can't upload photo since upload uri can't be retrieved");

            this.UploadUri = uploadUriResponse.PhotoUri;
            this.Stream = await FileOpener.ReadPhoto(Photo);

            return await Transporter.RequestPhotoUpload(this);


        }

        public Uri UploadUri { get; private set; }


        public Stream Stream { get; private set; }

    }

    public class PhotoDownloadRequest : IPhotoDownloadRequest
    {
        private readonly ITransportEvents Transporter;
        private readonly IJsonFactory jFactory;
        private readonly IPhotoHandler PhotoHandler;


        public PhotoDownloadRequest(Photo photo, IJsonFactory jFactory, ITransportEvents transporter, IPhotoHandler fileOpener)
        {
            // TODO: Complete member initialization

            this.jFactory = jFactory;
            this.Photo = photo;
            this.PhotoHandler = fileOpener;
            this.Transporter = transporter;
        }

        public Photo Photo { get; private set; }

        public bool IsEmpty
        {
            get { return false; }
        }


        public async Task<IPhotoDownloadResponse> GetResponse()
        {

            if (Photo.RemoteUri == null && Photo.BlobKey == null)
                throw new InvalidOperationException("Can't download image which doesn't have RemoteUri or BlobKey set.");

            if (Photo.RemoteUri != null)
                this.DownloadUri = new Uri(Photo.RemoteUri);
            else
            {
                var r = await Transporter.RequestPhotoDownloadUri(Photo.BlobKey);
                if (r.StatusCode != GSStatusCode.OK)
                    throw new InvalidOperationException("Unable to retrieve download uri for image");
                this.DownloadUri = r.PhotoUri;

            }

            var r2 = await Transporter.RequestPhotoDownload(this);
            if (r2.StatusCode != GSStatusCode.OK)
                throw new InvalidOperationException("Unable to download image " + this.DownloadUri);



            using (var writeStream = await PhotoHandler.WritePhoto(Photo))
            using (var readStream = r2.Stream)
            {
                await readStream.CopyToAsync(writeStream);
            }


            return r2;


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
        public SyncHead SyncHead { get; set; }
        [JsonIgnore]
        public int NumLeftInCommit { get; set; }


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
