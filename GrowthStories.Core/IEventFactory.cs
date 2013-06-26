
using System;


namespace Growthstories.Core
{
    public interface IEventFactory
    {
        T Create<T>() where T : IEvent;

        void Fill(IEvent @event, IGSAggregate aggregate);
    }

    public class EventFactory : IEventFactory
    {
        public T Create<T>() where T : IEvent
        {
            throw new NotImplementedException();
        }

        public void Fill(IEvent Event, IGSAggregate aggregate)
        {
            Event.Created = DateTimeOffset.Now;
            Event.EventId = Guid.NewGuid();
        }
    }

    public class FakeEventFactory : IEventFactory
    {

        public static Guid FakeEventId = Guid.Parse("10000000-0000-0000-0000-000000000000");
        public static DateTimeOffset FakeCreated = DateTimeOffset.MaxValue;


        public T Create<T>() where T : IEvent
        {
            throw new NotImplementedException();
        }

        public void Fill(IEvent Event, IGSAggregate aggregate)
        {
            Event.Created = FakeCreated;
            Event.EventId = FakeEventId;
        }
    }
}
