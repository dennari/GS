using Growthstories.Domain.Entities;
//using CommonDomain;
using System;
using Growthstories.Core;


namespace Growthstories.Domain.Messaging
{


    public abstract class CommandBase : ICommand
    {

    }

    public abstract class EntityCommand : CommandBase
    {
        public Guid EntityId { get; private set; }
        protected EntityCommand() { }
        public EntityCommand(Guid EntityId)
        {
            this.EntityId = EntityId;
        }
    }


}

