using EventStore;
using Growthstories.Core;
using System;
using System.Collections.Generic;
using System.Linq;


namespace Growthstories.Sync
{
    public interface ITranslateEvents
    {
        //IEventDTO Out(IEvent msg);
        //IEvent In(IEventDTO msg);

        IEventDTO Out(IEvent @event);
        IEnumerable<IEventDTO> Out(IEnumerable<IEvent> events);
        IEnumerable<IEventDTO> Out(IEnumerable<ISyncEventStream> streams);

        IEvent In(IEventDTO dto);
        IGrouping<Guid, IEvent>[] In(IEnumerable<IEventDTO> enumerable);

    }
}
