using EventStore;
using EventStore.Dispatcher;
using Growthstories.Core;
using Growthstories.Domain.Messaging;
using System.Linq;
using System;
using System.Collections.Generic;
using EventStore.Persistence;

namespace Growthstories.Sync
{
    public class SyncEventStream : OptimisticEventStream, ISyncEventStream
    {
        private readonly Commit[] Commits;
        private int RebasedRevision = 0;
        private readonly IPersistDeleteStreams Persistence;

        //private readonly ICommitEvents _Events = new LinkedList<IEvent>();


        public SyncEventStream(IGrouping<Guid, IEvent> events, IPersistDeleteStreams persistence)
            : base(events.Key, persistence)
        {
            this.Persistence = persistence;
            foreach (var e in events)
            {
                this.Add(new EventMessage()
                {
                    Body = e
                });
            }
        }

        public SyncEventStream(Guid streamId, ICommitEvents persistence)
            : base(streamId, persistence)
        {

        }

        public SyncEventStream(Guid streamId, IPersistDeleteStreams persistence, int minRevision, int maxRevision)
            : base(streamId, persistence)
        {
            var commits = persistence.GetFrom(streamId, minRevision, maxRevision).Where(cmt => cmt.CommitSequence != 1).ToArray();

            this.PopulateStream(minRevision, maxRevision, commits);

            if (minRevision > 0 && this.CommittedEvents.Count == 0)
                throw new StreamNotFoundException();
            this.Commits = commits;
            this.Persistence = persistence;
        }


        public Guid EntityId { get { return this.StreamId; } }


        public int EntityVersion
        {
            get { throw new System.NotImplementedException(); }
        }

        public IEnumerable<IEvent> Events
        {
            get
            {
                return this.CommittedEvents.Select(x => (IEvent)x.Body).Union(this.UncommittedEvents.Select(x => (IEvent)x.Body));
            }
        }

        public override int GetHashCode()
        {
            return this.StreamId.GetHashCode();
        }
        public override bool Equals(object obj)
        {
            return this.Equals(obj as ISyncEventStream);
        }
        public virtual bool Equals(ISyncEventStream other)
        {
            return null != other && other.StreamId == this.StreamId;
        }


        public void Rebase(ISyncEventStream remoteStream)
        {
            //var persistence = EventStore.Advanced as IPersistDeleteStreams;
            //if (persistence == null)
            //    throw new InvalidOperationException("Rebase is not supported in this storage engine");
            //store.
            if (remoteStream == null)
                throw new ArgumentNullException("remoteStream");
            if (remoteStream.UncommittedEvents.Count == 0)
                throw new InvalidOperationException("Remote stream doesn't have any events");
            if (this.Commits == null || this.Commits.Length == 0)
                throw new InvalidOperationException("Can't rebase without commits");
            if (this.HasChanges())
                throw new InvalidOperationException("Can't rebase with pending changes");


            // it is supposed that the stream only contains the events committed after the last sync
            int streamMax = this.StreamRevision;
            int streamMin = streamMax - this.CommittedEvents.Count + 1;
            // this should not be possible 
            if (streamMin < 2)
                streamMin = 2;

            int Revision = 0;

            foreach (var commit in Commits)
            {

                Revision = commit.StreamRevision - commit.Events.Count + 1;
                if (Revision >= streamMin)
                    continue;
                foreach (var e in commit.Events)
                {
                    if (Revision < streamMin)
                    {
                        var ee = ((IEvent)e.Body);
                        if (ee.EntityVersion != Revision)
                            throw new InvalidOperationException(string.Format("IEvent.EntityVersion = {0}, Revision = {1}", ee.EntityVersion, Revision));
                        this.Add(e);
                    }
                    Revision++;
                }
            }
            Revision = streamMin;
            foreach (var e in remoteStream.UncommittedEvents)
            {
                var ee = ((IEvent)e.Body);
                if (ee.EntityVersion != Revision)
                    throw new InvalidOperationException(string.Format("IEvent.EntityVersion = {0}, Revision = {1}", ee.EntityVersion, Revision));
                Revision++;
                this.Add(e);
            }
            foreach (var commit in Commits)
            {
                //Revision = commit.StreamRevision - commit.Events.Count + 1;
                foreach (var e in commit.Events)
                {
                    //if (Revision >= streamMin)
                    //{
                    var ee = ((IEvent)e.Body);
                    ee.EntityVersion = Revision;
                    //e.Body = ee;
                    //if (ee.EntityVersion != Revision)
                    //   throw new InvalidOperationException(string.Format("IEvent.EntityVersion = {0}, Revision = {1}", ee.EntityVersion, Revision));

                    this.Add(e);
                    //}
                    Revision++;
                }
            }
            this.RebasedRevision = Revision - 1;

        }

        protected override void PersistChanges(Guid commitId)
        {
            //base.PersistChanges();
            var attempt = this.BuildCommitAttempt(commitId);

            //Logger.Debug(Resources.PersistingCommit, commitId, this.StreamId);

            if (this.RebasedRevision > 0)
            {
                this.Persistence.Rebase(this.Commits, attempt);

            }
            else
            {
                this.Persistence.Commit(attempt);

            }

            this.RebasedRevision = 0;
            this.PopulateStream(this.StreamRevision + 1, attempt.StreamRevision, new[] { attempt });
            this.ClearChanges();
        }

        protected virtual Commit BuildCommitAttempt(Guid commitId)
        {
            //Logger.Debug(Resources.BuildingCommitAttempt, commitId, this.StreamId);
            if (this.RebasedRevision > 0)
                return new Commit(
                    this.StreamId,
                    this.RebasedRevision,
                    commitId,
                    this.CommitSequence + 1 - this.Commits.Length,
                    SystemTime.UtcNow,
                    this.UncommittedHeaders.ToDictionary(x => x.Key, x => x.Value),
                    this.UncommittedEvents.ToList());
            return base.BuildCommitAttempt(commitId);
        }



    }
}
