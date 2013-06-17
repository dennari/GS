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

                SyncStore.MarkPublic(@event.EntityId);

            }
        }

        public void Dispose()
        {
        }
    }
}
