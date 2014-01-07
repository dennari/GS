using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Growthstories.Domain.Interfaces
{


    public interface ICommand<out TIdentity>
    where TIdentity : IIdentity
    {
        TIdentity EntityId { get; }
    }

    public interface ICommand : ICommand<IIdentity>
    {

    }


    public interface IIdentity
    {
        /// <summary>
        /// Gets the id, converted to a string. Only alphanumerics and '-' are allowed.
        /// </summary>
        /// <returns></returns>
        string ToString();

        /// <summary>
        /// Unique tag (should be unique within the assembly) to distinguish
        /// between different identities, while deserializing.
        /// </summary>
        string GetTag();
    }

    public interface IEvent<out TIdentity>
    where TIdentity : IIdentity
    {
        TIdentity EntityId { get; }
    }

    public interface IEvent : IEvent<IIdentity>
    {
    }

    public interface IDomainEvent : IEvent
    {
        int Version { get; }
    }

    public interface IEventProvider<T> where T : IEvent
    {

        IList<T> Changes { get; }
    }

    public interface IEventStore
    {
        IEventStream LoadEventStream(IIdentity id);
        void AppendEventsToStream(IIdentity id, long version, ICollection<IEvent> events);
    }

    public interface IEventStream
    {
        long StreamVersion { get; }
        IList<IEvent> Events { get; }
    }

    public interface ICommandHandler<T> where T : IIdentity
    {
        void Execute(ICommand<T> c);
    }

}
