using Growthstories.Domain.Entities;
using Growthstories.Core;
using System;
using System.Linq;
using Newtonsoft.Json;
using System.ComponentModel;
using Growthstories.Sync;


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

    public class GSAppCreated : GSAppEvent, ICreateMessage
    {

        [JsonIgnore]
        private Type _AggregateType;
        [JsonIgnore]
        public Type AggregateType
        {
            get { return _AggregateType ?? (_AggregateType = typeof(GSApp)); }
        }
        protected GSAppCreated() { }


        public GSAppCreated(CreateGSApp cmd)
            : base(cmd)
        {

        }

        public override string ToString()
        {
            return string.Format(@"Created GSApp {0}", EntityId);
        }





    }

    public class AppUserAssigned : GSAppEvent
    {
        [JsonProperty]
        public Guid UserId { get; protected set; }

        [JsonProperty]
        public Guid UserGardenId { get; protected set; }

        [JsonProperty]
        public int UserVersion { get; protected set; }

        [JsonProperty]
        public string Username { get; private set; }

        [JsonProperty]
        public string Password { get; private set; }

        [JsonProperty]
        public string Email { get; private set; }

        protected AppUserAssigned() { }
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


    public class SyncStreamCreated : GSAppEvent
    {
        [JsonProperty]
        public Guid StreamId { get; protected set; }
        [JsonProperty]
        public Guid? AncestorId { get; protected set; }
        [JsonProperty]
        public StreamType StreamType { get; protected set; }


        protected SyncStreamCreated() { }
        public SyncStreamCreated(CreateUser createEvent)
        {
            this.StreamId = createEvent.AggregateId;
            this.StreamType = StreamType.USER;
        }

        public SyncStreamCreated(CreatePlant createEvent)
        {
            this.StreamId = createEvent.AggregateId;
            this.StreamType = StreamType.PLANT;
            this.AncestorId = createEvent.AncestorId;
        }

        public SyncStreamCreated(BecomeFollower createEvent)
        {
            this.StreamId = createEvent.Target;
            this.StreamType = StreamType.USER;
            //this.AncestorId = createEvent.AncestorId;
        }

        public SyncStreamCreated(CreateSyncStream createEvent)
        {
            this.StreamId = createEvent.StreamId;
            this.StreamType = createEvent.StreamType;
            this.AncestorId = createEvent.AncestorId;
        }

        public override string ToString()
        {
            return string.Format(@"Added syncstream {0} of type {1}", StreamId, StreamType);
        }


    }

    public class SyncStampSet : GSAppEvent
    {
        [JsonProperty]
        public Guid StreamId { get; protected set; }
        [JsonProperty]
        public long SyncStamp { get; protected set; }


        protected SyncStampSet() { }
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

    public class Synchronized : GSAppEvent
    {
        [JsonProperty]
        public Guid StreamId { get; protected set; }
        [JsonProperty]
        public long SyncStamp { get; protected set; }


        protected Synchronized() { }
        public Synchronized(Synchronize cmd)
        {


        }



        public override string ToString()
        {
            return string.Format(@"Syncstamp set to {0} for stream {1}", SyncStamp, StreamId);
        }


    }


    public class Pulled : GSAppEvent
    {

        //public Guid[] Streams { get; private set; }
        public long SyncStamp { get; private set; }
        public int SyncSequence { get; private set; }

        protected Pulled() { }
        public Pulled(Pull cmd)
        {

            SyncSequence = cmd.GlobalCommitSequence;
            SyncStamp = cmd.Sync.PullResp.SyncStamp;
            //Streams = cmd.Sync.PullResp.Streams.Where(x => GSApp.CanHandle(x)).Select(x => x.AggregateId).ToArray();
        }

        public override string ToString()
        {
            return string.Format(@"Pulled.");
        }


    }

    public class Pushed : GSAppEvent
    {

        public int SyncSequence { get; private set; }

        protected Pushed() { }
        public Pushed(Push cmd)
        {

            SyncSequence = cmd.GlobalCommitSequence;
        }

        public override string ToString()
        {
            return string.Format(@"Pushed.");
        }


    }

    public class AuthTokenSet : EventBase
    {

        [JsonProperty]
        public string AccessToken { get; protected set; }
        [JsonProperty]
        public int ExpiresIn { get; protected set; }
        [JsonProperty]
        public string RefreshToken { get; protected set; }

        protected AuthTokenSet() { }

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

