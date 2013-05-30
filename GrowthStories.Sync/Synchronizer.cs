using System;
using CommonDomain.Core;
using CommonDomain;
using EventStore;
using System.Collections.Generic;
using System.Threading.Tasks;
using Growthstories.Domain.Entities;
using Growthstories.Core;

namespace Growthstories.Sync
{
    public class Synchronizer
    {
        private readonly IStoreEvents EventStore;
        private readonly ITranslateEvents Translator;
        private readonly ITransportEvents Transporter;
        private readonly IStoreSyncHeads SyncStore;



        public Synchronizer(IStoreEvents eventStore, IStoreSyncHeads syncStore, ITranslateEvents translator, ITransportEvents transporter)
        {
            EventStore = eventStore;
            SyncStore = syncStore;
            Translator = translator;
            Transporter = transporter;
        }


        public Task<bool> Synchronize()
        {
            return Push();
            int MaxTries = 5;
            int Counter = 0;
            //while (await Pull() && Counter < MaxTries)
            //{
            //if (await Push())
            //{
            //RaiseEvent(new Synchronized(Id));
            //return true;
            //}
            Counter++;
            //}
            //return false;
        }

        private async Task<bool> Pull()
        {
            ISyncPullResponse r = await Transporter.CreatePullRequest().Execute();
            return true;
        }

        public ISyncPushRequest GetPushRequest()
        {
            return Transporter.CreatePushRequest(Translator.Out(PendingSynchronization()));
        }

        private Task<bool> Push()
        {
            var req = GetPushRequest();
            var resp = req.Execute();
            return new Task<bool>(() => true);
        }

        public IEnumerable<IEvent> PendingSynchronization()
        {
            foreach (var lastSync in SyncStore.GetSyncHeads())
            {
                IEventStream changes = EventStore.OpenStream(lastSync.StreamId, lastSync.SyncedRevision + 1, int.MaxValue);
                if (changes.StreamRevision > lastSync.SyncedRevision) // updates exist
                {
                    foreach (var @event in changes.CommittedEvents)
                    {
                        yield return (IEvent)@event.Body;
                    }
                }
            }
        }

        private async Task<bool> Push(ISyncPushRequest req)
        {
            ISyncPushResponse r = req.Execute();
            return true;
        }

    }
}
