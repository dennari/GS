﻿using Growthstories.Domain.Entities;
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

    public interface ICreateCommand : IEntityCommand
    {

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
            if (EntityId == default(Guid))
                throw new ArgumentNullException();
            this.EntityId = EntityId;
        }
    }


}

