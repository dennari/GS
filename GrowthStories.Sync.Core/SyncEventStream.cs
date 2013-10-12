using EventStore;
using EventStore.Dispatcher;
using Growthstories.Core;
//using Growthstories.Domain.Messaging;
//using Growthstories.Domain;
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

        public GSCommit[] Commits { get; private set; }

        private Commit[] PendingCommits { get; set; }

        private readonly IList<IEvent> RemoteEvents = new List<IEvent>();

        public ICollection<IEvent> UncommittedRemoteEvents { get { return RemoteEvents; } }

        public SyncStreamType Type { get; set; }
        public long SyncStamp { get; set; }

        private readonly IList<IEvent> Events = new List<IEvent>();


        private readonly ICommitEvents Persistence;
        private readonly IPersistSyncStreams SyncPersistence;

        private static readonly ILog Logger = LogFactory.BuildLogger(typeof(SyncEventStream));





        public SyncEventStream(
            IGrouping<Guid, GSCommit> commits,
            ICommitEvents persistence,
            IPersistSyncStreams syncPersistence)
            : this(commits.Key, persistence, syncPersistence)
        {
            if (commits == null)
                throw new ArgumentNullException();

            this.Commits = commits.OrderBy(x => x.CommitSequence).ToArray();


            this.PopulateStream(int.MinValue, int.MaxValue, this.Commits);
            //this.CheckSyncHead();

        }

        public SyncEventStream(IGrouping<Guid, IEvent> events, ICommitEvents persistence, IPersistSyncStreams syncPersistence) :
            this(events.Key, persistence, syncPersistence)
        {

            //var commits = persistence.GetFrom(this.StreamId, 0, int.MaxValue);

            //var first = commits.FirstOrDefault();

            //if (first == null)
            //    throw new StreamNotFoundException();

            //var ffirst = first as GSCommit;
            //if (ffirst != null)
            //{
            //    this.Type = ffirst.SyncStreamType;
            //}


            //this.PopulateStream(0, int.MaxValue, new Commit[] { first }.Concat(commits));




            foreach (var e in events)
            {

                this.AddRemote(e);

            }

        }




        public SyncEventStream(Guid streamId, ICommitEvents persistence, IPersistSyncStreams syncPersistence)
            : base(streamId, persistence)
        {

            this.Persistence = persistence;
            this.SyncPersistence = syncPersistence;

        }


        public SyncEventStream(GSStreamHead streamHead, ICommitEvents persistence, IPersistSyncStreams syncPersistence)
            : base(streamHead.StreamId, persistence)
        {

            this.Persistence = persistence;
            this.SyncPersistence = syncPersistence;
            this._StreamRevision = streamHead.HeadRevision;
            this.Type = streamHead.SyncStreamType;
            this.SyncStamp = streamHead.SyncStamp;

        }

        public SyncEventStream(Guid streamId, ICommitEvents persistence, int minRevision, int maxRevision, IPersistSyncStreams syncPersistence)
            : base(streamId, persistence)
        {
            this.SyncPersistence = syncPersistence;
            this.Persistence = persistence;

            this.Commits = persistence.GetFrom(streamId, minRevision, maxRevision).Cast<GSCommit>().ToArray();
            if (minRevision > 0 && this.Commits.Length == 0)
                throw new StreamNotFoundException();

            this.PopulateStream(minRevision, maxRevision, this.Commits);

            if (this.Commits.Length > 0)
            {
                Commit last = this.Commits[this.Commits.Length - 1];
                if (last.StreamRevision != this.StreamRevision)
                {
                    this.Commits = this.Commits.Take(this.Commits.Length - 1).ToArray();
                    if (this.Commits.Length > 0)
                    {
                        last = this.Commits[this.Commits.Length - 1];
                        if (last.StreamRevision != this.StreamRevision)
                        {
                            this.Commits = null;

                        }

                    }


                }

            }




        }

        public SyncEventStream(Snapshot snapshot, ICommitEvents persistence, int maxRevision, IPersistSyncStreams syncPersistence)
            : base(snapshot, persistence, maxRevision)
        {
            this.SyncPersistence = syncPersistence;
            this.Persistence = persistence;
        }


        protected int? _StreamRevision;
        public override int StreamRevision
        {
            get
            {
                if (_StreamRevision.HasValue)
                    return _StreamRevision.Value;
                return base.StreamRevision;
            }
        }


        public void AddRemote(IEvent e)
        {
            this.RemoteEvents.Add(e);
            this.Add(e, true);
        }

        public IEnumerable<IEventDTO> Translate(ITranslateEvents translator)
        {
            return this.CommittedEvents
                .Select(x =>
                {
                    var re = translator.Out((IEvent)x.Body);
                    if (re != null)
                        re.AggregateVersion += this.RemoteEvents.Count();
                    return re;
                });

        }

        public void Add(IEvent e, bool setVersion = false)
        {
            var correctVersion = this.StreamRevision + this.Events.Count + 1;
            if (e.AggregateVersion != correctVersion)
            {
                if (!setVersion)
                    throw new InvalidOperationException(string.Format("SyncEventStream Add: event has version {0}, should have {1}", e.AggregateVersion, correctVersion));
                e.AggregateVersion = correctVersion;
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

            base.CommitChanges(commitId);
            this.Events.Clear();

        }



        protected override Commit BuildCommitAttempt(Guid commitId)
        {
            //Logger.Debug(Resources.BuildingCommitAttempt, commitId, this.StreamId);
            return new GSCommit(base.BuildCommitAttempt(commitId), false, Type);
        }


        public void CommitRemoteChanges(Guid commitId)
        {

            if (!this.HasChanges())
                return;

            try
            {
                var attempt = this.BuildCommitAttempt(commitId) as GSCommit;
                attempt.Synchronized = true;
                attempt.SyncStamp = this.SyncStamp;

                //Logger.Debug(Resources.PersistingCommit, commitId, this.StreamId);
                this.Persistence.Commit(attempt);


                this.PopulateStream(this.StreamRevision + 1, attempt.StreamRevision, new[] { attempt });


                this.ClearChanges();
                this.RemoteEvents.Clear();
                //this.Persistence.MoreAdvanced.MarkCommitAsSynchronized(attempt);
            }
            catch (ConcurrencyException)
            {
                //Logger.Info(Resources.UnderlyingStreamHasChanged, this.StreamId);
                var commits = this.Persistence.GetFrom(this.StreamId, this.StreamRevision + 1, int.MaxValue);
                this.PopulateStream(this.StreamRevision + 1, int.MaxValue, commits);

                throw;
            }



        }

        public bool MarkCommitsSynchronized(ISyncPushResponse pushResp = null)
        {
            if (this.Commits == null || this.Commits.Length == 0)
                return false;

            int outerCounter = 0;
            Guid? lastExecuted = null;
            if (pushResp != null && pushResp.StatusCode == GSStatusCode.VERSION_TOO_LOW && pushResp.LastExecuted != default(Guid))
                lastExecuted = pushResp.LastExecuted;




            foreach (var commit in this.Commits)
            {
                int counter = 0;
                if (lastExecuted.HasValue)
                {
                    foreach (var e in commit.Events.Select(x => (IEvent)x.Body))
                    {
                        counter++;
                        if (e.MessageId == lastExecuted.Value)
                            break;
                    }
                }
                if (!lastExecuted.HasValue || counter == commit.Events.Count)
                {
                    this.SyncPersistence.MarkCommitAsSynchronized(commit);
                    outerCounter++;
                }

            }

            return outerCounter == this.Commits.Length;

        }


        private int EnumerateEvents(IEnumerable<EventMessage> events, int Revision, bool set)
        {
            foreach (var e in events)
            {
                var ee = ((IEvent)e.Body);
                if (set)
                    ee.AggregateVersion = Revision;
                else if (ee.AggregateVersion != Revision)
                    throw new InvalidOperationException(string.Format("IEvent.EntityVersion = {0}, Revision = {1}", ee.AggregateVersion, Revision));
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
