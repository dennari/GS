using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Growthstories.Sync
{
    public interface IStoreSyncHeads
    {
        void MarkPublic(Guid StreamId);

        void MarkPrivate(Guid StreamId);

        IEnumerable<SyncHead> GetSyncHeads();

        IEnumerable<Guid> GetPublic();

        bool PersistSyncHead(SyncHead head);



        void Purge();
    }
}
