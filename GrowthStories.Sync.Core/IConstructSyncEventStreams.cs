using EventStore;
using Growthstories.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Growthstories.Sync
{
    public interface IConstructSyncEventStreams
    {
        ISyncEventStream CreateStreamFromRemoteEvents(IGrouping<Guid, IEvent> events);

        ISyncEventStream CreateStream(IGrouping<Guid, Commit> commits);
    }
}
