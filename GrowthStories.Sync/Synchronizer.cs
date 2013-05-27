using System;
using CommonDomain.Core;
using CommonDomain;
using EventStore;
using System.Collections.Generic;
using System.Threading.Tasks;
using Growthstories.Domain.Entities;

namespace Growthstories.Sync
{
    public class Synchronizer
    {
        private readonly IStoreEvents EventStore;
        private readonly ITranslateEvents Translator;
        private readonly ITransportEvents Transporter;
        private readonly IStoreSyncHeads SyncStore;



        public Synchronizer(IStoreEvents eventStore, IStoreSyncHeads syncStore, ITranslateEvents translator, ITransportEvents transporter)
            : base()
        {
            EventStore = eventStore;
            SyncStore = syncStore;
            Translator = translator;
            Transporter = transporter;
        }


        public async Task<bool> Synchronize()
        {
            int MaxTries = 5;
            int Counter = 0;
            while (await Pull() && Counter < MaxTries)
            {
                //if (await Push())
                //{
                //RaiseEvent(new Synchronized(Id));
                //return true;
                //}
                Counter++;
            }
            return false;
        }

        private async Task<bool> Pull()
        {
            ISyncPullResponse r = await Transporter.CreatePullRequest().Execute();
            return true;
        }

        //private async Task<bool> Push()
        //{
        //    var syncDTOs = new List<IEventDTO>();
        //    foreach (Commit commit in PendingSynchronization())
        //    {
        //        foreach (var eventMsg in commit.Events)
        //        {
        //            syncDTOs.Add(Translator.Out((IEvent)eventMsg.Body));
        //        }
        //    }
        //    if (syncDTOs.Count == 0)
        //    {
        //        return true;
        //    }
        //    if (await Push(Transporter.CreatePushRequest(syncDTOs)))
        //    {
        //        return true;
        //    }
        //    return false;
        //}

        private async Task<bool> Push(ISyncPushRequest req)
        {
            ISyncPushResponse r = await req.Execute();
            return true;
        }

    }
}
