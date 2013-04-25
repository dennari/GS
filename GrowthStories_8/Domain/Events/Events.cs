using Growthstories.WP8.Domain.Interfaces;
using System;


namespace Growthstories.WP8.Domain.Events
{
    public abstract class DomainEvent : IDomainEvent
    {
        public Guid Id { get; private set; }
        public Guid AggregateId { get; set; }
        int IDomainEvent.Version { get; set; }

        public DomainEvent()
        {
            Id = Guid.NewGuid();
        }
    }


}
