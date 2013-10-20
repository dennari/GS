

using EventStore;
using EventStore.Dispatcher;
using Growthstories.Core;
using ReactiveUI;
using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Disposables;

namespace Growthstories.Domain.Services
{
    public class MessageBusDispatcher : IDispatchCommits, IObservable<IEvent>
    {
        private readonly IMessageBus Mb;
        private IObserver<IEvent> Observer;

        public MessageBusDispatcher(IMessageBus mb)
        {
            this.Mb = mb;
            //this.Mb.Re
            this.Mb.RegisterMessageSource(this);

        }

        public void Dispatch(Commit commit)
        {

            if (this.Observer == null)
                return;
            foreach (var e in commit.Events.Select(x => (IEvent)x.Body))
                this.Observer.OnNext(e);
        }

        public void Dispose()
        {
            //throw new NotImplementedException();
        }

        public IDisposable Subscribe(IObserver<IEvent> observer)
        {
            this.Observer = observer;
            return Disposable.Create(() => this.Observer = null);
        }
    }
}
