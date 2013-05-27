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
            if (_state != null)
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


        public void Create(Guid Id)
        {

            RaiseEvent(Activator.CreateInstance(typeof(TCreate), Id));

        }

        //public void ThrowOnInvalidStateTransition(ICommand c)
        //{
        //    if (Version == 0)
        //    {
        //        if (c is TCreate)
        //        {
        //            return;
        //        }
        //        throw DomainError.Named("premature", "Can't do anything to unexistent aggregate");
        //    }
        //    if (Version == -1)
        //    {
        //        throw DomainError.Named("zombie", "Can't do anything to deleted aggregate.");
        //    }
        //    if (c is TCreate)
        //        throw DomainError.Named("rebirth", "Can't create aggregate that already exists");

        //}

        protected override IMemento GetSnapshot()
        {
            return State;
        }

    }
}
