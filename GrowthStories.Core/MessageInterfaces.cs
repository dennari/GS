using EventStore;
using System;
using System.Collections.Generic;
//using System.Collections.Specialized;
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

    public interface IAggregateMessages
    {
        Guid AggregateId { get; }
        IReadOnlyList<IMessage> Messages { get; }

        IGSAggregate Aggregate { get; set; }

    }

    public sealed class AggregateMessages : IAggregateMessages
    {
        private readonly List<IMessage> _Messages = new List<IMessage>();

        public Guid AggregateId { get; private set; }

        public IGSAggregate Aggregate { get; set; }


        public IReadOnlyList<IMessage> Messages
        {
            get { return _Messages; }
        }

        public AggregateMessages(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }

        public AggregateMessages(IGrouping<Guid, IMessage> msgs)
            : this(msgs.Key)
        {
            this.AddMessage(msgs);
        }

        public AggregateMessages(IGrouping<Guid, GSCommit> commits)
            : this(commits.Key)
        {

            foreach (var commit in commits)
                foreach (var e in commit.ActualEvents())
                    AddMessage(e);
        }

        public void AddMessage(IMessage msg)
        {
            if (msg.AggregateId != this.AggregateId)
                throw new ArgumentException("all the messages have to belong to the same aggregate");
            _Messages.Add(msg);
        }

        public void AddMessage(IEnumerable<IMessage> msgs)
        {
            foreach (var msg in msgs)
                AddMessage(msg);
        }


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

    public interface ICreateMessage : IMessage
    {
        Type AggregateType { get; }
    }



    public interface ICommand : IMessage
    {

    }

    public interface IAggregateCommand : ICommand
    {

        //Type AggregateType { get; }

        //DTOType StreamType { get; }
        Guid? AncestorId { get; set; }
        Guid? ParentAncestorId { get; set; }
        Guid? ParentId { get; set; }

        Guid? StreamEntityId { get; set; }
        Guid? StreamAncestorId { get; set; }
    }

}
