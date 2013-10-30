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

    public interface IStreamSegment : ICollection<IMessage>
    {
        Guid AggregateId { get; }
        int AggregateVersion { get; }
        int TranslateOffset { get; set; }

        //IReadOnlyDictionary<Guid, IMessage> Messages { get; }

        IGSAggregate Aggregate { get; set; }
        ICreateMessage CreateMessage { get; }

        void MergeIncoming(IStreamSegment other);
        void MergeOutgoing(IStreamSegment other);


        void TrimDuplicates();
    }

    public sealed class StreamSegment : IStreamSegment
    {
        private readonly SortedDictionary<int, IMessage> Messages = new SortedDictionary<int, IMessage>();

        public Guid AggregateId { get; private set; }

        /// <summary>
        /// Gets the value which indicates the revision of the most recent event in this aggregate.
        /// </summary>
        public int AggregateVersion { get; private set; }

        public int TranslateOffset { get; set; }

        public IGSAggregate Aggregate { get; set; }

        public ICreateMessage CreateMessage { get; private set; }

        public StreamSegment(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }

        public StreamSegment(IGrouping<Guid, IMessage> msgs)
            : this(msgs.Key)
        {
            this.AddRange(msgs);
        }

        public StreamSegment(Guid aggregateId, IEnumerable<IMessage> msgs)
            : this(aggregateId)
        {
            this.AddRange(msgs);
        }

        //public AggregateMessages(IGrouping<Guid, GSCommit> commits)
        //    : this(commits.Key)
        //{

        //    foreach (var commit in commits)
        //        foreach (var e in commit.ActualEvents())
        //            AddMessage(e);
        //}

        bool eventMode = false;
        public void Add(IMessage msg)
        {
            if (msg.AggregateId != this.AggregateId)
                throw new ArgumentException("all the messages have to belong to the same aggregate");

            if (Messages.Count == 0)
            {
                var e = msg as IEvent;
                if (e != null)
                {
                    eventMode = true;
                    AggregateVersion = e.AggregateVersion - 1;
                }
                var c = msg as ICreateMessage;
                if (c != null)
                    CreateMessage = c;

            }
            if (eventMode)
                Add((IEvent)msg);
            else
            {
                Messages[Messages.Count] = msg;
            }

        }

        private void Add(IEvent e)
        {
            if (AggregateVersion != e.AggregateVersion - 1)
                throw new ArgumentException("Can't add event out of sequence");

            AggregateVersion = e.AggregateVersion;
            Messages[e.AggregateVersion] = e;
        }

        public void AddRange(IEnumerable<IMessage> msgs)
        {
            foreach (var msg in msgs)
                Add(msg);
        }


        public void TrimDuplicates()
        {
            if (this.Aggregate != null)
            {
                var l = this.Messages.Where(x => !this.Aggregate.State.IsDuplicate(x.Value)).Select(x => x.Key).ToArray();
                if (l.Length != this.Messages.Count)
                {
                    var numRemoved = l.Aggregate(0, (acc, k) => Messages.Remove(k) ? acc + 1 : acc + 0);
                    if (numRemoved != l.Length)
                        throw new InvalidOperationException("Couldn't remove all duplicates.");
                }

            }

        }


        public void MergeIncoming(IStreamSegment outgoing)
        {
            // just cross-updates the version numbers
            // which is enough in case of commutable events
            MergeOutgoing(outgoing);
            //other.MergeOut(this);

            // if we can, let's check for non-commutable events
            // and let the aggregate handle them one by one
            if (this.Aggregate != null)
            {
                var Outgoing = outgoing as StreamSegment;
                if (Outgoing == null)
                    return;
                foreach (var incomingKey in this.Messages.Keys.ToArray())
                {
                    var incomingValue = this.Messages[incomingKey];
                    if (incomingValue is INullEvent)
                        continue;
                    foreach (var outgoingKey in Outgoing.Messages.Keys.ToArray())
                    {
                        var outgoingValue = Outgoing.Messages[outgoingKey];
                        if (outgoingValue is INullEvent)
                            continue;

                        IMessage incomingNew, outgoingNew;
                        this.Aggregate.State.Merge(incomingValue, outgoingValue, out incomingNew, out outgoingNew);

                        if (incomingValue != incomingNew)
                            Messages[incomingKey] = incomingNew;
                        if (outgoingValue != outgoingNew)
                            Outgoing.Messages[outgoingKey] = outgoingNew;

                    }
                }
            }
        }



        public void MergeOutgoing(IStreamSegment incoming)
        {
            if (incoming.AggregateId != AggregateId)
                throw new ArgumentException("Can only merge events from the same stream");
            this.TranslateOffset = incoming.Count;

        }



        public void Clear()
        {
            throw new NotSupportedException();
        }

        public bool Contains(IMessage item)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(IMessage[] array, int arrayIndex)
        {
            Messages.Values.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return Messages.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(IMessage item)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<IMessage> GetEnumerator()
        {
            return this.Messages.Values.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.Messages.Values.GetEnumerator();
        }
    }

    public interface IEvent : IMessage//, IEnumerable<KeyValuePair<string, string>>
    {

        int AggregateVersion { get; set; }
        //IEvent ShallowCopy();
    }

    public interface INullEvent : IEvent
    {

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
