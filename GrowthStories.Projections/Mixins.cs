using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Text;
using System.Threading.Tasks;

namespace Growthstories.UI
{
    public static class Mixins
    {


        public static ReactiveCommand ToCommandWithSubscription(this IObservable<bool> This, Action<object> subscription, bool allowsConcurrentExecution = false, IScheduler scheduler = null)
        {
            var cmd = new ReactiveCommand(This, allowsConcurrentExecution, scheduler);
            cmd.Subscribe(subscription);
            return cmd;
        }
    }
}
