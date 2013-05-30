using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Growthstories.Sync
{
    public class SynchronizerInMemoryPersistence : IStoreSyncHeads
    {

        private readonly ISet<SyncHead> _syncheads = new HashSet<SyncHead>();
        private ISet<Guid> _public = new HashSet<Guid>();

        public void MarkPublic(Guid StreamId)
        {
            _public.Add(StreamId);
            PersistSyncHead(new SyncHead(StreamId, 0));
        }


        public void MarkPrivate(Guid StreamId)
        {
            _public.Remove(StreamId);
        }

        public IEnumerable<SyncHead> GetSyncHeads()
        {
            return _syncheads;
        }

        public IEnumerable<Guid> GetPublic()
        {
            return _public;
        }

        public bool PersistSyncHead(SyncHead head)
        {
            return _syncheads.Add(head);
        }


        public void Purge()
        {
            _syncheads.Clear();
            _public.Clear();
        }
    }
}
