using CommonDomain;
using CommonDomain.Persistence;
using EventStore;
using EventStore.Persistence;
using Growthstories.Core;
using System;
using System.Collections.Generic;

namespace Growthstories.Sync
{
    public static class StoreExtensions
    {
        public static void Rebase(this IStoreEvents store, IEventStream stream, ICollection<EventMessage> newEvents)
        {
            var persistence = store.Advanced as IPersistDeleteStreams;
            if (persistence == null)
                throw new InvalidOperationException("Rebase is not supported in this storage engine");
            //store.
            var currentCommits = persistence.GetFrom(stream.StreamId, stream.StreamRevision - stream.CommittedEvents.Count + 1, stream.StreamRevision);

            List<EventMessage> currentEvents = new List<EventMessage>();
            foreach (var commit in currentCommits)
            {
                currentEvents.AddRange(commit.Events);
            }
            var events = new List<EventMessage>(currentEvents);
            events.AddRange(newEvents);

            var newCommit = new Commit(
                stream.StreamId,
                stream.StreamRevision - currentEvents.Count + events.Count,
                Guid.NewGuid(),
                stream.CommitSequence + 1,
                SystemTime.UtcNow,
                null,//this.uncommittedHeaders.ToDictionary(x => x.Key, x => x.Value),
                events);


            persistence.Rebase(currentCommits, newCommit);
            //stream.
        }
    }
}
