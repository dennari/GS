using EventStore;
using EventStore.Dispatcher;
using Growthstories.Core;
using Growthstories.Domain.Messaging;
using System.Linq;

namespace Growthstories.Sync
{
    public class SyncDispatcher : IDispatchCommits
    {
        private readonly IStoreSyncHeads SyncStore;

        public SyncDispatcher(IStoreSyncHeads syncStore)
        {
            SyncStore = syncStore;
        }

        public void Dispatch(Commit commit)
        {
            foreach (var @event in commit.Events.Select(msg => (IEvent)msg.Body))
            {
                if ((@event is PlantCreated) || ((@event is MarkedGardenPublic)))
                {
                    SyncStore.MarkPublic(@event.EntityId);
                }
                if ((@event is MarkedPlantPrivate) || ((@event is MarkedGardenPrivate)))
                {
                    SyncStore.MarkPrivate(@event.EntityId);
                }
            }
        }

        public void Dispose()
        {
        }
    }
}
