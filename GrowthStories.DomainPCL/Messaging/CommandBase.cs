using Growthstories.Domain.Entities;
//using CommonDomain;
using System;
using Growthstories.Core;


namespace Growthstories.Domain.Messaging
{


    public abstract class CommandBase : ICommand
    {

    }

    public interface IEntityCommand : ICommand
    {
        Guid EntityId { get; }
        Type EntityType { get; }
    }

    public abstract class EntityCommand<T> : CommandBase, IEntityCommand
    {
        public Guid EntityId { get; private set; }
        private readonly Type _EntityType = typeof(T);
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
            this.EntityId = EntityId;
        }
    }


}

