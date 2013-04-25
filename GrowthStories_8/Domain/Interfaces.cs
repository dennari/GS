using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Growthstories.WP8.Domain.Interfaces
{
    interface ICommand
    {
        Guid Id { get; }
    }

    public interface ICommand<out TIdentity> : ICommand
    where TIdentity : IIdentity
    {
        TIdentity Id { get; }
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
        //string GetTag();

        //int GetStableHashCode();
    }

    interface IEvent
    {
        Guid Id { get; }
    }

    public interface IEvent<out TIdentity> : IEvent
    where TIdentity : IIdentity
    {
        TIdentity Id { get; }
    }

    public interface IDomainEvent
    {
        Guid Id { get; }
        Guid EntityId { get; }
        int Version { get; }
    }

    public interface IEventProvider<TDomainEvent> where TDomainEvent : IDomainEvent
    {

        IEnumerable<TDomainEvent> Changes { get; }
    }

}
