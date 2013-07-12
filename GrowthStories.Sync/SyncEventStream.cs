using EventStore;
using EventStore.Dispatcher;
using Growthstories.Core;
using Growthstories.Domain.Messaging;
using Growthstories.Domain;
using System.Linq;
using System;
using System.Collections.Generic;
using EventStore.Persistence;
using EventStore.Logging;

namespace Growthstories.Sync
{
    public class SyncEventStream : OptimisticEventStream, ISyncEventStream
    {
        public const string REMOTE_COMMIT_HEADER = "REMOTE_COMMIT";

        public Commit[] Commits { get; private set; }

        private Commit[] PendingCommits { get; set; }

        private readonly IList<IEvent> RemoteEvents = new List<IEvent>();

        public ICollection<IEvent> UncommittedRemoteEvents { get { return RemoteEvents; } }


        private readonly IList<IEvent> Events = new List<IEvent>();


        private readonly GSEventStore Persistence;
        private static readonly ILog Logger = LogFactory.BuildLogger(typeof(SyncEventStream));


        public SyncEventStream(IGrouping<Guid, Commit> commits, GSEventStore persistence)
            : this(commits.Key, persistence)
        {
            if (commits == null)
                throw new ArgumentNullException();

            this.Commits = commits.OrderBy(x => x.CommitSequence).ToArray();


            this.PopulateStream(int.MinValue, int.MaxValue, this.Commits);
            this.CheckSyncHead();

        }

        protected void CheckSyncHead()
        {
            var shouldBe = Persistence.MoreAdvanced.GetStreamRevision(this.StreamId);
            //var actually = commits.Last().StreamRevision;
            var diff = shouldBe - this.StreamRevision;
            if (diff > 0)
            {
                foreach (var e in this.Events())
                {
                    e.EntityVersion += diff;
                }
            }
        }


        public SyncEventStream(Guid streamId, GSEventStore persistence)
            : base(streamId, persistence)
        {

            this.Persistence = persistence;

        }

        public void AddRemote(IEvent e)
        {
            this.RemoteEvents.Add(e);
            this.Add(e, true);
            this.UncommittedHeaders[REMOTE_COMMIT_HEADER] = 1;
        }

        public void Add(IEvent e, bool setVersion = false)
        {
            var correctVersion = this.StreamRevision + this.Events.Count + 1;
            if (e.EntityVersion != correctVersion)
            {
                if (!setVersion)
                    throw new InvalidOperationException(string.Format("SyncEventStream Add: event has version {0}, should have {1}", e.EntityVersion, correctVersion));
                e.EntityVersion = correctVersion;
            }
            this.Events.Add(e);
            base.Add(new EventMessage() { Body = e });
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
            this.PendingCommits = newCommits;
            //this.Persistence.Rebase(Commits, newCommits);

            //return remoteStream.UncommittedEvents.Select(x => (IEvent)x.Body).Concat(this.Events());
        }

        public override void CommitChanges(Guid commitId)
        {
            if (this.PendingCommits == null)
            {
                base.CommitChanges(commitId);
                this.Events.Clear();
                this.RemoteEvents.Clear();
                return;
            }
            this.Persistence.Rebase(Commits, PendingCommits);
            this.PendingCommits = null;
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




        //public void CommitPullChanges(Guid commitId)
        //{

        //    Logger.Debug("CommitPullChanges {0}", this.StreamId);

        //    if (!this.HasChanges())
        //        return;

        //    try
        //    {
        //        var attempt = this.BuildCommitAttempt(commitId);

        //        Logger.Debug("Persisting pull changes in commit {0}, stream {1} ", commitId, this.StreamId);
        //        this.Persistence.Commit(attempt);
        //        this.Persistence.MarkCommitAsSynchronized(attempt);

        //        this.PopulateStream(this.StreamRevision + 1, attempt.StreamRevision, new[] { attempt });
        //        this.ClearChanges();
        //    }
        //    catch (ConcurrencyException)
        //    {
        //        Logger.Info("Underlying stream {0} has changed", this.StreamId);
        //        var commits = this.Persistence.GetFrom(this.StreamId, this.StreamRevision + 1, int.MaxValue);
        //        this.PopulateStream(this.StreamRevision + 1, int.MaxValue, commits);

        //        throw;
        //    }
        //}
    }
}
