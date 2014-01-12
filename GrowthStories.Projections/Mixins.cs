using Growthstories.Sync;
using Growthstories.UI.ViewModel;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Text;
using System.Threading.Tasks;
using EventStore.Logging;

namespace Growthstories.UI
{
    public static class Mixins
    {

        private static readonly ILog Logger = LogFactory.BuildLogger(typeof(Mixins));


        public static ReactiveCommand ToCommandWithSubscription(this IObservable<bool> This, 
            Action<object> subscription, bool allowsConcurrentExecution = false, IScheduler scheduler = null)
        {
            var cmd = new ReactiveCommand(This, allowsConcurrentExecution, scheduler);
            cmd.Subscribe(subscription);
            return cmd;
        }


        public static async Task<Tuple<AllSyncResult, GSStatusCode?>>
            SyncAll(this IGSAppViewModel app, int maxRounds = 20)
        {
            try { app.SyncMutex.WaitOne(); }
            catch (Exception ex) { Logger.Warn("Mutex exception in SyncAll: " + ex.Message); };
            try
            {
                return await UnsafeSyncAll(app, maxRounds);
            }
            finally
            {
                app.SyncMutex.ReleaseMutex();
            }
        }


        private static async Task<Tuple<AllSyncResult, GSStatusCode?>> 
            UnsafeSyncAll(this IGSAppViewModel app, int maxRounds = 20)
        {
            int counter = 0;
            ISyncInstance R = null;
            GSStatusCode? nullResponseCode = null;

            while (counter < maxRounds)
            {
                R = await app.Synchronize();
                counter++;

                if (R.Status != SyncStatus.OK)
                {
                    return Tuple.Create(AllSyncResult.Error, nullResponseCode);
                }

                // TODO: check if there is more stuff to pull

                if (R.PushReq.IsEmpty)
                {
                    return Tuple.Create(AllSyncResult.AllSynced, nullResponseCode);
                }
            }

            return Tuple.Create(AllSyncResult.SomeLeft, nullResponseCode);
        }


        //
        // ONLY FOR TESTING
        // not necessarily thread safe, do not call from app code
        //
        public static async Task<Tuple<AllSyncResult, GSStatusCode?>> PushAll(this IGSAppViewModel app, int maxRounds = 20)
        {
            int counter = 0;
            ISyncInstance R = null;
            GSStatusCode? nullResponseCode = null;
            while (counter < maxRounds)
            {
                R = await app.Push();
                counter++;
                if (R == null) // there is nothing to do
                {
                    return Tuple.Create(AllSyncResult.AllSynced, nullResponseCode);
                }
                else if (R.PushReq.IsEmpty)
                {
                    return Tuple.Create(AllSyncResult.AllSynced, nullResponseCode);
                }
                else if (R.PushResp.StatusCode != GSStatusCode.OK)
                {
                    nullResponseCode = R.PushResp.StatusCode;
                    return Tuple.Create(AllSyncResult.Error, nullResponseCode);

                }
            }

            return Tuple.Create(AllSyncResult.SomeLeft, nullResponseCode);
        }


    }
}
