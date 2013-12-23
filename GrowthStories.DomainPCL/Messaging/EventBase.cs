using Growthstories.Domain.Entities;
using Growthstories.Core;
using System;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;
using Growthstories.Sync;


namespace Growthstories.Domain.Messaging
{


    public interface IDomainEvent : IEvent
    {


        void FillDTO(IEventDTO Dto);

        void FromDTO(IEventDTO Dto);

    }


    public abstract class EventBase : IDomainEvent
    {
        [JsonProperty]
        public Guid AggregateId { get; protected set; }
        [JsonProperty]
        public int AggregateVersion { get; set; }
        [JsonProperty]
        public Guid? EntityId { get; set; }


        [JsonProperty]
        public Guid MessageId { get; set; }
        [JsonProperty]
        public DateTimeOffset Created { get; set; }

        [JsonProperty]
        public Guid? StreamEntityId { get; protected set; }
        [JsonProperty]
        public Guid? StreamAncestorId { get; protected set; }
        [JsonProperty]
        public DTOType StreamType { get; protected set; }

        [JsonProperty]
        public Guid? AncestorId { get; protected set; }
        [JsonProperty]
        public Guid? ParentAncestorId { get; protected set; }
        [JsonProperty]
        public Guid? ParentId { get; protected set; }

        protected EventBase()
        {

        }

        public EventBase(Guid aggregateId)
            : this()
        {
            if (aggregateId == default(Guid))
            {
                throw new ArgumentException("argument has to be specified", "aggregateId");
            }
            AggregateId = aggregateId;
        }

        public EventBase(Guid aggregateId, Guid entityId)
            : this(aggregateId)
        {
            if (entityId == default(Guid))
            {
                throw new ArgumentException("argument has to be specified", "entityId");
            }
            EntityId = entityId;
        }

        public EventBase(IMessage msg)
            : this(msg.AggregateId)
        {
            this.EntityId = msg.EntityId;

        }
        public EventBase(IAggregateCommand cmd)
            : this((IMessage)cmd)
        {


            this.StreamEntityId = cmd.StreamEntityId;
            this.StreamAncestorId = cmd.StreamAncestorId;
            //this.StreamVersion = cmd.StreamVersion;
            this.ParentId = cmd.ParentId;
            this.ParentAncestorId = cmd.ParentAncestorId;
            this.AncestorId = cmd.AncestorId;
            //this.StreamType = cmd.StreamType;

        }

        public virtual void FromDTO(IEventDTO Dto)
        {
            this.AggregateId = Dto.AggregateId;
            this.AggregateVersion = Dto.AggregateVersion;
            this.EntityId = Dto.EntityId;
            this.MessageId = Dto.MessageId;
            this.Created = Dto.Created.DateTimeFromUnixTimestampMillis();
            //this.StreamEntityId = Dto.StreamEntity;
            //this.StreamAncestorId = Dto.StreamAncestor;
            this.ParentAncestorId = Dto.ParentAncestorId;
            this.ParentId = Dto.ParentId;
            this.AncestorId = Dto.AncestorId;


        }

        public virtual void FillDTO(IEventDTO Dto)
        {
            Dto.AggregateId = this.AggregateId;
            Dto.EntityId = this.EntityId ?? this.AggregateId;
            Dto.AggregateVersion = this.AggregateVersion;
            Dto.MessageId = this.MessageId;
            Dto.Created = this.Created.GetUnixTimestampMillis();
            //Dto.StreamEntity = this.StreamEntityId;
            Dto.StreamAncestor = this.StreamAncestorId;
            Dto.ParentAncestorId = this.ParentAncestorId;
            Dto.ParentId = this.ParentId;
            Dto.AncestorId = this.AncestorId;
        }




    }

    [DTOObject(DTOType.nullEvent)]
    public sealed class NullEvent : EventBase, INullEvent
    {

        public NullEvent(Guid aggregateId) : base(aggregateId) { }

        public NullEvent(IMessage msg)
            : base(msg.AggregateId)
        {
            this.Created = msg.Created;
            this.MessageId = msg.MessageId;
            var other = msg as EventBase;
            if (other != null)
            {
                this.AggregateVersion = other.AggregateVersion;
                this.AncestorId = other.AncestorId;
                this.StreamAncestorId = other.StreamAncestorId;
                this.StreamEntityId = other.StreamEntityId;
                this.StreamType = other.StreamType;
            }

        }


    }


    [DTOObject(DTOType.delEntity)]
    public class AggregateDeleted : EventBase, IDeleteEvent
    {

        
        public AggregateDeleted() { }

        [JsonProperty]
        public string Kind {get; set; }

        public AggregateDeleted(IDeleteCommand cmd)
            : base(cmd)
        {
            this.Kind = cmd.Kind;
        }


        public override string ToString()
        {
            return string.Format(@"Deleted aggregate {0}.", AggregateId);
        }


        public override void FillDTO(IEventDTO Dto)
        {
            var D = (IDelEntityDTO)Dto;

            base.FillDTO(D);

            switch (this.Kind)
            {
                case "user":
                    D.EntityType = DTOType.user;
                    break;

                case "plant":
                    D.EntityType = DTOType.plant;
                    break;

                case "blooming":
                    D.EntityType = DTOType.blooming;
                    break;

                case "comment":
                    D.EntityType = DTOType.comment;
                    break;

                case "deceased":
                    D.EntityType = DTOType.deceased;
                    break;

                case "fertilizing":
                    D.EntityType = DTOType.fertilizing;
                    break;

                case "harvesting":
                    D.EntityType = DTOType.harvesting;
                    break;

                case "measurement":
                    D.EntityType = DTOType.measurement;
                    break;

                case "misting":
                    D.EntityType = DTOType.misting;
                    break;

                case "photo":
                    D.EntityType = DTOType.photo;
                    break;

                case "pollination":
                    D.EntityType = DTOType.pollination;
                    break;

                case "pruning":
                    D.EntityType = DTOType.pruning;
                    break;

                case "sprouting":
                    D.EntityType = DTOType.sprouting;
                    break;

                case "transfer":
                    D.EntityType = DTOType.transfer;
                    break;

                case "watering":
                    D.EntityType = DTOType.watering;
                    break;

                default:
                    // BUG
                    D.EntityType = DTOType.NOTYPE;
                    break;
            }

            if (this.Kind == null)
            {
                // BUG
                D.EntityType = DTOType.NOTYPE;
            }
        }

        public override void FromDTO(IEventDTO Dto)
        {
            var D = (IDelEntityDTO)Dto;


            base.FromDTO(D);
        }
    }

    public class DeleteAggregate : IDeleteCommand
    {


        public DeleteAggregate(Guid id, string Kind)
        {
            this.AggregateId = id;
            this.Kind = Kind;
        }

        public override string ToString()
        {
            return string.Format(@"Delete aggregate {0}.", AggregateId);
        }

        public string Kind { get; set; }
        public Guid AggregateId { get; private set; }
        public Guid? AncestorId { get; set; }
        public Guid? ParentAncestorId { get; set; }
        public Guid? ParentId { get; set; }
        public Guid? StreamEntityId { get; set; }
        public Guid? StreamAncestorId { get; set; }
        public Guid? EntityId { get; set; }
        public DateTimeOffset Created { get; set; }
        public Guid MessageId { get; set; }
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

