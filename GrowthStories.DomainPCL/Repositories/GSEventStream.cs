using EventStore;
using EventStore.Dispatcher;
using Growthstories.Core;
using Growthstories.Domain.Messaging;
using Growthstories.Domain;
using System.Linq;
using System;
using System.Collections.Generic;
using EventStore.Persistence;
using EventStore.Logging;

namespace Growthstories.Sync
{
    public class GSEventStream : OptimisticEventStream
    {


        private readonly IList<IEvent> Events = new List<IEvent>();


        private readonly GSEventStore Persistence;
        private static readonly ILog Logger = LogFactory.BuildLogger(typeof(GSEventStream));


        public GSEventStream(Guid streamId, GSEventStore persistence)
            : base(streamId, persistence)
        {

            this.Persistence = persistence;

        }

        //public override void Add(EventMessage uncommittedEvent)
        //{

        //}

        public void Add(IEvent e, bool setVersion = false)
        {
            var correctVersion = this.StreamRevision + this.Events.Count + 1;
            if (e.AggregateVersion != correctVersion)
            {
                if (!setVersion)
                    throw new InvalidOperationException(string.Format("SyncEventStream Add: event has version {0}, should have {1}", e.AggregateVersion, correctVersion));
                e.AggregateVersion = correctVersion;
            }
            this.Events.Add(e);
            base.Add(new EventMessage() { Body = e });
        }

        public override int GetHashCode()
        {
            return this.StreamId.GetHashCode();
        }
        public override bool Equals(object obj)
        {
            return this.Equals(obj as ISyncEventStream);
        }
        public virtual bool Equals(ISyncEventStream other)
        {
            return null != other && other.StreamId == this.StreamId;
        }

    }
}
