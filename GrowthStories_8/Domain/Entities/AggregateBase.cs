using Growthstories.WP8.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Growthstories.WP8.Domain.Entities
{

    public abstract class AggregateBase<TEvent> : Handler<TEvent>, IEventProvider<TEvent>
    {

        public List<TEvent> Changes { get; protected set; }

        public Guid Id { get; protected set; }
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
}
