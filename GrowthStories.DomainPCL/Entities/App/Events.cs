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

    public class GSAppCreated : GSAppEvent
    {


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

        protected AppUserAssigned() { }
        public AppUserAssigned(AssignAppUser cmd)
            : base(cmd)
        {
            this.UserId = cmd.UserId;
        }

        public override string ToString()
        {
            return string.Format(@"Assigned user {0} to app.", UserId);
        }

    }

    public enum StreamType
    {
        USER,
        PLANT
    }

    public class SyncStreamCreated : GSAppEvent
    {
        [JsonProperty]
        public Guid StreamId { get; protected set; }
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
        }

        public override string ToString()
        {
            return string.Format(@"Added syncstream {0} of type {1}", StreamId, StreamType);
        }


    }




    #endregion


}

