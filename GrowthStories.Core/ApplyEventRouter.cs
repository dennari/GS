using Growthstories.Core;
namespace Growthstories.Core
{
    using CommonDomain;


    public class ApplyEventRouter : IRouteEvents
    {
        private IAppliesEvents AggregateState;


        public ApplyEventRouter(IAppliesEvents aggregateState)
        {
            AggregateState = aggregateState;
        }



        public void Register<T>(System.Action<T> handler)
        {
            throw new System.NotImplementedException();
        }

        public void Register(object aggregate)
        {
            AggregateState = (IAppliesEvents)aggregate;
        }

        public void Dispatch(object eventMessage)
        {
            AggregateState.Apply((IEvent)eventMessage);
        }
    }
}