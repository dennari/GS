using Growthstories.Domain.Entities;
//using CommonDomain;
using System;
using Growthstories.Core;
using Growthstories.Sync;


namespace Growthstories.Domain.Messaging
{




    public abstract class AggregateCommand<T> : IAggregateCommand
    {
        public Guid AggregateId { get; protected set; }
        public Guid? EntityId { get; protected set; }

        public DateTimeOffset Created { get; set; }
        public Guid MessageId { get; set; }

        private readonly Type _EntityType = typeof(T);


        public Guid? StreamEntityId { get; set; }
        public Guid? StreamAncestorId { get; set; }
        public DTOType StreamType { get; set; }
        public Guid? AncestorId { get; set; }
        public Guid? ParentAncestorId { get; set; }
        public Guid? ParentId { get; set; }


        public Type AggregateType
        {
            get
            {
                return _EntityType;
            }
        }
        protected AggregateCommand() { }
        public AggregateCommand(Guid AggregateId)
        {
            if (AggregateId == default(Guid))
                throw new ArgumentNullException();
            this.AggregateId = AggregateId;
            if (AggregateType == typeof(User))
            {
                this.StreamType = DTOType.user;
            }
            if (AggregateType == typeof(Plant))
            {
                this.StreamType = DTOType.plant;
            }
        }

        public AggregateCommand(Guid AggregateId, Guid entityId)
            : this(AggregateId)
        {
            if (entityId == default(Guid))
                throw new ArgumentNullException();
            this.EntityId = entityId;
        }

    }


}

