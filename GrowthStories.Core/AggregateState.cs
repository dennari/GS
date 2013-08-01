using CommonDomain;
using Microsoft.CSharp.RuntimeBinder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Growthstories.Core
{


    public abstract class AggregateState : IMemento, IAppliesEvents
    {
        public Guid Id { get; protected set; }

        public Type AggregateType { get; set; }

        public int Version { get; protected set; }

        public bool? Public { get; protected set; }

        protected AggregateState()
        {
            Version = 0;
            //Public = false;
        }



        //protected AggregateState(Guid id, int version, bool Public)
        //{
        //    Id = id;
        //    Version = Version;
        //    this.Public = Public;
        //}


        public abstract void Apply(IEvent @event);

    }

    public abstract class AggregateState<TCreateEvent> : AggregateState where TCreateEvent : IEvent
    {

        private bool applying = false;

        protected AggregateState()
            : base()
        {

        }

        public virtual void Apply(TCreateEvent @event)
        {
            if (Version != 0)
            {
                throw DomainError.Named("rebirth", "Can't create aggregate that already exists");
            }
            Id = @event.EntityId;
        }

        public override void Apply(IEvent @event)
        {
            if (Version == 0 && !(@event is TCreateEvent))
            {
                throw DomainError.Named("premature", "Aggregate hasn't been created yet");
            }
            if (this.applying)
                throw new InvalidOperationException(string.Format("Can't find handler for event {0}", @event.GetType().ToString()));
            try
            {
                this.applying = true;
                ((dynamic)this).Apply((dynamic)@event);
                this.applying = false;
                Version++;
            }
            catch (RuntimeBinderException)
            {
                throw;
            }

        }

    }
}
