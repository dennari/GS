using ReactiveUI;
using Growthstories.Core;
using System;
using System.Reactive;
using System.Reactive.Linq;

namespace Growthstories.Domain
{
    public static class Mixins
    {

        public static void DebugExceptionExtended(this IFullLogger This, string message, Exception exception)
        {
            if ((int)This.Level < (int)LogLevel.Debug)
                This.Debug(String.Format("{0}: {1}", message, exception.ToStringExtended()));
        }

        public static IObservable<Unit> AsCompletion<T>(this IObservable<T> observable)
        {
            return Observable.Create<Unit>(observer =>
            {
                Action onCompleted = () =>
                {
                    observer.OnNext(Unit.Default);
                    observer.OnCompleted();
                };
                return observable.Subscribe(_ => { }, observer.OnError, onCompleted);
            });
        }

        public static IObservable<TRet> ContinueAfter<T, TRet>(
          this IObservable<T> observable, Func<IObservable<TRet>> selector)
        {
            return observable.AsCompletion().SelectMany(_ => selector());
        }

    }
}
