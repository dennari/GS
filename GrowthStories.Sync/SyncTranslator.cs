using CommonDomain;
using EventStore;
using EventStore.Persistence;
using Growthstories.Core;
using Growthstories.Domain.Messaging;
//using Ninject;
using System;
using System.Collections.Generic;
using System.Linq;


namespace Growthstories.Sync
{
    public class SyncTranslator : ITranslateEvents
    {
        private readonly IAncestorFactory AncestorFactory;
        private readonly IPersistDeleteStreams Store;

        public SyncTranslator(IAncestorFactory ancestorFactory, IPersistDeleteStreams store)
        {
            this.AncestorFactory = ancestorFactory;
            this.Store = store;
        }

        public IMemento Ancestor { get; set; }

        public IEventDTO Out(IEvent e)
        {
            IEventDTO ed = null;
            ed = ((IDomainEvent)e).ToDTO();
            ed.AncestorId = AncestorFactory.GetAncestor().Id;
            var edd = ed as IAddEntityDTO;
            if (edd != null)
            {
                edd.ParentAncestorId = ed.AncestorId;
            }
            return ed;

        }


        public IEnumerable<IEventDTO> Out(IEnumerable<IEvent> events)
        {
            IEventDTO ed = null;
            foreach (var e in events)
            {
                try
                {
                    ed = Out(e);
                }
                catch (Exception) { }
                if (ed != null)
                {
                    yield return ed;
                }
            }
        }

        public IEnumerable<IEventDTO> Out(IEnumerable<ISyncEventStream> streams)
        {

            foreach (var stream in streams)
            {
                foreach (var e in Out(stream.Events.Cast<IDomainEvent>()))
                {
                    yield return e;
                }
            }

        }

        public IEvent In(IEventDTO dto)
        {

            return ((EventDTOUnion)dto).ToEvent();

        }

        public ICollection<ISyncEventStream> In(IEnumerable<IEventDTO> enumerable)
        {

            return enumerable
                .Select(x => In(x))
                .GroupBy(x => x.EntityId)
                .Select(x => new SyncEventStream(x, this.Store))
                .ToArray();


        }
    }
}
