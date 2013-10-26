using System;
using System.Collections.Generic;
using CommonDomain;
using CommonDomain.Core;
using Growthstories.Core;
using System.Reflection;
using EventStore.Logging;
using EventStore;
using Microsoft.CSharp.RuntimeBinder;

namespace Growthstories.Core
{
    public enum PullStreamType
    {
        NULL,
        USER,
        PLANT
    }


    public interface IGSAggregate : IAggregate, IApplyState
    {
        void SetEventFactory(IEventFactory factory);
        PullStreamType SyncStreamType { get; }
        bool UIPersistable { get; }
        void Handle(IMessage msg);
        IAggregateState State { get; }
        void ApplyRemoteMessage(IMessage Event);
        bool HasUncommittedEvents { get; }

    }

    public abstract class AggregateBase<TState, TCreate> : AggregateBase, IGSAggregate
        where TState : AggregateState, new()
        where TCreate : ICreateMessage
    {

        public void Handle(IMessage @event)
        {
            if (@event == null)
                throw new ArgumentNullException("event");

            if (this.applying)
                throw new InvalidOperationException(string.Format("Can't find handler for message {0}", @event.GetType().ToString()));
            try
            {
                this.applying = true;
                ((dynamic)this).Handle((dynamic)@event);
                this.applying = false;
            }
            catch (RuntimeBinderException)
            {
                throw;
            }


        }

        public bool HasUncommittedEvents
        {
            get
            {
                return ((IAggregate)this).GetUncommittedEvents().Count > 0;
            }
        }


        public PullStreamType SyncStreamType { get; protected set; }
        public bool UIPersistable { get; protected set; }

        private bool applying = false;
        private TState _state;
        private IEventFactory _eventFactory;
        private static ILog Logger = LogFactory.BuildLogger(typeof(TState));

        private readonly ICollection<IEvent> UncommittedRemoteEvents = new LinkedList<IEvent>();




        public AggregateBase()
        {
            this.ApplyState(this.InitializeState());
        }

        public void SetEventFactory(IEventFactory factory)
        {
            this._eventFactory = factory;
        }

        public TState State
        {
            get
            {
                return _state;
            }
            private set
            {
                _state = value;
                _state.AggregateType = this.GetType();
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
            // Let us only override an existing state with Version equal to 0
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
            Event.AggregateVersion = this.Version + 1;

            var sE = Event as IAggregateEvent<TState>;
            if (sE != null)
            {
                sE.AggregateState = this.State;
            }


            Logger.Info("Raised event: {0}", Event.ToString());
            base.RaiseEvent(Event); // calls ApplyEvent and increases Version
            //this.AppliedEventIds.Add(Event.MessageId);

        }

        public void ApplyRemoteMessage(IMessage e)
        {
            var Event = e as IEvent;
            if (Event == null)
                throw new ArgumentException("Only remote EVENTS accepted");

            Validate(Event);

            // this is here ON PURPOSE
            Event.AggregateVersion = this.Version + 1;

            var sE = Event as IAggregateEvent<TState>;
            if (sE != null)
            {
                sE.AggregateState = this.State;
            }


            Logger.Info("Raised REMOTE event: {0}", Event.ToString());
            base.RaiseEvent(Event);
        }


        private void Validate(IEvent Event)
        {
            if (Event == null)
                throw new ArgumentNullException("Event");
            //var eb = Event as EventBase;
            //if (this.Version > 0 && Event.ParentId != this.Id)
            //    throw new ArgumentException(string.Format("Event {0} EntityId doesn't match the Id of the raising entity {1}", Event.GetType(), this.GetType()));
            //if (this.Version > 0 && Event.EntityId != this.Id)
            //    throw new ArgumentException(string.Format("Event {0} EntityId doesn't match the Id of the raising entity {1}", Event.GetType(), this.GetType()));
        }

        //public void Create(Guid Id)
        //{
        //    if (State == null)
        //    {
        //        ApplyState(null);
        //    }
        //    RaiseEvent(Activator.CreateInstance(typeof(TCreate), Id));

        //}


        protected override IMemento GetSnapshot()
        {
            return State;
        }



        IAggregateState IGSAggregate.State
        {
            get { return State; }
        }







    }
}
