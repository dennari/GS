using EventStore;
using EventStore.Dispatcher;
using Growthstories.Core;
using Growthstories.Domain.Messaging;
using System.Linq;
using System;
using System.Collections.Generic;
using EventStore.Persistence;
using EventStore.Logging;
using Growthstories.Domain;

namespace Growthstories.Sync
{
    public class SyncEventStreamFactory : IConstructSyncEventStreams
    {
        private readonly GSEventStore Persistence;
        private readonly IDictionary<Guid, ISyncEventStream> Cache = new Dictionary<Guid, ISyncEventStream>();

        public SyncEventStreamFactory(GSEventStore persistence)
        {
            this.Persistence = persistence;
        }

        public ISyncEventStream CreateStreamFromRemoteEvents(IGrouping<Guid, IEvent> events)
        {
            ISyncEventStream s = null;
            if (!Cache.TryGetValue(events.Key, out s))
            {
                s = new SyncEventStream(events.Key, Persistence);
                Cache[events.Key] = s;
            }
            foreach (var e in events)
                s.AddRemote(e);
            return s;
        }


        public ISyncEventStream CreateStream(IGrouping<Guid, Commit> commits)
        {
            // we know these are local, so overwrite cache
            var s = new SyncEventStream(commits, Persistence);
            Cache[commits.Key] = s;
            return s;
        }
    }
}
