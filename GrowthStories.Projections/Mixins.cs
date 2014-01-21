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
using System.Threading;
using Enough.Async;


namespace Growthstories.UI
{

    public static class Mixins
    {

        private static readonly ILog Logger = LogFactory.BuildLogger(typeof(Mixins));


        public static ReactiveCommand ToCommandWithSubscription(this IObservable<bool> This,
            Action<object> subscription, bool allowsConcurrentExecution = false, System.Reactive.Concurrency.IScheduler scheduler = null)
        {
            var cmd = new ReactiveCommand(This, allowsConcurrentExecution, scheduler);
            cmd.Subscribe(subscription);
            return cmd;
        }

        //public static SemaphoreSlim SyncAllLock = new SemaphoreSlim(1);
    }
}
