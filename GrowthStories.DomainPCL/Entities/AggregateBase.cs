using Growthstories.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Growthstories.Domain.Entities
{

    public abstract class AggregateBase<TEvent> : IEventProvider<TEvent>, IStateful where TEvent : IEvent<IIdentity>
    {

        public IList<TEvent> Changes { get; protected set; }

        public IIdentity Id { get; protected set; }
        public int Version { get; protected set; }
        public int EventVersion { get; protected set; }



        public AggregateBase()
        {
            Changes = new List<TEvent>();
            Version = 0;
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
            //MethodInfo h;
            //if (!Handlers.TryGetValue(e.GetType(), out h))
            //{
            //    var s = string.Format("Failed to locate {0}.When({1})", this.GetType().Name, e.GetType().Name);
            //    throw new InvalidOperationException(s);
            //}
            //try
            //{
            //    h.Invoke(this, new object[] { e });
            //}
            //catch (TargetInvocationException ex)
            //{
            //    throw ex.InnerException;
            //}
            ((dynamic)this).When((dynamic)e);
            Version++;
        }

        protected void Apply(TEvent e)
        {
            Mutate(e);
            Changes.Add(e);
        }


        public abstract void ThrowOnInvalidStateTransition(ICommand<IIdentity> c);
    }



}
