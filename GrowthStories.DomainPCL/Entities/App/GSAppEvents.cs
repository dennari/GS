using Growthstories.Domain.Entities;
using Growthstories.Core;
using System;
using System.Linq;
using Newtonsoft.Json;
using System.ComponentModel;
using Growthstories.Sync;
using System.Collections.Generic;


namespace Growthstories.Domain.Messaging
{



    #region GSApp

    public class GSAppEvent : EventBase
    {

        public GSAppEvent()
            : base(GSAppState.GSAppId)
        {

        }
        public GSAppEvent(IMessage msg)
            : this()
        {

        }
    }

    public sealed class GSAppCreated : GSAppEvent, ICreateMessage
    {

        [JsonIgnore]
        private Type _AggregateType;
        [JsonIgnore]
        public Type AggregateType
        {
            get { return _AggregateType ?? (_AggregateType = typeof(GSApp)); }
        }
        private GSAppCreated() { }


        public GSAppCreated(CreateGSApp cmd)
            : base(cmd)
        {

        }

        public override string ToString()
        {
            return string.Format(@"Created GSApp {0}", EntityId);
        }





    }

    public sealed class AppUserAssigned : GSAppEvent
    {
        [JsonProperty]
        public Guid UserId { get; private set; }

        [JsonProperty]
        public Guid UserGardenId { get; private set; }

        [JsonProperty]
        public int UserVersion { get; private set; }

        [JsonProperty]
        public string Username { get; private set; }

        [JsonProperty]
        public string Password { get; private set; }

        [JsonProperty]
        public string Email { get; private set; }

        private AppUserAssigned() { }
        public AppUserAssigned(AssignAppUser cmd)
            : base(cmd)
        {
            this.UserId = cmd.UserId;
            this.Username = cmd.Username;
            this.Password = cmd.Password;
            this.Email = cmd.Email;
            this.UserGardenId = cmd.UserGardenId;
            this.UserVersion = cmd.UserVersion;
        }

        public override string ToString()
        {
            return string.Format(@"Assigned user {0} to app.", UserId);
        }

    }


    public sealed class SyncStreamCreated : GSAppEvent
    {
        [JsonProperty]
        public Guid StreamId { get; private set; }

        [JsonProperty]
        public PullStreamType SyncStreamType { get; private set; }


        private SyncStreamCreated() { }
        public SyncStreamCreated(CreateUser createEvent)
        {
            this.StreamId = createEvent.AggregateId;
            this.SyncStreamType = PullStreamType.USER;
        }

        public SyncStreamCreated(CreatePlant createEvent)
        {
            this.StreamId = createEvent.AggregateId;
            this.SyncStreamType = PullStreamType.PLANT;
            this.AncestorId = createEvent.AncestorId;
        }

        public SyncStreamCreated(BecomeFollower createEvent)
        {
            this.StreamId = createEvent.Target;
            this.SyncStreamType = PullStreamType.USER;
            //this.AncestorId = createEvent.AncestorId;
        }

        public SyncStreamCreated(CreateSyncStream createEvent)
        {
            this.StreamId = createEvent.StreamId;
            this.SyncStreamType = createEvent.SyncStreamType;
            this.AncestorId = createEvent.AncestorId;
        }

        public override string ToString()
        {
            return string.Format(@"Added syncstream {0} of type {1}", StreamId, SyncStreamType);
        }


    }

    public sealed class SyncStampSet : GSAppEvent
    {
        [JsonProperty]
        public Guid StreamId { get; private set; }
        [JsonProperty]
        public long SyncStamp { get; private set; }


        private SyncStampSet() { }
        public SyncStampSet(SetSyncStamp cmd)
        {
            this.StreamId = cmd.StreamId;
            this.SyncStamp = cmd.SyncStamp;

        }



        public override string ToString()
        {
            return string.Format(@"Syncstamp set to {0} for stream {1}", SyncStamp, StreamId);
        }


    }

    public sealed class PhotoUploadScheduled : GSAppEvent
    {
        [JsonProperty]
        public Photo Photo { get; private set; }

        private PhotoUploadScheduled() { }
        public PhotoUploadScheduled(SchedulePhotoUpload cmd)
        {
            this.Photo = cmd.Photo;
        }



        public override string ToString()
        {
            return string.Format(@"New photo scheduled for upload");
        }


    }

    public sealed class PhotoUploadCompleted : GSAppEvent
    {
        [JsonProperty]
        public Photo Photo { get; private set; }

        private PhotoUploadCompleted() { }
        public PhotoUploadCompleted(CompletePhotoUpload cmd)
        {
            this.Photo = cmd.Photo;
        }



        public override string ToString()
        {
            return string.Format(@"Photo upload completed.");
        }


    }

    public sealed class PhotoDownloadScheduled : GSAppEvent
    {
        [JsonProperty]
        public Photo Photo { get; private set; }

        private PhotoDownloadScheduled() { }
        public PhotoDownloadScheduled(PlantActionCreated e)
        {
            this.Photo = e.Photo;
        }



        public override string ToString()
        {
            return string.Format(@"New photo scheduled for download");
        }


    }

    public sealed class PhotoDownloadCompleted : GSAppEvent
    {
        [JsonProperty]
        public Photo Photo { get; private set; }

        private PhotoDownloadCompleted() { }
        public PhotoDownloadCompleted(CompletePhotoDownload cmd)
        {
            this.Photo = cmd.Photo;
        }



        public override string ToString()
        {
            return string.Format(@"Photo download completed.");
        }


    }



    public sealed class Pulled : GSAppEvent
    {

        //public Guid[] Streams { get; private set; }
        [JsonProperty]
        public IDictionary<Guid, long> SyncStamps { get; private set; }


        private Pulled() { }
        public Pulled(Pull cmd)
        {

            SyncStamps = cmd.Sync.PullResp.Projections.ToDictionary(x => x.StreamId, x => x.NextSince);
            //Streams = cmd.Sync.PullResp.Streams.Where(x => GSApp.CanHandle(x)).Select(x => x.AggregateId).ToArray();
        }

        public override string ToString()
        {
            return string.Format(@"Pulled.");
        }


    }

    public sealed class Pushed : GSAppEvent
    {
        [JsonProperty]
        public SyncHead SyncHead { get; private set; }



        private Pushed() { }
        public Pushed(Push cmd)
        {

            SyncHead = cmd.SyncHead;
        }

        public override string ToString()
        {
            return string.Format(@"Pushed.");
        }


    }

    public sealed class AuthTokenSet : EventBase
    {

        [JsonProperty]
        public string AccessToken { get; private set; }
        [JsonProperty]
        public int ExpiresIn { get; private set; }
        [JsonProperty]
        public string RefreshToken { get; private set; }

        private AuthTokenSet() { }

        public AuthTokenSet(SetAuthToken cmd)
            : base(cmd)
        {
            this.AccessToken = cmd.AccessToken;
            this.ExpiresIn = cmd.ExpiresIn;
            this.RefreshToken = cmd.RefreshToken;

        }

        public AuthTokenSet(Guid id, string accessToken, string refreshToken, int expiresIn)
            : base(id)
        {
            this.AccessToken = accessToken;
            this.ExpiresIn = expiresIn;
            this.RefreshToken = refreshToken;
        }

        public override string ToString()
        {
            return string.Format(@"AuthTokenSet access: {0}, refresh: {1}, expires {2}, for user {3}.", AccessToken, RefreshToken, ExpiresIn, AggregateId);
        }

    }




    #endregion


}

