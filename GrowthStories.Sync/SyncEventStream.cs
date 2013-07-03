using EventStore;
using EventStore.Dispatcher;
using Growthstories.Core;
using Growthstories.Domain.Messaging;
using System.Linq;
using System;
using System.Collections.Generic;
using EventStore.Persistence;
using EventStore.Logging;

namespace Growthstories.Sync
{
    public class SyncEventStream : OptimisticEventStream, ISyncEventStream
    {
        public Commit[] Commits { get; private set; }
        private readonly IPersistSyncStreams Persistence;
        private static readonly ILog Logger = LogFactory.BuildLogger(typeof(SyncEventStream));


        public SyncEventStream(IGrouping<Guid, Commit> commits, IPersistSyncStreams persistence)
            : base(commits.Key, persistence)
        {
            if (commits == null)
                throw new ArgumentNullException();

            this.Commits = commits.OrderBy(x => x.CommitSequence).ToArray();
            this.Persistence = persistence;

            this.PopulateStream(int.MinValue, int.MaxValue, this.Commits);

        }


        public SyncEventStream(IGrouping<Guid, IEvent> events, IPersistSyncStreams persistence)
            : base(events.Key, persistence)
        {
            if (events == null)
                throw new ArgumentNullException();
            this.Persistence = persistence;
            foreach (var e in events)
            {
                this.Add(new EventMessage()
                {
                    Body = e
                });
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

            //int Revision = ;

            int Revision = EnumerateEvents(remoteStream.UncommittedEvents, streamMin, false);
            int RebaseSequence = 0;
            Commit[] newCommits = new Commit[Commits.Length + 1];
            newCommits[0] = new Commit(
                this.StreamId,
                Revision - 1,
                Guid.NewGuid(),
                this.Commits[0].CommitSequence,
                DateTime.Now,
                new Dictionary<string, object>() { { "REBASE_SEQUENCE", RebaseSequence } },
                remoteStream.UncommittedEvents.ToList()
            );

            foreach (var commit in Commits)
            {
                RebaseSequence++;
                Revision = EnumerateEvents(commit.Events, Revision, true);
                commit.Headers.Add("REBASE_SEQUENCE", RebaseSequence);
                newCommits[RebaseSequence] = new Commit(
                    this.StreamId,
                    Revision - 1,
                    Guid.NewGuid(),
                    commit.CommitSequence + 1,
                    DateTime.Now,
                    commit.Headers,
                    commit.Events.ToList()
                 );
            }

            this.Persistence.Rebase(Commits, newCommits);

        }

        private int EnumerateEvents(IEnumerable<EventMessage> events, int Revision, bool set)
        {
            foreach (var e in events)
            {
                var ee = ((IEvent)e.Body);
                if (set)
                    ee.EntityVersion = Revision;
                else if (ee.EntityVersion != Revision)
                    throw new InvalidOperationException(string.Format("IEvent.EntityVersion = {0}, Revision = {1}", ee.EntityVersion, Revision));
                Revision++;
            }
            return Revision;
        }




        public void CommitPullChanges(Guid commitId)
        {

            Logger.Debug("CommitPullChanges {0}", this.StreamId);

            if (!this.HasChanges())
                return;

            try
            {
                var attempt = this.BuildCommitAttempt(commitId);

                Logger.Debug("Persisting pull changes in commit {0}, stream {1} ", commitId, this.StreamId);
                this.Persistence.Commit(attempt);
                this.Persistence.MarkCommitAsSynchronized(attempt);

                this.PopulateStream(this.StreamRevision + 1, attempt.StreamRevision, new[] { attempt });
                this.ClearChanges();
            }
            catch (ConcurrencyException)
            {
                Logger.Info("Underlying stream {0} has changed", this.StreamId);
                var commits = this.Persistence.GetFrom(this.StreamId, this.StreamRevision + 1, int.MaxValue);
                this.PopulateStream(this.StreamRevision + 1, int.MaxValue, commits);

                throw;
            }
        }
    }
}
