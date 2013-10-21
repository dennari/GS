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

        public Commit[] Commits { get; private set; }

        private Commit[] PendingCommits { get; set; }

        private readonly IList<IEvent> RemoteEvents = new List<IEvent>();

        public ICollection<IEvent> UncommittedRemoteEvents { get { return RemoteEvents; } }

        public SyncStreamType Type { get; set; }
        public long SyncStamp { get; set; }

        private readonly IList<IEvent> Events = new List<IEvent>();


        private readonly IPersistSyncStreams SyncPersistence;

        private static readonly ILog Logger = LogFactory.BuildLogger(typeof(SyncEventStream));





        public SyncEventStream(
            IGrouping<Guid, GSCommit> commits,
            IPersistSyncStreams syncPersistence)
            : this(commits.Key, syncPersistence)
        {
            if (commits == null)
                throw new ArgumentNullException();

            this.Commits = commits.OrderBy(x => x.CommitSequence).ToArray();


            this.PopulateStream(int.MinValue, int.MaxValue, this.Commits);
            //this.CheckSyncHead();

        }

        public SyncEventStream(IGrouping<Guid, IEvent> events, IPersistSyncStreams syncPersistence) :
            this(events.Key, syncPersistence)
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




        public SyncEventStream(Guid streamId, IPersistSyncStreams syncPersistence)
            : base(streamId, syncPersistence)
        {

            this.SyncPersistence = syncPersistence;

        }


        public SyncEventStream(StreamHead streamHead, IPersistSyncStreams syncPersistence)
            : base(streamHead.StreamId, syncPersistence)
        {

            this.SyncPersistence = syncPersistence;
            this._StreamRevision = streamHead.HeadRevision;

        }

        public SyncEventStream(Guid streamId, int minRevision, int maxRevision, IPersistSyncStreams syncPersistence)
            : base(streamId, syncPersistence)
        {
            this.SyncPersistence = syncPersistence;

            this.Commits = syncPersistence.GetFrom(streamId, minRevision, maxRevision).Cast<GSCommit>().ToArray();
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

        public SyncEventStream(Snapshot snapshot, int maxRevision, IPersistSyncStreams syncPersistence)
            : base(snapshot, syncPersistence, maxRevision)
        {
            this.SyncPersistence = syncPersistence;
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

        //public IEnumerable<IEventDTO> Translate(ITranslateEvents translator)
        //{
        //    return this.CommittedEvents
        //        .Select(x =>
        //        {
        //            var re = translator.Out((IEvent)x.Body);
        //            if (re != null)
        //                re.AggregateVersion += this.RemoteEvents.Count();
        //            return re;
        //        });

        //}

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



        public override void CommitChanges(Guid commitId)
        {

            base.CommitChanges(commitId);
            this.Events.Clear();

        }


    }
}
