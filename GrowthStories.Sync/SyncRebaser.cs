using EventStore;
using EventStore.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Growthstories.Sync
{
    public class SyncRebaser : IRebaseEvents
    {

        private readonly IPersistDeleteStreams EventStore;
        private readonly IStoreSyncHeads SyncStore;

        public SyncRebaser(IPersistDeleteStreams eventStore, IStoreSyncHeads syncStore)
        {
            EventStore = eventStore;
            SyncStore = syncStore;
        }


        public ISyncPushRequest Rebase(ISyncPushRequest pushReq, ISyncPullResponse pullResp)
        {

            foreach (var remoteStream in pullResp.Streams.Intersect(pushReq.Streams))
            {
                var localStream = pushReq.Streams.Single(x => x.EntityId == remoteStream.EntityId);
                localStream.Rebase(remoteStream);
                localStream.CommitChanges(Guid.NewGuid());
                //SyncStore.PersistSyncHead(new SyncHead(localStream.StreamId, localStream.StreamRevision));
            }

            foreach (var remoteStream in pullResp.Streams.Except(pushReq.Streams))
            {
                // do something with nonconflicting
            }
            return pushReq;
        }

        /*
        protected void RebaseStream(IEntityEventStream stream, IEntityEventStream inStream)
        {
            var persistence = EventStore.Advanced as IPersistDeleteStreams;
            if (persistence == null)
                throw new InvalidOperationException("Rebase is not supported in this storage engine");
            //store.


            // it is supposed that the stream only contains the events committed after the last sync
            int streamMax = stream.StreamRevision;
            int streamMin = streamMax - stream.Events.Count + 1;
            //if (streamMin < 2)
            //{
            //    streamMin = 2; // 
            //}
            var currentCommits = persistence.GetFrom(stream.StreamId, streamMin, streamMax);

            List<EventMessage> rebasedEvents = new List<EventMessage>();

            int Revision = 0;
            int currentCommitCount = 0;

            foreach (var commit in currentCommits)
            {
                currentCommitCount++;
                Revision = commit.StreamRevision - commit.Events.Count + 1;
                foreach (var e in commit.Events)
                {
                    if (Revision < streamMin)
                    {
                        rebasedEvents.Add(e);
                    }
                    Revision++;
                }
            }
            rebasedEvents.AddRange(newEvents);
            foreach (var commit in currentCommits)
            {
                Revision = commit.StreamRevision - commit.Events.Count + 1;
                foreach (var e in commit.Events)
                {
                    if (Revision >= streamMin)
                    {
                        rebasedEvents.Add(e);
                    }
                    Revision++;
                }
            }


            var newCommit = new Commit(
                stream.StreamId,
                streamMin + rebasedEvents.Count - 1,
                Guid.NewGuid(),
                stream.CommitSequence - currentCommitCount + 1,
                SystemTime.UtcNow,
                null,//this.uncommittedHeaders.ToDictionary(x => x.Key, x => x.Value),
                rebasedEvents);


            persistence.Rebase(currentCommits, newCommit);
        }
        */
        public IEnumerable<ISyncEventStream> Pending()
        {
            foreach (var lastSync in SyncStore.GetSyncHeads())
            {
                ISyncEventStream changes = null;
                try
                {
                    changes = new SyncEventStream(lastSync.StreamId, this.EventStore, lastSync.SyncedRevision + 1, int.MaxValue);

                }
                catch (StreamNotFoundException) { }

                if (changes != null && changes.StreamRevision > lastSync.SyncedRevision) // updates exist
                {
                    yield return changes;
                }
            }
        }


    }
}
