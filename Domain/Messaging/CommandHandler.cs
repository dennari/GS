using Growthstories.Domain.Entities;
using Growthstories.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Growthstories.Domain.Messaging
{
    public class CommandHandler : Handler<IEvent>, ICommandHandler<UserId>, ICommandHandler<PlantId>
    {

        readonly IEventStore _store;

        public CommandHandler(IEventStore store)
            : base()
        {
            _store = store;
        }

        void Update(ICommand<UserId> c, Action<User> action)
        {
            var stream = _store.LoadEventStream(c.EntityId);
            var agg = new User(stream.Events);

            //using (Context.CaptureForThread())
            //{
            agg.ThrowOnInvalidStateTransition(c);
            action(agg);
            _store.AppendEventsToStream(c.EntityId, stream.StreamVersion, agg.Changes);
            //}
        }

        public void When(CreateUser c)
        {
            throw new NotImplementedException();
        }

        public void Execute(ICommand<UserId> c)
        {
            throw new NotImplementedException();
        }

        public void Execute(ICommand<PlantId> c)
        {
            throw new NotImplementedException();
        }
    }
}
