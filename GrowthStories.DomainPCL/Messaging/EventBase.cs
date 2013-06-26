using Growthstories.Domain.Entities;
using Growthstories.Core;
using System;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;
using Growthstories.Sync;


namespace Growthstories.Domain.Messaging
{


    public interface IDomainEvent : IEvent//, IEquatable<IEvent>
    {
        void FillDTO(IEventDTO Dto);

        void FromDTO(IEventDTO Dto);

    }


    public abstract class EventBase : IDomainEvent
    {

        public Guid EntityId { get; set; }
        public int EntityVersion { get; set; }
        public Guid EventId { get; set; }
        protected EventBase() { }
        public DateTimeOffset Created { get; set; }


        public EventBase(Guid entityId)
        {
            if (entityId == default(Guid))
            {
                throw new ArgumentException("argument has to be specified", "entityId");
            }
            EntityId = entityId;
        }

        public virtual void FromDTO(IEventDTO Dto)
        {
            this.EntityId = Dto.EntityId;
            this.EntityVersion = Dto.EntityVersion;
            this.EventId = Dto.EventId;
            this.Created = Dto.Created;

        }

        public virtual void FillDTO(IEventDTO Dto)
        {
            Dto.EntityId = this.EntityId;
            Dto.EntityVersion = this.EntityVersion;
            Dto.EventId = this.EventId;
            Dto.Created = this.Created;
        }


    }


}

