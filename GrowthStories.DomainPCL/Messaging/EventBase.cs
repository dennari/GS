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

        bool HasAncestor { get; }
        bool HasParent { get; }

        void FillDTO(IEventDTO Dto);

        void FromDTO(IEventDTO Dto);

    }


    public abstract class EventBase : IDomainEvent
    {
        [JsonProperty]
        public Guid EntityId { get; private set; }
        [JsonProperty]
        public int EntityVersion { get; set; }
        [JsonProperty]
        public Guid EventId { get; set; }
        [JsonProperty]
        public DateTimeOffset Created { get; set; }

        [JsonIgnore]
        public bool HasAncestor { get; protected set; }

        [JsonIgnore]
        public bool HasParent { get; protected set; }


        protected EventBase()
        {
            HasAncestor = true;
            HasParent = true;
        }

        public EventBase(Guid entityId)
            : this()
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
            this.Created = Dto.Created.DateTimeFromUnixTimestampMillis();

        }

        public virtual void FillDTO(IEventDTO Dto)
        {
            Dto.EntityId = this.EntityId;
            Dto.EntityVersion = this.EntityVersion;
            Dto.EventId = this.EventId;
            Dto.Created = this.Created.GetUnixTimestampMillis();
        }




    }

    public static class DateTimeMixins
    {
        private static readonly DateTimeOffset UnixEpoch =
    new DateTimeOffset(1970, 1, 1, 0, 0, 0, new TimeSpan(0));

        public static long GetCurrentUnixTimestampMillis()
        {
            return (long)(DateTimeOffset.UtcNow - UnixEpoch).TotalMilliseconds;
        }

        public static long GetUnixTimestampMillis(this DateTimeOffset d)
        {
            return (long)(d.ToUniversalTime() - UnixEpoch).TotalMilliseconds;
        }

        public static DateTimeOffset DateTimeFromUnixTimestampMillis(this long millis)
        {
            return UnixEpoch.AddMilliseconds(millis);
        }

        public static long GetCurrentUnixTimestampSeconds()
        {
            return (long)(DateTimeOffset.UtcNow - UnixEpoch).TotalSeconds;
        }

        public static DateTimeOffset DateTimeFromUnixTimestampSeconds(this long seconds)
        {
            return UnixEpoch.AddSeconds(seconds);
        }
    }


}

