using Growthstories.Core;


namespace Growthstories.Sync
{
    public interface ITranslateEvents
    {
        IEventDTO Out(IEvent msg);
        IEvent In(IEventDTO msg);
    }
}
