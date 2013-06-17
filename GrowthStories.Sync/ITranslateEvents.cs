using Growthstories.Core;
using Growthstories.Domain.Messaging;
using System.Collections.Generic;


namespace Growthstories.Sync
{
    public interface ITranslateEvents
    {
        //IEventDTO Out(IEvent msg);
        //IEvent In(IEventDTO msg);

        IEnumerable<IEventDTO> Out(IEnumerable<IDomainEvent> msgs);
        IEnumerable<IDomainEvent> In(IEnumerable<IEventDTO> enumerable);
    }
}
