using EventStore;
using Growthstories.Core;
using System.Collections.Generic;


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
        ICollection<ISyncEventStream> In(IEnumerable<IEventDTO> enumerable);

    }
}
