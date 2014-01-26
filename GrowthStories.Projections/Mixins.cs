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


        /// <summary>
        /// Returns the plural form of the specified word.
        /// </summary>
        /// <param name="count">How many of the specified word there are. A count equal to 1 will not pluralize the specified word.</param>
        /// <returns>A string that is the plural form of the input parameter.</returns>
        public static string ToPlural(this string @this, int count, string plural = null)
        {
            return count == 1 ? @this : (plural != null ? plural : @this + "s");
        }
        //public static SemaphoreSlim SyncAllLock = new SemaphoreSlim(1);
    }
}
