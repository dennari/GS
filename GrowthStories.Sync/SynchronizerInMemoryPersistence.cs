using EventStore.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Growthstories.Sync
{
    public class SynchronizerInMemoryPersistence : IStoreSyncHeads
    {
        private static readonly ILog Logger = LogFactory.BuildLogger(typeof(SynchronizerInMemoryPersistence));
        private readonly ISet<SyncHead> _syncheads = new HashSet<SyncHead>();
        private ISet<Guid> _public = new HashSet<Guid>();
        private bool disposed;

        public void MarkPublic(Guid StreamId)
        {
            if (_public.Add(StreamId))
                PersistSyncHead(new SyncHead(StreamId, 0));
        }


        public void MarkPrivate(Guid StreamId)
        {
            _public.Remove(StreamId);
        }

        public IEnumerable<SyncHead> GetSyncHeads()
        {
            this.ThrowWhenDisposed();
            lock (_syncheads)
                return _syncheads.ToArray();
        }

        public IEnumerable<Guid> GetPublic()
        {
            return _public;
        }

        public bool PersistSyncHead(SyncHead head)
        {
            this.ThrowWhenDisposed();
            lock (_syncheads)
            {
                if (_syncheads.Contains(head))
                    _syncheads.Remove(head);
                return _syncheads.Add(head);

            }

        }


        public void Purge()
        {
            lock (_syncheads)
            {
                _syncheads.Clear();

            }
            lock (_public)
            {
                _public.Clear();

            }
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            this.disposed = true;
            Logger.Info("Disposed");
            //this.persistence.Dispose();
            //foreach (var hook in this.pipelineHooks)
            //    hook.Dispose();
        }

        private void ThrowWhenDisposed()
        {
            if (!this.disposed)
                return;

            var msg = "Attempted to use after disposing";
            Logger.Warn(msg);
            throw new ObjectDisposedException(msg);
        }
    }
}
