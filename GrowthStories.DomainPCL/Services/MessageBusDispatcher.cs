

using EventStore;
using EventStore.Dispatcher;
using Growthstories.Core;
using ReactiveUI;
using System.Linq;


namespace Growthstories.Domain.Services
{
    public class MessageBusDispatcher : IDispatchCommits
    {
        private readonly IMessageBus Mb;

        public MessageBusDispatcher(IMessageBus mb)
        {
            this.Mb = mb;
        }

        public void Dispatch(Commit commit)
        {
            foreach (var e in commit.Events.Select(x => (IEvent)x.Body))
                this.Mb.SendMessage(e);
        }

        public void Dispose()
        {
            //throw new NotImplementedException();
        }
    }
}
