using Growthstories.Core;
using System.Collections.Generic;


namespace Growthstories.Sync
{
    public interface ITranslateEvents
    {
        IEventDTO Out(IEvent msg);
        IEvent In(IEventDTO msg);

        ICollection<IEventDTO> Out(IEnumerable<IEvent> msgs);

        ICollection<IEvent> In(IEnumerable<IEventDTO> enumerable);
    }
}
