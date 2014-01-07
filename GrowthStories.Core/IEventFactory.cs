
using System;


namespace Growthstories.Core
{
    public interface IEventFactory
    {

        void Fill(IEvent @event, IGSAggregate aggregate);
    }

    public class EventFactory : IEventFactory
    {

        public void Fill(IEvent Event, IGSAggregate aggregate)
        {
            Event.Created = DateTimeOffset.Now;
            Event.MessageId = Guid.NewGuid();
        }
    }

    public class FakeEventFactory : IEventFactory
    {

        public static Guid FakeEventId = Guid.Parse("10000000-0000-0000-0000-000000000000");
        public static DateTimeOffset FakeCreated = DateTimeOffset.MaxValue;


        public void Fill(IEvent Event, IGSAggregate aggregate)
        {
            Event.Created = FakeCreated;
            Event.MessageId = FakeEventId;
        }
    }
}
