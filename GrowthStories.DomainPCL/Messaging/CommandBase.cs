using Growthstories.Domain.Entities;
//using CommonDomain;
using System;
using Growthstories.Core;
using Growthstories.Sync;


namespace Growthstories.Domain.Messaging
{


    public abstract class CommandBase : ICommand
    {

    }

    public interface IEntityCommand : ICommand
    {
        Guid EntityId { get; }
        Type EntityType { get; }

        DTOType StreamType { get; }
        Guid? AncestorId { get; }
        Guid? ParentAncestorId { get; }
        Guid? ParentId { get; }

        Guid? StreamEntityId { get; }
        Guid? StreamAncestorId { get; }
    }

    public interface ICreateCommand : IEntityCommand
    {

    }



    public abstract class EntityCommand<T> : CommandBase, IEntityCommand
    {
        public Guid EntityId { get; private set; }
        private readonly Type _EntityType = typeof(T);


        public Guid? StreamEntityId { get; set; }
        public Guid? StreamAncestorId { get; set; }
        public DTOType StreamType { get; set; }
        public Guid? AncestorId { get; set; }
        public Guid? ParentAncestorId { get; set; }
        public Guid? ParentId { get; set; }


        public Type EntityType
        {
            get
            {
                return _EntityType;
            }
        }
        protected EntityCommand() { }
        public EntityCommand(Guid EntityId)
        {
            if (EntityId == default(Guid))
                throw new ArgumentNullException();
            this.EntityId = EntityId;
            if (EntityType == typeof(User))
            {
                this.StreamType = DTOType.user;
            }
            if (EntityType == typeof(Plant))
            {
                this.StreamType = DTOType.plant;
            }
        }
    }


}

