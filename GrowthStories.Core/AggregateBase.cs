using System;
using System.Collections.Generic;
using CommonDomain;
using CommonDomain.Core;
using Growthstories.Core;
using System.Reflection;

namespace Growthstories.Core
{

    public abstract class AggregateBase<TState, TCreate> : AggregateBase, IApplyState
        where TState : IAppliesEvents, IMemento, new()
        where TCreate : IEvent
    {

        private TState _state;

        protected TState State
        {
            get
            {
                return _state;
            }
            set
            {
                _state = value;
            }
        }

        public override Guid Id
        {
            get { return State == null ? Guid.Empty : State.Id; }
            protected set
            {
            }
        }
        public override int Version
        {
            get { return State == null ? 0 : State.Version; }
            protected set
            {
            }
        }

        protected AggregateBase()
        {
        }

        public void ApplyState(IMemento st)
        {
            if (this.State != null)
            {
                throw new InvalidOperationException("Can't override existing state");
            }
            // st is null when an unexisting aggregate is initialized through the repository
            TState state = st == null ? InitializeState() : (TState)st;
            SetupRoutes(state);
            this.State = state;
        }

        protected virtual TState InitializeState()
        {
            return new TState();
        }

        private void SetupRoutes(IAppliesEvents state)
        {
            if (state == null)
            {
                throw new ArgumentNullException("State cannot be null for the router");
            }
            RegisteredRoutes = new ApplyEventRouter(state);

        }

        protected new void RaiseEvent(object @event)
        {
            //((IAggregate)this).ApplyEvent(@event);
            //this.uncommittedEvents.Add(@event);
            base.RaiseEvent(@event); // calls ApplyEvent and increases Version
            IEvent Event = @event as IEvent;
            if (Event != null)
            {
                Event.EntityVersion = this.Version;
                //@event = Event;
            }

        }

        public void Create(Guid Id)
        {
            if (State == null)
            {
                ApplyState(null);
            }
            RaiseEvent(Activator.CreateInstance(typeof(TCreate), Id));

        }


        protected override IMemento GetSnapshot()
        {
            return State;
        }

    }
}
