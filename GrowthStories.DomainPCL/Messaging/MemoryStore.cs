using Growthstories.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Growthstories.Domain.Messaging
{
    public class EventStream : IEventStream
    {

        public long StreamVersion { get; set; }
        public IList<IEvent<IIdentity>> Events { get; set; }

        public EventStream()
        {

        }

    }

    public class InMemoryStore : IEventStore
    {
        public readonly List<IEvent<IIdentity>> Store = new List<IEvent<IIdentity>>();

        public IEventStream LoadStream(IIdentity id)
        {
            var events = Store.OfType<IEvent<IIdentity>>().Where(i => id.Equals(i.EntityId)).ToList();
            return new EventStream
            {
                Events = events,
                StreamVersion = events.Count
            };
        }

        public IEventStream LoadStream()
        {
            var events = Store;
            return new EventStream
            {
                Events = events,
                StreamVersion = events.Count
            };
        }

        public void AppendToStream(IIdentity id, long originalVersion, ICollection<IEvent<IIdentity>> events)
        {
            Store.AddRange(events);
        }
    }

}
