using Growthstories.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Growthstories.Domain.Entities
{

    public abstract class AggregateBase<TEvent> : Handler<TEvent>, IEventProvider<TEvent> where TEvent : IEvent
    {

        public IList<TEvent> Changes { get; protected set; }

        public IIdentity Id { get; protected set; }
        public int Version { get; protected set; }
        public int EventVersion { get; protected set; }



        public AggregateBase()
            : base()
        {
            Changes = new List<TEvent>();
        }

        public AggregateBase(IEnumerable<TEvent> events)
            : this()
        {
            foreach (var e in events)
            {
                Mutate(e);
            }
        }


        protected void Mutate(TEvent e)
        {
            Action<TEvent> h;
            if (!Handlers.TryGetValue(e.GetType(), out h))
            {
                var s = string.Format("Failed to locate {0}.When({1})", this.GetType().Name, e.GetType().Name);
                throw new InvalidOperationException(s);
            }
            try
            {
                h(e);
            }
            catch (TargetInvocationException ex)
            {
                throw ex.InnerException;
            }
        }

        protected void Apply(TEvent e)
        {
            Mutate(e);
            Changes.Add(e);
        }

    }

    public class DomainError : Exception
    {
        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //

        public DomainError() { }
        public DomainError(string message) : base(message) { }
        public DomainError(string format, params object[] args) : base(string.Format(format, args)) { }

        /// <summary>
        /// Creates domain error exception with a string name, that is easily identifiable in the tests
        /// </summary>
        /// <param name="name">The name to be used to identify this exception in tests.</param>
        /// <param name="format">The format.</param>
        /// <param name="args">The args.</param>
        /// <returns></returns>
        public static DomainError Named(string name, string format, params object[] args)
        {
            var message = "[" + name + "] " + string.Format(format, args);
            return new DomainError(message)
            {
                Name = name
            };
        }

        public string Name { get; private set; }

        public DomainError(string message, Exception inner) : base(message, inner) { }


    }

}
