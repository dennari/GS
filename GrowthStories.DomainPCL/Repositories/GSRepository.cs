﻿using CommonDomain;
using CommonDomain.Persistence;
//using CommonDomain.Persistence.EventStore;
using EventStore;
using EventStore.Persistence;
using Growthstories.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Threading.Tasks;

namespace Growthstories.Domain
{
    public class GSRepository : IGSRepository, IDisposable
    {

        private const string AggregateTypeHeader = "AggregateType";

        private readonly IDictionary<Guid, Snapshot> snapshots = new Dictionary<Guid, Snapshot>();
        private readonly IDictionary<Guid, IGSAggregate> aggregates = new Dictionary<Guid, IGSAggregate>();
        private readonly IDictionary<Guid, IEventStream> streams = new Dictionary<Guid, IEventStream>();
        private readonly IStoreEvents eventStore;
        private readonly IDetectConflicts conflictDetector;
        private readonly IAggregateFactory factory;
        public GSRepository(
            IStoreEvents eventStore,
            IDetectConflicts conflictDetector,
            IAggregateFactory factory)
        {

            this.eventStore = eventStore;
            this.conflictDetector = conflictDetector;
            this.factory = factory;

        }




        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (!disposing)
                return;

            lock (this.streams)
            {
                foreach (var stream in this.streams)
                    stream.Value.Dispose();

                this.snapshots.Clear();
                this.streams.Clear();
            }
        }


        public void PlayById(IGSAggregate aggregate, Guid id)
        {
            PlayById(aggregate, id, int.MaxValue);
        }

        public void PlayById(IGSAggregate aggregate, Guid id, int versionToLoad)
        {
            if (aggregate.Version != 0 || aggregate.Id != default(Guid))
            {
                throw new ArgumentException("Can't play on top of existing events", "aggregate");
            }

            var snapshot = this.GetSnapshot(id, versionToLoad);
            var stream = this.OpenStream(id, versionToLoad, snapshot);
            if (snapshot != null)
                aggregate.ApplyState((IMemento)snapshot.Payload);

            ApplyEventsToAggregate(versionToLoad, stream, aggregate);
            lock (this.aggregates)
            {
                this.aggregates[id] = aggregate;
            }

        }

        private static void ApplyEventsToAggregate(int versionToLoad, IEventStream stream, IAggregate aggregate)
        {
            if (versionToLoad == 0 || aggregate.Version < versionToLoad)
                foreach (var @event in stream.CommittedEvents.Select(x => x.Body))
                    aggregate.ApplyEvent(@event);
        }


        private Snapshot GetSnapshot(Guid id, int version)
        {
            Snapshot snapshot;
            if (!this.snapshots.TryGetValue(id, out snapshot))
            {
                snapshot = this.eventStore.Advanced.GetSnapshot(id, version);
                if (snapshot != null)
                    this.snapshots[id] = snapshot;
            }


            return snapshot;
        }
        private IEventStream OpenStream(Guid id, int version, Snapshot snapshot)
        {
            IEventStream stream;
            if (this.streams.TryGetValue(id, out stream))
                return stream;

            stream = snapshot == null
                ? this.eventStore.OpenStream(id, 0, version)
                : this.eventStore.OpenStream(snapshot, version);

            return this.streams[id] = stream;
        }

        public virtual void Save(IGSAggregate aggregate)
        {

            Guid commitId = Guid.NewGuid();
            Action<IDictionary<string, object>> updateHeaders = null;
            var headers = PrepareHeaders(aggregate, updateHeaders);
            while (true)
            {
                var stream = this.PrepareStream(aggregate, headers);
                var commitEventCount = stream.CommittedEvents.Count;

                try
                {
                    stream.CommitChanges(commitId);
                    aggregate.ClearUncommittedEvents();
                    return;
                }
                catch (DuplicateCommitException)
                {
                    stream.ClearChanges();
                    return;
                }
                catch (ConcurrencyException e)
                {
                    if (this.ThrowOnConflict(stream, commitEventCount))
                        throw new ConflictingCommandException(e.Message, e);

                    stream.ClearChanges();
                }
                catch (StorageException e)
                {
                    throw new PersistenceException(e.Message, e);
                }
            }
        }
        private IEventStream PrepareStream(IAggregate aggregate, Dictionary<string, object> headers)
        {
            IEventStream stream;
            if (!this.streams.TryGetValue(aggregate.Id, out stream))
                this.streams[aggregate.Id] = stream = this.eventStore.CreateStream(aggregate.Id);

            foreach (var item in headers)
                stream.UncommittedHeaders[item.Key] = item.Value;

            foreach (var e in aggregate.GetUncommittedEvents()
                .Cast<object>()
                .Select(x => new EventMessage { Body = x }))
            {
                stream.Add(e);
            }

            return stream;
        }
        private static Dictionary<string, object> PrepareHeaders(IAggregate aggregate, Action<IDictionary<string, object>> updateHeaders)
        {
            var headers = new Dictionary<string, object>();

            headers[AggregateTypeHeader] = aggregate.GetType().FullName;
            if (updateHeaders != null)
                updateHeaders(headers);

            return headers;
        }
        private bool ThrowOnConflict(IEventStream stream, int skip)
        {
            var committed = stream.CommittedEvents.Skip(skip).Select(x => x.Body);
            var uncommitted = stream.UncommittedEvents.Select(x => x.Body);
            return this.conflictDetector.ConflictsWith(uncommitted, committed);
        }


        public IGSAggregate GetById(Guid id)
        {
            IGSAggregate aggregate = null;
            if (this.aggregates.TryGetValue(id, out aggregate))
                return aggregate;

            var versionToLoad = int.MaxValue;
            var snapshot = this.GetSnapshot(id, versionToLoad);

            var stream = this.OpenStream(id, versionToLoad, snapshot);

            if (snapshot != null)
            {
                var state = snapshot.Payload as AggregateState;
                if (state != null)
                {
                    aggregate = (IGSAggregate)factory.Build(state.AggregateType);
                    aggregate.ApplyState((IMemento)snapshot.Payload);
                }
            }
            if (aggregate == null)
            {
                var createEvent = stream.CommittedEvents.First().Body as ICreateEvent;
                if (createEvent != null)
                    aggregate = (IGSAggregate)factory.Build(createEvent.AggregateType);
            }
            if (aggregate == null)
                throw new InvalidOperationException(string.Format("Can't find the Type for aggregate id {0}", id));
            ApplyEventsToAggregate(versionToLoad, stream, aggregate);
            lock (this.aggregates)
            {
                this.aggregates[id] = aggregate;
            }

            return aggregate;
        }

        //public TAggregate GetById<TAggregate>(Guid id) where TAggregate : IGSAggregate, new()
        //{
        //    IGSAggregate aggregate = null;
        //    if (this.aggregates.TryGetValue(id, out aggregate))
        //        return aggregate;

        //    aggregate = factory.Build<TAggregate>();
        //    PlayById(aggregate, id);
        //    return aggregate;
        //}
    }
}
