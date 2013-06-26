using System;
using System.Collections.Generic;
using CommonDomain;
using CommonDomain.Core;
using Growthstories.Core;
using System.Reflection;

namespace Growthstories.Core
{

    public interface IGSAggregate : IAggregate, IApplyState
    {
        void SetEventFactory(IEventFactory factory);
    }

    public abstract class AggregateBase<TState, TCreate> : AggregateBase, IGSAggregate
        where TState : IAppliesEvents, IMemento, new()
        where TCreate : IEvent
    {

        private TState _state;
        private IEventFactory _eventFactory;

        public AggregateBase()
        {
            this.ApplyState(this.InitializeState());
        }

        public void SetEventFactory(IEventFactory factory)
        {
            this._eventFactory = factory;
        }

        protected TState State
        {
            get
            {
                return _state;
            }
            private set
            {
                _state = value;
            }
        }

        public override Guid Id
        {
            get { return State.Id; }
            protected set
            {
            }
        }
        public override int Version
        {
            get { return State.Version; }
            protected set
            {
            }
        }

        public void ApplyState(IMemento st)
        {
            if (this.State != null && this.State.Version != 0)
            {
                throw new InvalidOperationException("Can't override existing state");
            }

            TState state = (TState)st;
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

        protected void RaiseEvent(IEvent Event)
        {

            Validate(Event);

            if (this._eventFactory != null)
                this._eventFactory.Fill(Event, this);
            Event.EntityVersion = this.Version + 1;

            base.RaiseEvent(Event); // calls ApplyEvent and increases Version



        }

        private void Validate(IEvent Event)
        {
            if (Event == null)
                throw new ArgumentNullException();
            if (this.Version > 0 && Event.EntityId != this.Id)
                throw new InvalidOperationException();
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
