using CommonDomain;
using Microsoft.CSharp.RuntimeBinder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Growthstories.Core
{
    public abstract class AggregateState<TCreateEvent> : IMemento, IAppliesEvents where TCreateEvent : IEvent
    {
        public Guid Id { get; private set; }

        public int Version { get; private set; }

        public bool? Public { get; protected set; }


        protected AggregateState()
        {
            Version = 0;
            //Public = false;
        }

        protected AggregateState(Guid id, int version, bool Public)
        {
            Id = id;
            Version = Version;
            this.Public = Public;
        }

        public void Apply(TCreateEvent @event)
        {
            if (Version != 0)
            {
                throw DomainError.Named("rebirth", "Can't create aggregate that already exists");
            }
            Id = @event.EntityId;
        }



        public void Apply(IEvent @event)
        {
            if (Version == 0 && !(@event is TCreateEvent))
            {
                throw DomainError.Named("premature", "Aggregate hasn't been created yet");
            }
            try
            {
                ((dynamic)this).Apply((dynamic)@event);
                Version++;
            }
            catch (RuntimeBinderException)
            {
                throw;
            }

        }

    }
}
