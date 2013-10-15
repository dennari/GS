using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Growthstories.Core
{
    public interface IMessage
    {
        Guid? EntityId { get; }
        Guid AggregateId { get; }
        DateTimeOffset Created { get; set; }
        Guid MessageId { get; set; }
    }

    public interface IEvent : IMessage//, IEnumerable<KeyValuePair<string, string>>
    {

        int AggregateVersion { get; set; }
        //IEvent ShallowCopy();
    }

    public interface IAggregateEvent<TState> : IEvent
        where TState : IAggregateState
    {
        TState AggregateState { get; set; }
    }

    public interface ICreateEvent : IEvent
    {
        Type AggregateType { get; }
    }



    public interface ICommand : IMessage
    {

    }

    public interface IAggregateCommand : ICommand
    {

        Type AggregateType { get; }

        //DTOType StreamType { get; }
        Guid? AncestorId { get; set; }
        Guid? ParentAncestorId { get; set; }
        Guid? ParentId { get; set; }

        Guid? StreamEntityId { get; set; }
        Guid? StreamAncestorId { get; set; }
    }

    public interface ICreateCommand : IAggregateCommand
    {

    }
}
