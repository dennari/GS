using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using Growthstories.Core;
using EventStore;

namespace Growthstories.Sync
{
    public interface IEntityEventStream
    {
        Guid EntityId { get; }
        IEnumerable<IEvent> Events { get; }
    }

    public class EntityEventStream : IEntityEventStream
    {
        public Guid EntityId { get; private set; }
        public int EntityVersion { get; private set; }
        public IEnumerable<IEvent> Events { get; private set; }

        public EntityEventStream(IEnumerable<IEvent> events)
        {
            var g = events.GroupBy(x => x.EntityId);
            if (g.Count() != 1)
                throw new ArgumentException("");
            this.EntityVersion = events.Max(x => x.EntityVersion);
            this.EntityId = g.First().Key;
        }

        public EntityEventStream(IGrouping<Guid, IEvent> events)
        {
            this.Events = events.ToList();
            this.EntityVersion = events.Max(x => x.EntityVersion);
            this.EntityId = events.Key;
        }

        public EntityEventStream(IEventStream stream)
        {
            this.Events = stream.CommittedEvents.Select(x => (IEvent)x.Body);
            this.EntityVersion = stream.StreamRevision;
            this.EntityId = stream.StreamId;
        }


    }



}
