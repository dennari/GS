using Growthstories.Domain.Entities;
using Growthstories.Domain.Interfaces;
using Microsoft.CSharp.RuntimeBinder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Growthstories.Domain.Messaging
{
    public class CommandHandler : ICommandHandler<IIdentity>
    {

        readonly IEventStore _store;

        public CommandHandler(IEventStore store)
            : base()
        {
            _store = store;
        }


        void DoIt(ICommand<IIdentity> c, Func<IEventStream, ICollection<IEvent<IIdentity>>> a)
        {
            var stream = _store.LoadStream(c.EntityId);
            _store.AppendToStream(c.EntityId, stream.StreamVersion, a(stream));

        }


        public void When(CreateUser c)
        {

            DoIt(c, (IEventStream stream) =>
            {
                var u = new User(stream.Events);
                u.ThrowOnInvalidStateTransition(c);
                u.Create(c.EntityId);
                return u.Changes;
            });
        }

        public void When(CreatePlant p)
        {
            DoIt(p, (IEventStream stream) =>
            {
                var u = new Plant(stream.Events);
                u.ThrowOnInvalidStateTransition(p);
                u.Create(p.EntityId);
                return u.Changes;
            });
        }

        public void When(CreateGarden g)
        {
            DoIt(g, (IEventStream stream) =>
            {
                var u = new Garden(stream.Events);
                u.ThrowOnInvalidStateTransition(g);
                u.Create(g.EntityId);
                return u.Changes;
            });
        }

        public void Execute(ICommand<IIdentity> e)
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

            try
            {
                ((dynamic)this).When((dynamic)e);
            }
            catch (RuntimeBinderException ee)
            {

            }
        }

    }
}
