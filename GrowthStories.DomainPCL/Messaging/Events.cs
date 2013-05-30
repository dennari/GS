using Growthstories.Domain.Entities;
using Growthstories.Core;
using System;


namespace Growthstories.Domain.Messaging
{


    public interface IDomainEvent : IEvent
    {
        //public long EntityVersion { get; }
    }


    public abstract class EventBase : IDomainEvent
    {
        public Guid EntityId { get; set; }
        public int EntityVersion { get; set; }
        public Guid EventId { get; set; }

        protected EventBase() { }

        public EventBase(Guid entityId)
        {
            //if (entityId == default(Guid))
            //{
            //    throw new ArgumentException("argument has to be specified", "entityId");
            //}
            EntityId = entityId;
        }

        //public EventBase(Guid eventId, Guid entityId, long entityVersion)
        //{
        //    EntityId = entityId;
        //    EventId = eventId;
        //    EntityVersion = entityVersion;
        //}

        //public EventBase(Guid eventId, Guid entityId)
        //    : this(eventId, entityId, 0)
        //{

        //}

    }


}

