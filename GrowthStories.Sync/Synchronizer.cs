using System;
using System.Linq;
using CommonDomain.Core;
using CommonDomain;
using EventStore;
using System.Collections.Generic;
using System.Threading.Tasks;
using Growthstories.Domain.Entities;
using Growthstories.Core;
using System.Net.Http;

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


        public async Task<int> Synchronize()
        {

            int MaxTries = 5;
            int Counter = 0;

            ICollection<IEventStream> pending = UpdatedStreams().ToArray();
            if (pending.Count == 0)
            {
                return Counter;
            }
            ISyncPushResponse pushResp = await Transporter.PushAsync(Transporter.CreatePushRequest(Translator.Out(EventsFromStreams(pending)))); // try pushing
            ISyncPullResponse pullResp = null;
            ICollection<IEvent> incoming = null;
            Counter++;

            while (Counter < MaxTries && pushResp.StatusCode != 200) // if push didn't go through and we haven't exceeded maxtries, try pulling and pushing
            {

                // pull
                pullResp = await Transporter.PullAsync(Transporter.CreatePullRequest());
                incoming = Translator.In(pullResp.Events);
                if (incoming.Count > 0)
                {
                    Rebase(pending, incoming);
                }

                // start anew by pushing again
                pending = UpdatedStreams().ToArray();
                pushResp = await Transporter.PushAsync(Transporter.CreatePushRequest(Translator.Out(EventsFromStreams(pending))));

                Counter++;
            }
            return Counter;
        }

        public void Rebase(ICollection<IEventStream> outgoing, ICollection<IEvent> incoming)
        {

            var o = outgoing.ToDictionary(x => x.StreamId);
            var i = incoming.GroupBy(x => x.EntityId).ToDictionary(x => x.Key);

            var oIds = o.Keys;
            var iIds = i.Keys;

            var conflicting = iIds.Intersect(oIds);
            var nonconflicting = iIds.Except(conflicting);

            foreach (var cId in conflicting)
            {
                EventStore.Rebase(o[cId], i[cId].Select(x => new EventMessage() { Body = x }).ToArray());
            }



        }


        public ISyncPushRequest GetPushRequest()
        {
            return Transporter.CreatePushRequest(Translator.Out(PendingSynchronization()));
        }

        public IEnumerable<IEvent> PendingSynchronization()
        {
            return EventsFromStreams(UpdatedStreams());
        }

        public IEnumerable<IEvent> EventsFromStreams(IEnumerable<IEventStream> streams)
        {
            foreach (var stream in streams)
            {
                foreach (var @event in stream.CommittedEvents)
                {
                    yield return (IEvent)@event.Body;
                }
            }
        }

        public IEnumerable<IEventStream> UpdatedStreams()
        {
            foreach (var lastSync in SyncStore.GetSyncHeads())
            {
                IEventStream changes = EventStore.OpenStream(lastSync.StreamId, lastSync.SyncedRevision + 1, int.MaxValue);
                if (changes.StreamRevision > lastSync.SyncedRevision) // updates exist
                {
                    yield return changes;
                }
            }
        }


    }
}
