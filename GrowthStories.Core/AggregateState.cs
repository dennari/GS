﻿using CommonDomain;
using Microsoft.CSharp.RuntimeBinder;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text;

namespace Growthstories.Core
{


    public abstract class AggregateState : IMemento, IAppliesEvents
    {

        [JsonIgnore]
        public Type AggregateType { get; set; }

        [JsonProperty]
        public Guid Id { get; protected set; }

        [JsonProperty]
        public int Version { get; protected set; }

        [JsonProperty]
        public DateTimeOffset Created { get; protected set; }

        //protected bool? _Public;
        //public bool? Public { get { return _Public; } protected set { Set(ref _Public, value); } }


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


        //public event PropertyChangedEventHandler PropertyChanged;



        //protected bool Set<T>(ref T field, T newValue, [CallerMemberName] string propertyName = null)
        //{
        //    if (propertyName == null)
        //    {
        //        return false;
        //    }
        //    return Set<T>(propertyName, ref field, newValue);
        //}

        //protected bool Set<T>(string propertyName, ref T field, T newValue)
        //{
        //    if (propertyName == null)
        //    {
        //        return false;
        //    }
        //    if (EqualityComparer<T>.Default.Equals(field, newValue))
        //    {
        //        return false;
        //    }

        //    field = newValue;
        //    RaisePropertyChanged(propertyName);
        //    return true;
        //}




        //protected virtual void RaisePropertyChanged(string propertyName)
        //{

        //    var handler = PropertyChanged;
        //    if (handler != null)
        //    {
        //        handler(this, new PropertyChangedEventArgs(propertyName));
        //    }
        //}


    }

    public abstract class EntityState<TCreateEvent> : AggregateState where TCreateEvent : IEvent
    {

        private bool applying = false;

        protected EntityState()
            : base()
        {

        }

        public virtual void Apply(TCreateEvent @event)
        {
            if (Version != 0)
            {
                throw DomainError.Named("rebirth", "Can't create aggregate that already exists");
            }
            Id = @event.EntityId.Value;
            Created = @event.Created;
        }

        public override void Apply(IEvent @event)
        {
            if (Version == 0 && !(@event is TCreateEvent))
            {
                throw DomainError.Named("premature", "Event hasn't been created yet");
            }

            if (this.Id != default(Guid) && @event.EntityId != this.Id)
            {
                throw DomainError.Named("ALIEN_ID", "Nonmatching AggregateId");
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

    public abstract class AggregateState<TCreateEvent> : AggregateState where TCreateEvent : ICreateEvent
    {

        private bool applying = false;
        private readonly HashSet<Guid> AppliedEventIds = new HashSet<Guid>();

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
            Id = @event.AggregateId;
            Created = @event.Created;
        }

        public override void Apply(IEvent @event)
        {
            if (!(@event is TCreateEvent))
            {
                if (Version == 0)
                {
                    throw DomainError.Named("premature", "Aggregate hasn't been created yet");
                }
                if (@event.AggregateId != this.Id)
                {
                    throw DomainError.Named("ALIEN_ID", "Nonmatching AggregateId");
                }
            }
            if (this.applying)
                throw DomainError.Named("nohandler", "Can't find handler for event {0}", @event.GetType().ToString());
            if (AppliedEventIds.Contains(@event.MessageId))
                throw DomainError.Named("duplicate_event", "Event {0} of type {1} has already been applied", @event.MessageId, @event.GetType().ToString());

            try
            {
                this.applying = true;
                ((dynamic)this).Apply((dynamic)@event);
                this.applying = false;
                Version++;
                AppliedEventIds.Add(@event.MessageId);

            }
            catch (RuntimeBinderException)
            {
                throw;
            }

        }

    }
}
